using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using CIB.Exchange.Model;

namespace CIB.Exchange
{
    public class DemoReactiveExchangeGateway : IReactiveExchangeGateway
    {
        private readonly IReactiveExchangeGateway _reactiveExchangeGateway;
        private readonly BehaviorSubject<AccountBalance> _balanceSubject;
        private readonly Subject<OrderStatus> _orderSubject = new Subject<OrderStatus>();
        private int _orderId = 1;

        public DemoReactiveExchangeGateway(IReactiveExchangeGateway reactiveExchangeGateway)
        {
            if (reactiveExchangeGateway == null) throw new ArgumentNullException(nameof(reactiveExchangeGateway));
            _reactiveExchangeGateway = reactiveExchangeGateway;
            _balanceSubject = new BehaviorSubject<AccountBalance>(new AccountBalance
            {
                Bitcoin = 0.1m,
                Ethereum = 1m,
                Euro = 200m
            });
        }

        public string Name => _reactiveExchangeGateway.Name;

        public IObservable<OrderBook> GetMarketData()
        {
            return _reactiveExchangeGateway.GetMarketData();
        }

        public IObservable<AccountBalance> GetBalance()
        {
            return _balanceSubject;
        }

        public void AddOrder(Order order)
        {
            var balance = _balanceSubject.Value;

            if (order.Side == Side.Bid)
            {
                AddToBalance(balance, order.Pair.Base, order.Volume);
                AddToBalance(balance, order.Pair.Quote, -order.Price * order.Volume);
            }
            else
            {
                AddToBalance(balance, order.Pair.Base, -order.Volume);
                AddToBalance(balance, order.Pair.Quote, order.Price * order.Volume);
            }
            VerifyBalance(balance);
            _balanceSubject.OnNext(balance);
            _orderSubject.OnNext(new OrderStatus(order.Id, (_orderId++).ToString(), true));
        }

        private void VerifyBalance(AccountBalance balance)
        {
            if (balance.Bitcoin < 0)
                throw new InvalidOperationException("Negative balance");
            if (balance.Ethereum < 0)
                throw new InvalidOperationException("Negative balance");
            if (balance.Litecoin < 0)
                throw new InvalidOperationException("Negative balance");
            if (balance.DigitalCash < 0)
                throw new InvalidOperationException("Negative balance");
            if (balance.Euro < 0)
                throw new InvalidOperationException("Negative balance");
            if (balance.Dollars < 0)
                throw new InvalidOperationException("Negative balance");
        }

        private void AddToBalance(AccountBalance balance, string currency, decimal amount)
        {
            switch (currency.ToUpper())
            {
                case "BTC":
                    balance.Bitcoin += amount;
                    break;
                case "LTC":
                    balance.Litecoin += amount;
                    break;
                case "ETH":
                    balance.Ethereum += amount;
                    break;
                case "EUR":
                    balance.Euro += amount;
                    break;
                case "USD":
                    balance.Dollars += amount;
                    break;
                case "DSH":
                    balance.DigitalCash += amount;
                    break;
                default:
                    Debug.Fail("Unknown balance currency");
                    throw new NotImplementedException();
            }
        }

        public IObservable<OrderStatus> GetOrders()
        {
            return _orderSubject;
        }

        public void CancelOrder(Order order)
        {
            var balance = _balanceSubject.Value;

            if (order.Side == Side.Ask)
            {
                AddToBalance(balance, order.Pair.Base, order.Volume);
                AddToBalance(balance, order.Pair.Quote, -order.Price * order.Volume);
            }
            else
            {
                AddToBalance(balance, order.Pair.Base, -order.Volume);
                AddToBalance(balance, order.Pair.Quote, order.Price * order.Volume);
            }
            VerifyBalance(balance);
            _balanceSubject.OnNext(balance);
            _orderSubject.OnNext(new OrderStatus(order.Id, order.ExchangeOrderId, true, true));
        }
    }
}