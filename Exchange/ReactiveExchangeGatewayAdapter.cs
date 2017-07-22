using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange
{
    public class ReactiveExchangeGatewayAdapter : IReactiveExchangeGateway
    {
        private readonly IExchangeGateway _gateaway;
        private readonly IEnumerable<CurrencyPair> _tickers;
        private readonly TimeSpan _dueTime;
        private readonly Subject<OrderStatus> _orderSubject = new Subject<OrderStatus>();

        public ReactiveExchangeGatewayAdapter(IExchangeGateway gateaway, IEnumerable<CurrencyPair> tickers)
        {
            if (gateaway == null) throw new ArgumentNullException(nameof(gateaway));
            _gateaway = gateaway;
            _tickers = tickers;
            _dueTime = TimeSpan.FromSeconds(15);
        }

        public IObservable<OrderBook> GetMarketData()
        {
            return Observable.Create<OrderBook>(observer =>
            {
                return Scheduler.Default.Schedule(TimeSpan.FromMilliseconds(1), self =>
                {
                    try
                    {
                        foreach (var ticker in _tickers)
                        {
                            var orderbook = _gateaway.GetOrderBook(ticker);
                            observer.OnNext(orderbook);
                        }
                    }
                    catch (JsonReaderException e)
                    {
                        // nop
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }

                    self(_dueTime);
                });
            });
        }

        public string Name => _gateaway.Name;
        public IObservable<AccountBalance> GetBalance()
        {
            return Observable.Create<AccountBalance>(observer =>
            {
                return Scheduler.Default.Schedule(self =>
                {
                    try
                    {
                        var balance = _gateaway.GetBalance();
                        observer.OnNext(balance);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }

                    //self(_dueTime);
                });
            });

        }

        public void AddOrder(Order order)
        {
            var orderstatus = _gateaway.AddOrder(order);
            _orderSubject.OnNext(orderstatus);
            // update balance and so on
        }

        public IObservable<OrderStatus> GetOrders()
        {
            return _orderSubject;
        }

        public void CancelOrder(Order order)
        {
            var orderstatus = _gateaway.CancelOrder(order);
            _orderSubject.OnNext(orderstatus);
        }
    }
}
