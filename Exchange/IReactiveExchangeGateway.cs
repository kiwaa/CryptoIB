using System;
using CIB.Exchange.Model;

namespace CIB.Exchange
{
    public interface IReactiveExchangeGateway
    {
        string Name { get; }
        IObservable<OrderBook> GetMarketData();
        IObservable<AccountBalance> GetBalance();
        IObservable<OrderStatus> GetOrders();
        void AddOrder(Order order);
        void CancelOrder(Order order);
    }
}
