using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using CIB.Exchange.Model;
using log4net;

namespace CIB.Exchange
{
    public class Exchange : IDisposable
    {
        private readonly ILog Log = LogManager.GetLogger(typeof(Exchange));

        private readonly IReactiveExchangeGateway _reactiveExchangeGateway;
        private readonly Subject<Quote> _marketDataSubject = new Subject<Quote>();
        private IDisposable _marketDataSubscription;
        private IDisposable _balanceSubscription;

        private readonly Dictionary<CurrencyPair, OrderBook> _orderBooks = new Dictionary<CurrencyPair, OrderBook>();
        private AccountBalance _balance = new AccountBalance();

        public Exchange(IReactiveExchangeGateway reactiveExchangeGateway)
        {
            if (reactiveExchangeGateway == null) throw new ArgumentNullException(nameof(reactiveExchangeGateway));
            _reactiveExchangeGateway = reactiveExchangeGateway;
        }

        public void Dispose()
        {
            _marketDataSubscription?.Dispose();
            _balanceSubscription?.Dispose();
        }

        public void Subscribe()
        {
            IObservable<OrderBook> marketData = _reactiveExchangeGateway.GetMarketData();
            IObservable<AccountBalance> balance = _reactiveExchangeGateway.GetBalance();
            _marketDataSubscription = marketData.Subscribe(OnNewMarketData);
            _balanceSubscription = balance.Subscribe(OnBalance);
        }

        private void OnNewMarketData(OrderBook orderbook)
        {
            _orderBooks[orderbook.Ticker] = orderbook;
            var quote = GetQuote(orderbook);
            _marketDataSubject.OnNext(quote);
        }

        private void OnBalance(AccountBalance balance)
        {
            _balance = balance;
            //_balanceSubject.OnNext(balance);
            Log.Info("New balance on " + _reactiveExchangeGateway.Name + ":  " +
                     " BTC" + _balance.Bitcoin.ToString("F4") + 
                    ", ETH" + _balance.Ethereum.ToString("F4") +
                     " BCH" + _balance.Bitcoin.ToString("F4") +
                    ", EUR" + _balance.Euro.ToString("F2"));
        }

        private Quote GetQuote(OrderBook orderbook)
        {
            if (orderbook.Depth == 0)
                return null;
            var orderBookLevel = orderbook.GetLevel(1);
            var quote = new Quote(_reactiveExchangeGateway.Name, orderbook.Ticker, orderBookLevel.BidPrice, orderBookLevel.AskPrice, orderbook.LastUpdate);
            return quote;
        }

        public Quote GetLastQuote(CurrencyPair ticker)
        {
            OrderBook orderbook;
            if (_orderBooks.TryGetValue(ticker, out orderbook))
                return GetQuote(orderbook);
            return null;
        }

        public IObservable<Quote> GetMarketData()
        {
            return _marketDataSubject;
        }

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            return _orderBooks[ticker];
        }

        public decimal GetBalance(string currency)
        {
            return _balance.Get(currency);
        }
        
        public void SendOrder(Order order)
        {
            _reactiveExchangeGateway.AddOrder(order);
        }

        public IObservable<OrderStatus> GetOrdersStatus()
        {
            return _reactiveExchangeGateway.GetOrders();
        }

        public void CancelOrder(Order order)
        {
            _reactiveExchangeGateway.CancelOrder(order);
        }
    }
}