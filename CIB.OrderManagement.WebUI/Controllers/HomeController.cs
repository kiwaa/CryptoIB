using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using CIB.Exchange;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using Microsoft.AspNetCore.Mvc;

namespace CIB.OrderManagement.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private static readonly OrderStorage _storage = new OrderStorage();
        private readonly IOrderManagement _management;

        public HomeController(IOrderManagement management)
        {
            _management = management;
        }

        public IActionResult Index()
        {
            return View(_storage.GetAll());
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewOrder(OrderDto orderDro)
        {
            var currencyPair = new CurrencyPair(orderDro.Currency, "EUR");
            var order = _management.Create(orderDro.Exchange, currencyPair, orderDro.Side, orderDro.Volume, orderDro.Type, orderDro.Price);
            _storage.Add(order);
            order.Send();
            return RedirectToAction("Index");
        }

        public IActionResult Cancel(long orderId)
        {
            var order = _storage.Get(orderId);
            if (order == null)
                return NotFound();
            order.Cancel();
            return RedirectToAction("Index");
        }

    }

    internal class OrderStorage
    {
        private List<Order> _list = new List<Order>();

        public void Add(Order order)
        {
            _list.Add(order);
        }

        public IReadOnlyList<Order> GetAll()
        {
            return new ReadOnlyCollection<Order>(_list);
        }

        public Order Get(long orderId)
        {
            return _list.Find(x => x.Id == orderId);
        }
    }

    public class OrderDto
    {
        public string Exchange { get; set; }
        public string Currency { get; set; }
        public Side Side { get; set; }

        public decimal Volume { get; set; }
        public OrderType Type { get; set; }
        public decimal Price { get; set; }
    }
}
