using System;
using System.Collections.Generic;
using System.Diagnostics;
using CIB.Exchange.Model;
using log4net;

namespace CIB.Exchange.Demo
{
    public class DemoExchangeGateway : IExchangeGateway
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DemoExchangeGateway));
        private readonly IExchangeGateway _internalGateway;
        private readonly AccountBalance _balance;
        private int _orderId = 1;

        public string Name => _internalGateway.Name;

        public DemoExchangeGateway(IExchangeGateway internalGateway)
        {
            if (internalGateway == null) throw new ArgumentNullException(nameof(internalGateway));
            _internalGateway = internalGateway;

            // demo mode
            Log.Info($"[Demo mode]: {Name} balance faked");
            Log.Info("[Demo mode]: trades won't be generated");

            _balance = new AccountBalance()
            {
                Bitcoin = 0.1m,
                Ethereum = 1m,
                Euro = 200m
            };
        }

        public List<Quote> GetQuote(IEnumerable<CurrencyPair> ticker)
        {
            return _internalGateway.GetQuote(ticker);
        }

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            return _internalGateway.GetOrderBook(ticker);
        }

        public AccountBalance GetBalance()
        {
            return _balance;
        }

        public OrderStatus AddOrder(Order order)
        {
            Log.Info("[Demo mode]: order " + order.Pair.Ticker + " " + order.Volume + " @ " + order.Price);

            if (order.Side == Side.Bid)
            {
                AddToBalance(order.Pair.Base, order.Volume);
                AddToBalance(order.Pair.Quote, -order.Price * order.Volume);
            }
            else
            {
                AddToBalance(order.Pair.Base, -order.Volume);
                AddToBalance(order.Pair.Quote, order.Price * order.Volume);
            }
            VerifyBalance();

            return new OrderStatus(order.Id, (_orderId++).ToString(), true);
        }

        private void VerifyBalance()
        {
            if (_balance.Bitcoin < 0)
                throw new InvalidOperationException("Negative balance");
            if (_balance.Ethereum < 0)
                throw new InvalidOperationException("Negative balance");
            if (_balance.Litecoin < 0)
                throw new InvalidOperationException("Negative balance");
            if (_balance.DigitalCash < 0)
                throw new InvalidOperationException("Negative balance");
            if (_balance.Euro < 0)
                throw new InvalidOperationException("Negative balance");
            if (_balance.Dollars < 0)
                throw new InvalidOperationException("Negative balance");
        }

        private void AddToBalance(string currency, decimal amount)
        {
            switch (currency.ToUpper())
            {
                case "BTC":
                    _balance.Bitcoin += amount;
                    break;
                case "LTC":
                    _balance.Litecoin += amount;
                    break;
                case "ETH":
                    _balance.Ethereum += amount;
                    break;
                case "EUR":
                    _balance.Euro += amount;
                    break;
                case "USD":
                    _balance.Dollars += amount;
                    break;
                case "DSH":
                    _balance.DigitalCash += amount;
                    break;
                default:
                    Debug.Fail("Unknown balance currency");
                    throw new NotImplementedException();
            }
        }

        public OrderStatus CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
