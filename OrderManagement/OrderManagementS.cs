using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using CIB.Exchange;
using CIB.Exchange.Model;

namespace CIB.OrderManagement
{
    public class OrderManagementS : IOrderManagement, IDisposable
    {
        private readonly IDictionary<string, IReactiveExchangeGateway> _route;
        private readonly IDisposable _subscription;
        private readonly Dictionary<long, Order> _orders = new Dictionary<long, Order>();

        public OrderManagementS(IDictionary<string, IReactiveExchangeGateway> route)
        {
            _route = route;

            var disposables = route.Values.Select(e => e.GetOrders().Subscribe(OnOrderStatus));
            _subscription = new CompositeDisposable(disposables);
        }

        public Order Create(string exchange, CurrencyPair tickr, Side side, decimal volume, OrderType limit, decimal price)
        {
            var order = OrderFactory.CreateLimitOrder(this, exchange, tickr, side, volume, price);
            _orders.Add(order.Id, order);
            return order;
        }

        public void Send(Order order)
        {
            var exchange = GetExchange(order);
            exchange.AddOrder(order);
        }

        private IReactiveExchangeGateway GetExchange(Order order)
        {
            IReactiveExchangeGateway exchangeGateway;
            if (!_route.TryGetValue(order.Exchange, out exchangeGateway))
                throw new NotImplementedException();
            return exchangeGateway;
        }

        public void Cancel(Order order)
        {
            var exchange = GetExchange(order);
            exchange.CancelOrder(order);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        private void OnOrderStatus(OrderStatus status)
        {
            Order order;
            if (!_orders.TryGetValue(status.OrderId, out order))
                return;
            order.ApplyStatusChange(status);
        }
    }
}