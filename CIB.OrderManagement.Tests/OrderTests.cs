using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using CIB.Exchange;
using CIB.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace CIB.OrderManagement.Tests
{
    [TestClass]
    public class OrderTests
    {
        private static readonly CurrencyPair TestPair = new CurrencyPair("TST", "TST");

        [TestMethod]
        public void Send_SendLimitOrderAndAcceptByExchange_Success()
        {
            // arrange
            var exchange = "Test";
            var fixture = new Fixture();
            var sut = fixture.CreateSut(exchange, id => new OrderStatus(id, Guid.NewGuid().ToString(), OrderState.Accepted));
            var order = sut.Create(exchange, TestPair, Side.Buy, 1, OrderType.Limit, 100);

            // act
            order.Send();

            // assert
            Assert.AreEqual(OrderState.Accepted, order.State);
        }

        [TestMethod]
        public void Send_SendLimitOrderAndRejectedByExchange_Success()
        {
            // arrange
            var exchange = "Test";
            var fixture = new Fixture();
            var sut = fixture.CreateSut(exchange, id => new OrderStatus(id, null, OrderState.RejectedByExchange, "No"));
            var order = sut.Create("Test", TestPair, Side.Buy, 1, OrderType.Limit, 100);

            // act
            order.Send();

            // assert
            Assert.AreEqual(OrderState.RejectedByExchange, order.State);
            Assert.AreEqual("No", order.ErrorMessage);
        }

        [TestMethod]
        public void Send_CancelAcceptedLimitOrder_Success()
        {
            // arrange
            var exchange = "Test";
            var fixture = new Fixture();
            var sut = fixture.CreateSut(exchange, 
                id => new OrderStatus(id, Guid.NewGuid().ToString(), OrderState.Accepted),
                (id, eid) => new OrderStatus(id, eid, OrderState.Cancelled));
            var order = sut.Create("Test", TestPair, Side.Buy, 1, OrderType.Limit, 100);
            order.Send();

            // act
            order.Cancel();

            // assert
            Assert.AreEqual(OrderState.Cancelled, order.State);
        }

        [TestMethod]
        public void Send_CancelAcceptedLimitOrder_CancelPending()
        {
            // arrange
            var exchange = "Test";
            var fixture = new Fixture();
            var sut = fixture.CreateSut(exchange,
                id => new OrderStatus(id, Guid.NewGuid().ToString(), OrderState.Accepted));
            var order = sut.Create("Test", TestPair, Side.Buy, 1, OrderType.Limit, 100);
            order.Send();

            // act
            order.Cancel();

            // assert
            Assert.AreEqual(OrderState.CancelPending, order.State);
        }

        [TestMethod]
        public void StateNotifications_Send_Notify()
        {
            // arrange
            bool wasCalled = false;
            var exchange = "Test";
            var fixture = new Fixture();
            var sut = fixture.CreateSut(exchange, id => new OrderStatus(id, Guid.NewGuid().ToString(), OrderState.Accepted));
            var order = sut.Create("Test", TestPair, Side.Buy, 1, OrderType.Limit, 100);
            order.StateNotifications().Subscribe(state => wasCalled = true);
            
            // act
            order.Send();

            // assert
            Assert.AreEqual(true, wasCalled);
        }


        // reject cancel of new order immediately

        private class Fixture
        {
            public OrderManagementS CreateSut(string exchangeName, Func<long, OrderStatus> onAdd = null, Func<long, string, OrderStatus> onCancel = null)
            {
                Subject<OrderStatus> ordersSubject = new Subject<OrderStatus>();
                var exchange = Substitute.For<IReactiveExchangeGateway>();
                exchange.GetOrders().Returns(ordersSubject);
                exchange.When(e => e.AddOrder(Arg.Any<Order>())).Do(x =>
                {
                    if (onAdd != null)
                        ordersSubject.OnNext(onAdd(x.Arg<Order>().Id));
                });
                exchange.When(e => e.CancelOrder(Arg.Any<Order>())).Do(x =>
                {
                    if (onCancel != null)
                        ordersSubject.OnNext(onCancel(x.Arg<Order>().Id, x.Arg<Order>().ExchangeOrderId));
                });
                return new OrderManagementS(new Dictionary<string, IReactiveExchangeGateway>
                {
                    { exchangeName, exchange }
                });
            }
        }
    }
}
