using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CIB.Exchange.Coinbase.DTO;
using CIB.Exchange.Coinbase.Messaging;
using CIB.Exchange.Model;

namespace CIB.Exchange.Coinbase
{
    internal class CoinbaseRealTimeOrderBook
    {
        public OrderBook ToDomain()
        {
            var orderBookBids = _bids.Select(t => new List<decimal>{ t.Price, t.Volume }).ToList();
            var orderBookAsks = _asks.Select(t => new List<decimal> { t.Price, t.Volume }).ToList();
            return new OrderBook(CoinbaseConverter.ConvertTicker(_product), orderBookBids, orderBookAsks, DateTime.UtcNow);
        }

        public static CoinbaseRealTimeOrderBook FromDto(CurrencyPair ticker, CoinbaseFullOrderBook dto)
        {
            var book = new CoinbaseRealTimeOrderBook();
            book._sequence = dto.sequence;
            book._bids = dto.bids.Select(o => new OrderBookOrder(o[2], decimal.Parse(o[0]), decimal.Parse(o[1])))
                    .GroupBy(o => o.Price, o => o)
                    .Select(g => new OrderBookPosition(g))
                    .OrderByDescending(p => p.Price)
                    .ToList();
            book._asks = dto.asks.Select(o => new OrderBookOrder(o[2], decimal.Parse(o[0]), decimal.Parse(o[1])))
                    .GroupBy(o => o.Price, o => o)
                    .Select(g => new OrderBookPosition(g))
                    .OrderBy(p => p.Price)
                    .ToList();
            book._product = CoinbaseConverter.ConvertTicker(ticker);
            return book;
        }

        private long _sequence = -1;

        private List<OrderBookPosition> _bids = new List<OrderBookPosition>();
        private List<OrderBookPosition> _asks = new List<OrderBookPosition>();
        private string _product;

        private CoinbaseRealTimeOrderBook()
        {
        }

        private sealed class OrderBookPosition : List<OrderBookOrder>
        {
            public decimal Price { get; private set; }

            public decimal Volume => this.Sum(o => o.Volume);

            public OrderBookPosition(IEnumerable<OrderBookOrder> orders) : base(orders)
            {
                Price = orders.First().Price;
            }

            public void Match(OrderBookOrder order)
            {
                var bookOrder = this.FirstOrDefault(o => o.Id == order.Id);

                if (bookOrder != null)
                    bookOrder.ChangeVolume(-order.Volume);
            }

            public void Done(OrderBookOrder order)
            {
                var bookOrder = this.FirstOrDefault(o => o.Id == order.Id);

                if (bookOrder != null)
                {
                    Debug.Assert(bookOrder.Volume == order.Volume);
                    this.Remove(bookOrder);
                }
            }
        }

        private sealed class OrderBookOrder
        {
            public string Id { get; }
            public decimal Price { get; }
            public decimal Volume { get; private set; }

            public OrderBookOrder(string id, decimal price, decimal volume)
            {
                Id = id;
                Price = price;
                Volume = volume;
            }

            public void ChangeVolume(decimal volume)
            {
                Volume += volume;
            }
        }

        internal void Update(OrderBookReceived received)
        {
            if (received.sequence - _sequence == 1)
                _sequence++;
        }

        internal void Update(OrderBookDone done)
        {
            if (done.sequence - _sequence == 1)
            {
                _sequence = done.sequence;
                if (done.side == "sell")
                {
                    for (int i = 0; i < _asks.Count; i++)
                    {
                        if (_asks[i].Price == done.price)
                        {
                            _asks[i].Done(new OrderBookOrder(done.order_id, done.price, done.remaining_size));
                            if (_asks[i].Volume == 0m)
                            {
                                _asks.RemoveAt(i);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _bids.Count; i++)
                    {
                        if (_bids[i].Price == done.price)
                        {
                            _bids[i].Done(new OrderBookOrder(done.order_id, done.price, done.remaining_size));
                            if (_bids[i].Volume == 0m)
                            {
                                _bids.RemoveAt(i);
                            }
                            break;
                        }
                    }
                }
                return;
            }
            if (done.sequence <= _sequence)
                return;
            throw new NotImplementedException();
        }


        internal void Update(OrderBookMatch match)
        {
            if (match.sequence - _sequence == 1)
            {
                _sequence = match.sequence;
                if (match.side == "sell")
                {
                    for (int i = 0; i < _asks.Count; i++)
                    {
                        if (_asks[i].Price == match.price)
                        {
                            _asks[i].Match(new OrderBookOrder(match.maker_order_id, match.price, match.size));
                            if (_asks[i].Volume == 0m)
                            {
                                _asks.RemoveAt(i);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _bids.Count; i++)
                    {
                        if (_bids[i].Price == match.price)
                        {
                            _bids[i].Match(new OrderBookOrder(match.maker_order_id, match.price, match.size));
                            if (_bids[i].Volume == 0m)
                            {
                                _bids.RemoveAt(i);
                            }
                            break;
                        }
                    }
                }
                return;
            }
            if (match.sequence - _sequence <= 0)
            {
                return;
            }

            throw new NotImplementedException();
        }


        internal void Update(OrderBookOpen open)
        {
            if (open.sequence - _sequence == 1)
            {
                _sequence = open.sequence;
                if (open.side == "sell")
                {
                    //var ask = new List<decimal> { open.price, open.remaining_size };
                    var order = new OrderBookOrder(open.order_id, open.price, open.remaining_size);
                    for (int i = 0; i < _asks.Count; i++)
                    {
                        if (_asks[i].Price == open.price)
                        {
                            _asks[i].Add(order);
                            break;
                        }
                        if (_asks[i].Price > open.price)
                        {
                            _asks.Insert(i, new OrderBookPosition(new [] { order }));
                            break;
                        }
                        if (i == _asks.Count - 1)
                        {
                            _asks.Add(new OrderBookPosition(new [] { order }));
                            break;
                        }
                    }
                }
                else
                {
                    var order = new OrderBookOrder(open.order_id, open.price, open.remaining_size);
                    for (int i = 0; i < _bids.Count; i++)
                    {
                        if (_bids[i].Price == open.price)
                        {
                            _bids[i].Add(order);
                            break;
                        }
                        if (_bids[i].Price < open.price)
                        {
                            _bids.Insert(i, new OrderBookPosition(new [] { order }));
                            break;
                        }
                        if (i == _bids.Count - 1)
                        {
                            _bids.Add(new OrderBookPosition(new [] { order }));
                            break;
                        }
                    }
                }
                return;
            }
            if (open.sequence - _sequence <= 0)
            {
                return;
            }
            throw new NotImplementedException();
        }

        public decimal AskPrice => _asks.First().Price;
        public decimal BidPrice => _bids.First().Price;
    }


}
