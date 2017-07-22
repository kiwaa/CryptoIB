
using System.IO;
using System.Linq;
using CIB.Exchange.BTCe;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using Microsoft.Extensions.Configuration;

namespace CIB.Exchange.TestClient
{
    class EntryPoint
    {
        private const decimal TestVolume = 0.01m;
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            TestKraken();
            TestBtce();
        }

        private static void TestKraken()
        {
            var key = Configuration["kraken:key"];
            var secret = Configuration["kraken:secret"];
            KrakenExchangeGateway krakenGateway = new KrakenExchangeGateway(key, secret);

            var pairs = new[] {new CurrencyPair("BTC", "EUR")};
            var quote = krakenGateway.GetQuote(pairs);
            var balance = krakenGateway.GetBalance();
            var superLowBid = quote.First().Bid * 0.5m;
            //krakenGateway.AddOrder(new Order(Side.Bid, TestVolume, OrderType.Limit, superLowBid ));
        }

        private static void TestBtce()
        {
            var key = Configuration["btce:key"];
            var secret = Configuration["btce:secret"];
            BTCeExchangeGateway btceGateway = new BTCeExchangeGateway(key, secret);

            var pairs = new[] { new CurrencyPair("BTC", "EUR") };
            var quote = btceGateway.GetQuote(pairs);
            var balance = btceGateway.GetBalance();
            var superLowBid = quote.First().Bid * 0.5m;
            //btceGateway.AddOrder(new Order(Side.Bid, TestVolume, OrderType.Limit, superLowBid));
        }
    }
}