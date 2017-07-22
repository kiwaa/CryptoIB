using System;
using System.Collections.Generic;
using System.Linq;
using CIB.Edge;
using CIB.Exchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Edge.Tests
{
    // simple integration tests till I figure out how to use specflow or something else

    [TestClass]
    public class EdgeTests
    {
        // for 2 exchanges and pair BTCEUR arbitrage opportunity exists
        [TestMethod]
        public void EthEurArbirtrageTest()
        {
            // Arrange
            var pair = new CurrencyPair("ETH", "EUR");
            var pairs = new List<CurrencyPair>()
            {
                pair
            };
            AccountBalance balance1 = new AccountBalance()
            {
                Ethereum = 1,
                Euro = 200
            };
            AccountBalance balance2 = new AccountBalance()
            {
                Ethereum = 1,
                Euro = 200
            };
            OrderBook orderbook1 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(187.28268m, 0.1336923m, 186.25251m, 0.48655646m),
                new OrderBookLevel(187.37171m, 0.4840898m, 185.94149m, 0.1291425m),
                new OrderBookLevel(188.02748m, 0.97929806m, 185.6029m, 0.48777242m),
            }, DateTime.UtcNow);
            OrderBook orderbook2 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(193.60000m, 61.440m, 192.87051m, 3.227m),
                new OrderBookLevel(193.60674m, 2.175m, 192.87004m, 1.147m),
                new OrderBookLevel(193.60675m, 1.755m, 192.87000m, 14.720m),
            }, DateTime.UtcNow);

            IExchangeGateway exchange1 = Substitute.For<IExchangeGateway>();
            exchange1.GetQuote(pairs).Returns(new []
            {
                new Quote("Test1", pair, 186.25251m, 187.28268m, DateTime.UtcNow)
            }.ToList());
            exchange1.GetBalance().Returns(balance1);
            exchange1.GetOrderBook(pair).Returns(orderbook1);
            IExchangeGateway exchange2 = Substitute.For<IExchangeGateway>();
            exchange2.GetQuote(pairs).Returns(new []
            {
                new Quote("Test2", pair, 192.87051m , 193.60000m, DateTime.UtcNow)
            }.ToList());
            exchange2.GetBalance().Returns(balance2);
            exchange2.GetOrderBook(pair).Returns(orderbook2);
            int id = 0;
            var exchanges = new List<Exchange>(new[] { exchange1, exchange2 }.Select(e => new Exchange(id++, e)));
            var conf = new Configuration()
            {
                Pairs = pairs
            };

            var edgeRunner = new EdgeWorker();
            edgeRunner.Initialize(conf, exchanges);

            // Act
            edgeRunner.Run();

            // Assert
            Order expectedSellOrder = new Order(pair, Side.Ask, 0.25020240m, OrderType.Limit, 192.87051m);
            Order expectedBuyOrder = new Order(pair, Side.Bid, 0.25020240m, OrderType.Limit, 188.02748m);
            exchange2.Received(1).AddOrder(expectedSellOrder);
            exchange1.Received(1).AddOrder(expectedBuyOrder);
        }

        // for 2 exchanges and pair BTCEUR rebalance opportunity exists
        [TestMethod]
        public void BtcEurRebalanceTest()
        {
            // Arrange
            var pair = new CurrencyPair("BTC", "EUR");
            var pairs = new List<CurrencyPair>()
            {
                pair
            };
            AccountBalance balance1 = new AccountBalance()
            {
                Bitcoin = 2,
                Euro = 0
            };
            AccountBalance balance2 = new AccountBalance()
            {
                Bitcoin = 0.25m,
                Euro = 4000
            };
            OrderBook orderbook1 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(2406.390m, 3.164647m, 2406.38m, 2.71m),
                new OrderBookLevel(2406.490m, 0.082933m, 2406.37m, 0.5m),
                new OrderBookLevel(2407.850m, 0.465566154m, 2406.36m, 0.01m),
            }, DateTime.UtcNow);
            OrderBook orderbook2 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(2406.200m,  35.133m, 2405.310m, 0.384m),
                new OrderBookLevel(2406.579m,  0.002m, 2404.459m, 0.400m),
                new OrderBookLevel(2409.889m,  0.600m, 0.037m, 2404.458m),
            }, DateTime.UtcNow);
            IExchangeGateway exchange1 = Substitute.For<IExchangeGateway>();
            exchange1.GetQuote(pairs).Returns(new []
            {
                new Quote("Test", pair, 2406.38m, 2406.390m, DateTime.UtcNow)
            }.ToList());
            exchange1.GetBalance().Returns(balance1);
            exchange1.GetOrderBook(pair).Returns(orderbook1);
            IExchangeGateway exchange2 = Substitute.For<IExchangeGateway>();
            exchange2.GetQuote(pairs).Returns(new[]
            {
                new Quote("Test", pair, 2405.310m,2406.200m, DateTime.UtcNow)
            }.ToList());
            exchange2.GetBalance().Returns(balance2);
            exchange2.GetOrderBook(pair).Returns(orderbook2);
            int id = 0;
            var exchanges = new List<Exchange>(new[] { exchange1, exchange2 }.Select(e => new Exchange(id++, e)));
            var conf = new Configuration()
            {
                Pairs = pairs
            };

            var edgeRunner = new EdgeWorker();
            edgeRunner.Initialize(conf, exchanges);
            
            // Act
            edgeRunner.Run();

            // Assert
            Order expectedSellOrder = new Order(pair, Side.Ask, 0.875m, OrderType.Limit, 2406.36m);
            Order expectedBuyOrder = new Order(pair, Side.Bid, 0.875m, OrderType.Limit, 2406.200m);
            exchange1.Received(1).AddOrder(expectedSellOrder);
            exchange2.Received(1).AddOrder(expectedBuyOrder);
        }

        // for 2 exchanges and pair BTCEUR no arbitrage opportunity exists, no rebalance need
        [TestMethod]
        public void BtcEurNoArbirtrageTest()
        {
            // Arrange
            var pair = new CurrencyPair("BTC", "EUR");
            var pairs = new List<CurrencyPair>()
            {
                pair
            };
            AccountBalance balance1 = new AccountBalance()
            {
                Bitcoin = 1,
                Euro = 2000
            };
            AccountBalance balance2 = new AccountBalance()
            {
                Bitcoin = 1,
                Euro = 3500
            };
            OrderBook orderbook1 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(2406.390m, 3.164647m, 2406.38m, 2.71m),
                new OrderBookLevel(2406.490m, 0.082933m, 2406.37m, 0.5m),
                new OrderBookLevel(2407.850m, 0.465566154m, 2406.36m, 0.01m),
            }, DateTime.UtcNow);
            OrderBook orderbook2 = new OrderBook(pair, new[]
            {
                new OrderBookLevel(2406.200m,  35.133m, 2405.310m, 0.384m),
                new OrderBookLevel(2406.579m,  0.002m, 2404.459m, 0.400m),
                new OrderBookLevel(2409.889m,  0.600m, 0.037m, 2404.458m),
            }, DateTime.UtcNow);
            IExchangeGateway exchange1 = Substitute.For<IExchangeGateway>();
            exchange1.GetQuote(pairs).Returns(new[]
            {
                new Quote ("Test", pair, 2406.38m, 2406.390m, DateTime.UtcNow)
                }.ToList());
            exchange1.GetBalance().Returns(balance1);
            exchange1.GetOrderBook(pair).Returns(orderbook1);
            IExchangeGateway exchange2 = Substitute.For<IExchangeGateway>();
            exchange2.GetQuote(pairs).Returns(new [] {
                new Quote("Test", pair, 2405.310m,2406.200m, DateTime.UtcNow)
                }.ToList());
            exchange2.GetBalance().Returns(balance2);
            exchange2.GetOrderBook(pair).Returns(orderbook2);
            int id = 0;
            var exchanges = new List<Exchange>(new[] { exchange1, exchange2 }.Select(e => new Exchange(id++, e)));
            var conf = new Configuration()
            {
                Pairs = pairs
            };

            var edgeRunner = new EdgeWorker();
            edgeRunner.Initialize(conf, exchanges);

            // Act
            edgeRunner.Run();

            // Assert
            exchange1.DidNotReceive().AddOrder(Arg.Any<Order>());
            exchange2.DidNotReceive().AddOrder(Arg.Any<Order>());
        }
    }
}
