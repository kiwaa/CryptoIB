using System.Collections.Generic;
using System.Reactive.Subjects;
using CIB.Exchange;
using CIB.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using CIB.OrderManagement;

namespace CIB.OrderManagement.Tests
{
    [TestClass]
    public class OrderManagementTests
    {
        private static readonly CurrencyPair TestPair = new CurrencyPair("TST", "TST");

        [TestMethod]
        public void Create_NewLimitOrder_StateNew()
        {
            // arrange
            Subject<OrderStatus> ordersSubject = new Subject<OrderStatus>();
            var exchange = Substitute.For<IReactiveExchangeGateway>();
            exchange.GetOrders().Returns(ordersSubject);
            var route = new Dictionary<string, IReactiveExchangeGateway>
            {
                { "Test", exchange }
            };
            var sut = new OrderManagementS(route);

            // act
            var order = sut.Create("Test", TestPair, Side.Bid, 1, OrderType.Limit, 100);

            // assert
            Assert.AreEqual(TestPair, order.Pair);
            Assert.AreEqual(Side.Bid, order.Side);
            Assert.AreEqual(1m, order.Volume);
            Assert.AreEqual(OrderType.Limit, order.Type);
            Assert.AreEqual(100m, order.Price);
            Assert.AreEqual(OrderState.New, order.State);
        }
    }
}
