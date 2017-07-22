using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CIB.Edge.Reactive;
using CIB.Exchange;
using CIB.Exchange.Model;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

namespace CIB.Edge.ReactiveClient
{
    public class EntryPoint
    {
        public static IConfigurationRoot Configuration { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(typeof(EntryPoint));
        private static bool Demo = false;

        public static void Main()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            // Log file header
            Log.Info("--------------------------------------------");
            Log.Info("Edge Arbitrage Log File");
            Log.Info("--------------------------------------------");
            Log.Info("Edge started on " + DateTime.UtcNow);


            var tickers = new List<CurrencyPair>
            {
                new CurrencyPair("BTC", "EUR"),
                new CurrencyPair("ETH", "EUR"),
                new CurrencyPair("ETH", "BTC"),
                new CurrencyPair("BCH", "EUR"),
                new CurrencyPair("BCH", "BTC"),
            };

            var cexio = CreateExchange(ExchangeFactory.CreateCexioExchange(tickers));
            //var coinbase = CreateExchange(ExchangeFactory.CreateGdaxExchange(tickers));
            var kraken = CreateExchange(ExchangeFactory.CreateKrakenExchange(tickers));

            //var cexioGdaxBtcEur = new SpreadTrader(cexio, coinbase, tickers[0]);
            //var gdaxCexioBtcEur = new SpreadTrader(coinbase, cexio, tickers[0]);
            //var cexioGdaxEthEur = new SpreadTrader(cexio, coinbase, tickers[1]);
            //var gdaxCexioEthEur = new SpreadTrader(coinbase, cexio, tickers[1]);
            var krakenCexioBtcEur = new SpreadTrader(kraken, cexio, tickers[0]);
            var cexioKrakenBtcEur = new SpreadTrader(cexio, kraken, tickers[0]);
            var krakenCexioEthEur = new SpreadTrader(kraken, cexio, tickers[1]);
            var cexioKrakenEthEur = new SpreadTrader(cexio, kraken, tickers[1]);
            var krakenCexioEthBtc = new SpreadTrader(kraken, cexio, tickers[2]);
            var cexioKrakenEthBtc = new SpreadTrader(cexio, kraken, tickers[2]);
            var krakenCexioBchEur = new SpreadTrader(kraken, cexio, tickers[3]);
            var cexioKrakenBchEur = new SpreadTrader(cexio, kraken, tickers[3]);
            var krakenCexioBchBtc = new SpreadTrader(kraken, cexio, tickers[4]);
            var cexioKrakenBchBtc = new SpreadTrader(cexio, kraken, tickers[4]);

            //using (cexioGdaxBtcEur.Subscribe())
            //using (cexioGdaxEthEur.Subscribe())
            //using (gdaxCexioBtcEur.Subscribe())
            //using (gdaxCexioEthEur.Subscribe())
            using (krakenCexioBtcEur.Subscribe())
            using (cexioKrakenBtcEur.Subscribe())
            using (krakenCexioEthEur.Subscribe())
            using (cexioKrakenEthEur.Subscribe())
            using (krakenCexioEthBtc.Subscribe())
            using (cexioKrakenEthBtc.Subscribe())
            using (krakenCexioBchEur.Subscribe())
            using (cexioKrakenBchEur.Subscribe())
            using (krakenCexioBchBtc.Subscribe())
            using (cexioKrakenBchBtc.Subscribe())
            {
                Console.WriteLine("Press ENTER to unsubscribe...");
                Console.ReadLine();
                Log.Info("Canceled by user");
            }
        }

        private static CIB.Exchange.Exchange CreateExchange(IReactiveExchangeGateway reactiveExchangeGateway)
        {
            if (Demo)
                reactiveExchangeGateway = new DemoReactiveExchangeGateway(reactiveExchangeGateway);
            var exchange = new CIB.Exchange.Exchange(reactiveExchangeGateway);
            exchange.Subscribe();
            return exchange;
        }
    }
}
