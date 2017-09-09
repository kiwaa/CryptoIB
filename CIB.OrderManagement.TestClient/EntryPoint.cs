using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CIB.Exchange;
using CIB.Exchange.Cexio;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using Microsoft.Extensions.Configuration;

namespace CIB.OrderManagement.TestClient
{
    class EntryPoint
    {
        public static IConfigurationRoot Configuration { get; set; }

        private static Dictionary<long, Order> _orders = new Dictionary<long, Order>();

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            var tickr = new CurrencyPair("BTC", "EUR");
            var krakenExchange = CreateKrakenExchange(new[] { tickr });
            var cexioExchange = CreateCexioExchange(new[] { tickr });
            IDictionary<string, IReactiveExchangeGateway> route = new Dictionary<string, IReactiveExchangeGateway>
            {
                { "Kraken", krakenExchange },
                { "Cexio", cexioExchange }
            };
            var orderManagement = new OrderManagementS(route);


            Thread.Sleep(10000);

            Console.WriteLine("Start..");
            Test("Kraken", orderManagement, tickr); // sync!
            Test("Cexio", orderManagement, tickr);

            Console.ReadLine();
        }

        private static void Test(string name, IOrderManagement orderManagement, CurrencyPair tickr)
        {
            // buy 1 btc for 100 eur at kraken
            var order1 = orderManagement.Create(name, tickr, Side.Buy, 1m, OrderType.Limit, 100m);
            // buy 1 btc for 100 eur at kraken
            var order2 = orderManagement.Create(name, tickr, Side.Buy, 0.01m, OrderType.Limit, 100m);
            order1.StateNotifications().Subscribe(s => OnNewState(order1, s));
            order2.StateNotifications().Subscribe(s => OnNewState(order2, s));
            {
                order1.Send();
                order2.Send();
            }
        }

        private static void OnNewState(Order order, OrderState state)
        {
            switch (order.State)
            {
                case OrderState.RejectedByExchange:
                    Console.WriteLine($"[{order.Exchange}] Order {order.Id} was not accepted by exchange" + (order.ErrorMessage != null ? ": " + order.ErrorMessage : ""));
                    break;
                case OrderState.Accepted:
                    Console.WriteLine($"[{order.Exchange}] Order {order.Id} accepted by exchange");
                    order.Cancel();
                    break;
                case OrderState.Cancelled:
                    Console.WriteLine($"[{order.Exchange}] Order {order.Id} was cancelled by exchange");
                    break;
            }
        }

        private static IReactiveExchangeGateway CreateKrakenExchange(IEnumerable<CurrencyPair> tickers)
        {
            var key = Configuration["kraken:key"];
            var secret = Configuration["kraken:secret"];
            var krakenExchange = new KrakenExchangeGateway(key, secret);
            var krakenReactiveExchange = new ReactiveExchangeGatewayAdapter(krakenExchange , tickers);
            return krakenReactiveExchange;
        }

        private static IReactiveExchangeGateway CreateCexioExchange(CurrencyPair[] currencyPairs)
        {
            var key = Configuration["cexio:key"];
            var secret = Configuration["cexio:secret"];

            var cexioReactiveExchange = new CexioReactiveExchangeGateway(key, secret, currencyPairs);
            return cexioReactiveExchange;
        }
    }
}