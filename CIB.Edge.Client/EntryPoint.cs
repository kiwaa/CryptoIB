using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using CIB.Exchange.Bitstamp;
using CIB.Exchange.BTCe;
using CIB.Exchange.Cexio;
using CIB.Exchange.Coinbase;
using CIB.Exchange.Demo;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using Edge;
using log4net;
using log4net.Config;

namespace CIB.Edge.Client
{
    public class EntryPoint
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EntryPoint));
        private static EdgeWorker _worker;
        private static Timer _timer;

        public static int Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // Log file header
            Log.Info("--------------------------------------------");
            Log.Info("Edge Arbitrage Log File");
            Log.Info("--------------------------------------------");
            Log.Info("Edge started on " + DateTime.UtcNow);

            //new TriangularArbirtrage().Run();

            //Console.ReadLine();
            //Environment.Exit(1);
            // Loads all the parameters
            Configuration conf = new Configuration();
            List<global::CIB.Edge.Exchange> exchanges = CreateExchanges(conf);

            _worker = new EdgeWorker();
            _worker.Initialize(conf, exchanges);

            Log.Info("------------------");
            Log.Info("Starting main loop" + Environment.NewLine);

            // prepare timer
            _timer = new Timer((e) =>
            {
                Run();
            }, null, Timeout.Infinite, Timeout.Infinite);
            Run();


            Console.ReadLine();
            return 0;
        }

        private static void Run()
        {
            try
            {
                _worker.Run();
            }
            catch (Exception ex)
            {
                Log.Error("Suppressed exception: " + ex);
            }
            // reschedule
            _timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
        }


        private static List<global::CIB.Edge.Exchange> CreateExchanges(Configuration conf)
        {
            // Adds the exchange functions to the arrays for all the defined exchanges
            // Poloniex is only used if the traded pair is ETH/BTC as they don't
            // deal with USD.
            // TODO: should be in a separated function, and there probably is a better
            // way to implement that.

            List<global::CIB.Edge.Exchange> exchanges = new List<global::CIB.Edge.Exchange>();
            exchanges.Add(new global::CIB.Edge.Exchange(0, CreateKrakenExchangeGateway()));
            //exchanges.Add(new Exchange(1, CreateBtCeExchangeGateway()));
            exchanges.Add(new global::CIB.Edge.Exchange(2, CreateCexioExchangeGateway()));
            exchanges.Add(new global::CIB.Edge.Exchange(3, CreateCoinbaseExchangeGateway()));
            //exchanges.Add(new Exchange(4, CreateBitstampExchangeGateway()));

            // We need at least two exchanges to run Blackbird
            //if (index < 2)
            //{
            //    Log.Error("Blackbird needs at least two Bitcoin exchanges. Please edit the config.json file to add new exchanges");
            //    return 1;
            //}

            // The btcVec vector contains details about every exchange,
            // like fees, as specified in bitcoin.h
            // Creates a new Bitcoin structure within btcVec for every exchange we want to trade on
            //for (int i = 0; i < numExch; ++i)
            //{
            //    btcVec.Add(new Bitcoin(i,  conf.exchName[i], conf.fees[i], conf.canShort[i], conf.isImplemented[i]));
            //}
            return exchanges;
        }

        private static IExchangeGateway CreateCoinbaseExchangeGateway()
        {
            return new DemoExchangeGateway(new CoinbaseExchangeGateway());
        }

        private static IExchangeGateway CreateBitstampExchangeGateway()
        {
            return new DemoExchangeGateway(new BitstampExchangeGateway());
        }

        private static IExchangeGateway CreateCexioExchangeGateway()
        {
            return new DemoExchangeGateway(new CexioExchangeGateway());
        }

        private static IExchangeGateway CreateBtCeExchangeGateway()
        {
            return new DemoExchangeGateway(new BTCeExchangeGateway());
        }

        private static IExchangeGateway CreateKrakenExchangeGateway()
        {
            return new DemoExchangeGateway(new KrakenExchangeGateway());
        }
    }
}
