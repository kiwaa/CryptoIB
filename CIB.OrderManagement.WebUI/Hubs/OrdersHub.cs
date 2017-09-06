using System;
using System.Linq;
using System.Threading.Tasks;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Dto;
using Microsoft.AspNetCore.SignalR;

namespace CIB.OrderManagement.WebUI.Hubs
{
    public class OrdersHub : Hub
    {
        private readonly IOrderManagement _management;
        private readonly OrderStorage _storage;

        public OrdersHub(IOrderManagement management, OrderStorage storage)
        {
            _management = management;
            _storage = storage;
        }

        public override async Task OnConnectedAsync()
        {
            var orderDtos = _storage.GetAll().Select(OrderDto.FromDomain);
            await Clients.Client(Context.ConnectionId).InvokeAsync("List", orderDtos);
            await base.OnConnectedAsync();
        }

        public void New(OrderDto orderDro)
        {
            var currencyPair = new CurrencyPair(orderDro.Currency, "EUR");
            Order order;
            if (orderDro.Type == OrderType.Limit)
            {
                order = _management.Create(orderDro.Exchange, currencyPair, orderDro.Side, orderDro.Volume, orderDro.Type, orderDro.Price.Value);
                _storage.Add(order);
                order.StateNotifications().Subscribe(x => OnNewState(order));
            }
            else
            {
                throw new NotImplementedException("Only Limit orders supported for now");
            }
            order.Send();
        }

        private async void OnNewState(Order order)
        {
            var method = order.State == OrderState.New ? "New" : "Update";
            await Clients.All.InvokeAsync(method, OrderDto.FromDomain(order));
        }

    }
}
