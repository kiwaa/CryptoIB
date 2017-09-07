using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Dto;
using CIB.OrderManagement.WebUI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CIB.OrderManagement.WebUI
{
    public class OrderStorage
    {
        private readonly IHubContext<OrdersHub> _context;
        private List<Order> _list = new List<Order>();

        public OrderStorage(IHubContext<OrdersHub> context)
        {
            _context = context;
        }

        public void Add(Order order)
        {
            _list.Add(order);
            order.StateNotifications().Subscribe(x => OnNewState(order));
        }

        public IReadOnlyList<Order> GetAll()
        {
            return new ReadOnlyCollection<Order>(_list);
        }

        public Order Get(long orderId)
        {
            return _list.Find(x => x.Id == orderId);
        }


        private async void OnNewState(Order order)
        {
            var method = order.State == OrderState.New ? "New" : "Update";
            await _context.Clients.All.InvokeAsync(method, OrderDto.FromDomain(order));
        }
    }
}