using System.Collections.Generic;
using System.Collections.ObjectModel;
using CIB.Exchange.Model;

namespace CIB.OrderManagement.WebUI
{
    public class OrderStorage
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
}