using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIB.Exchange.Model;
using Edge;
using log4net;

namespace CIB.Edge
{
    public class Exchange
    {
        private readonly ILog Log = LogManager.GetLogger(typeof(Exchange));
        private readonly IExchangeGateway _gateway;

        private List<Quote> _lastQuotes = new List<Quote>();
        public AccountBalance Balance { get; private set; } = new AccountBalance();
        public OrderBook OrderBook { get; private set; }

        public bool IsImplemented { get; } = false;
        public decimal Fees { get; set; }
        public string Name => _gateway.Name;
        public int Id { get; }

        public Exchange(int id, IExchangeGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException(nameof(gateway));
            Id = id;
            _gateway = gateway;
        }

        public void UpdateQuotes(IEnumerable<CurrencyPair> tickers)
        {
            var quotes = _gateway.GetQuote(tickers);

            foreach (var quote in quotes)
            {
                Log.Info("(" + quote.Pair.Ticker + ")   " + Name + ": \t" + quote.Ask.ToString("F2") + " / " + quote.Bid.ToString("F4"));
            }

            _lastQuotes = quotes;
        }

        public Quote GetLastQuote(CurrencyPair ticker)
        {
            var quote = _lastQuotes.SingleOrDefault(q => q.Pair == ticker);
            VerifyQuote(quote);
            return quote;
        }

        // If there is an error with the bid or ask (i.e. value is null), throws exception
        private void VerifyQuote(Quote quote)
        {
            if (quote.Bid == 0)
            {
                Log.Warn(Name + " bid is null");
                throw new InvalidDataException("Invlaid bid");
            }
            if (quote.Ask == 0)
            {
                Log.Warn(Name + " ask is null");
                throw new InvalidDataException("Invlaid bid");
            }
        }

        public void UpdateBalance()
        {
            // Gets the the balances from every exchange
            // This is only done when not in Demo mode.
            Balance = _gateway.GetBalance();

            Log.Info("New balance on " + Name + ":  \tB" +
                Balance.Bitcoin.ToString("F4") + ", E" +
                Balance.Ethereum.ToString("F4") + ", $" +
                Balance.Euro.ToString("F2"));
        }

        public decimal GetLimitPrice(Configuration conf, CurrencyPair ticker, decimal volume, bool isBid)
        {
            decimal totVol = 0.0m;
            decimal currPrice = 0;

            var orderbook = _gateway.GetOrderBook(ticker);
            if (orderbook == null)
                return 0;

            if (isBid)
            {
                for (int i = 1; i < orderbook.Depth; i++)
                {
                    // volumes are added up until the requested volume is reached
                    var currVol = orderbook.GetLevel(i).BidVolume;
                    currPrice = orderbook.GetLevel(i).BidPrice;
                    totVol += currVol;
                    if (totVol >= volume * conf.OrderBookFactor)
                        break;
                }
            }
            else
            {
                for (int i = 1; i < orderbook.Depth; i++)
                {
                    // volumes are added up until the requested volume is reached
                    var currVol = orderbook.GetLevel(i).AskVolume;
                    currPrice = orderbook.GetLevel(i).AskPrice;
                    totVol += currVol;
                    if (totVol >= volume * conf.OrderBookFactor)
                        break;
                }
            }

            return currPrice;
        }
        
        public void SendSellOrder(CurrencyPair ticker, decimal volume, decimal price)
        {
            var order = new Order(ticker, Side.Sell, volume, OrderType.Limit, price);
            Log.Info("Send Order " + order.Pair.Ticker + " " + order.Volume + " @ " + order.Price);
            _gateway.AddOrder(order);
        }

        public void SendBuyOrder(CurrencyPair ticker, decimal volumeLong, decimal limPriceLong)
        {
            var order = new Order(ticker, Side.Buy, volumeLong, OrderType.Limit, limPriceLong);
            Log.Info("Send Order " + order.Pair.Ticker + " " + order.Volume + " @ " + order.Price);
            _gateway.AddOrder(order);
        }

        public void UpdateOrderBook(CurrencyPair ticker)
        {
            var orderbook = _gateway.GetOrderBook(ticker);

            OrderBook = orderbook;
        }
    }
}
