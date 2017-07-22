using System;
using System.Collections.Generic;
using CIB.Exchange.Model;
using Edge;
using log4net;

namespace CIB.Edge
{
    public class EdgeWorker
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EdgeWorker));

        private Result _res;
        private Configuration _conf;
        private List<Exchange> _exchanges;
        private bool _isInitialized;

        public void Initialize(Configuration conf, List<Exchange> exchanges)
        {
            if (conf == null) throw new ArgumentNullException(nameof(_conf));
            if (exchanges == null) throw new ArgumentNullException(nameof(_exchanges));
            if (exchanges.Count < 2) throw new ArgumentException("Not enough _exchanges. Need at least 2, found " + _exchanges.Count);
            if (_isInitialized) throw new ArgumentException("Worker cannot be initialized twice");

            _conf = conf;
            _exchanges = exchanges;
            _res = new Result();
            _isInitialized = true;

            //// We only trade BTC/USD and ETH/BTC for the moment
            //if (_conf.tradedPair().compare("BTC/USD") != 0 && _conf.tradedPair().compare("ETH/BTC") != 0) {
            //    std::cout << "ERROR: CurrencyPair '" << _conf.tradedPair() << "' is unknown. Valid pairs for now are BTC/USD and ETH/BTC\n" << std::endl;
            //    exit(EXIT_FAILURE);
            //}

            // Creates the CSV file that will collect the trade results
            //TradeResults trades = new TradeResults();
            //Log.imbue(mylocale);
            //Log.precision(2);
            //ogFile << std::fixed;
            //_conf.Log = &Log;

            // Shows the spreads
            Log.Info("[ Target ]");
            Log.Info("   Spread Entry:  " + _conf.SpreadEntry * 100.0m + "%");

            // SpreadEntry have to be positive,
            // Otherwise we will loose money on every trade.
            if (_conf.SpreadEntry <= 0.0m)
            {
                throw new ArgumentException("Spread Entry should be positive");
            }
            
            Log.Info("[ Current balances ]");
            foreach (var exchange in exchanges)
            {
                exchange.UpdateBalance();
            }
        }

        public void Run()
        {
            if (!_isInitialized) throw new ArgumentException("Worker should be initilized");

            Log.Info("----------------------------");
            Log.Info("[ " + DateTime.UtcNow + " ]");

            // Gets the bid and ask of all the _exchanges
            foreach (var exchange in _exchanges)
            {
                exchange.UpdateQuotes(_conf.Pairs);
            }
            Log.Info("----------------------------");


            // Looks for arbitrage opportunities on all the exchange combinations
            foreach (var ticker in _conf.Pairs)
            {
                foreach (var exchangeLong in _exchanges)
                {
                    foreach (var exchangeShort in _exchanges)
                    {
                        if (exchangeLong != exchangeShort)
                        {
                            //if (ticker.pair == "ETH/EUR" && (exchangeLong.Name == "Kraken" ||
                            //    exchangeShort.Name == "Kraken"))
                            //{
                            //    Log.Info("Skipping ETH/EUR for Kraken");
                            //    continue;
                            //}
                            if (ArbitrageHelper.CheckOpportunity(exchangeLong, exchangeShort, _res, ticker, _conf))
                            {
                                if (MakeTrade(exchangeLong, exchangeShort, _conf, _res, ticker, _conf.SpreadEntry))
                                    continue;

                                // Both orders are now fully executed
                                Log.Info("Done");

                                Log.Info("[ Current balances ]");
                                foreach (var exchange in _exchanges)
                                {
                                    exchange.UpdateBalance();
                                }
                                break;
                            }

                            if (_conf.AllowRebalance && RebalanceHelper.CheckOpportunity(exchangeLong, exchangeShort, ticker, _conf))
                            {
                                var leg1Short = exchangeShort.Balance.Get(ticker.Base);
                                var leg1Long = exchangeLong.Balance.Get(ticker.Base);

                                var average = (leg1Short + leg1Long) / 2;

                                var targetVolume = average - leg1Long;
                                if (targetVolume <= 0)
                                    continue;

                                if (MakeTrade(exchangeLong, exchangeShort, _conf, _res, ticker, _conf.MarketRebalanceSpread, targetVolume))
                                    continue;

                                Log.Info("Balancing is Done");

                                Log.Info("[ Current balances ]");
                                foreach (var exchange in _exchanges)
                                {
                                    exchange.UpdateBalance();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool MakeTrade(Exchange btcLong, Exchange btcShort, Configuration conf, Result res, CurrencyPair ticker, decimal spread, decimal? targetVolume = null)
        {
            // Checks the volumes and, based on that, computes the limit prices
            // that will be sent to the exchanges
            btcLong.UpdateOrderBook(ticker);
            btcShort.UpdateOrderBook(ticker);

            var volume = ArbitrageHelper.GetVolume(btcLong, btcShort, spread) / conf.OrderBookFactor;
            if (volume == 0m)
            {
                Log.Info("Opportunity is gone. Trade canceled\n");
                return true;
            }
            volume = Math.Min(targetVolume ?? decimal.MaxValue, volume); 

            var leg1Balance = btcShort.Balance.Get(ticker.Base);
            var leg2Balance = btcLong.Balance.Get(ticker.Quote);

            var longLastQuote = btcLong.GetLastQuote(ticker);
            decimal volumeLong = Math.Min(volume, leg2Balance / longLastQuote.Ask);
            decimal volumeShort = Math.Min(volume, leg1Balance);
            
            // recalculated volume given balances
            volume = Math.Min(volumeLong, volumeShort);

            // we're playing safe there
            decimal limPriceLong = btcLong.GetLimitPrice(conf, ticker, volume * conf.OrderBookFactor, false);
            decimal limPriceShort = btcShort.GetLimitPrice(conf, ticker, volume * conf.OrderBookFactor, true);

            // recheck volume again given long limit price
            volumeLong = Math.Truncate(Math.Min(volume, leg2Balance / limPriceLong) * 100000000) / 100000000;
            if (volumeLong * limPriceLong >= leg2Balance)
                volumeLong -= 0.00000001m;
            volume = Math.Min(volumeLong, volumeShort);

            if (limPriceLong == 0.0m || limPriceShort == 0.0m)
            {
                Log.Warn(
                    "Opportunity found but error with the order books (limit price is null). Trade canceled");
                Log.Warn("         Long limit price:  " + limPriceLong.ToString("F2"));
                Log.Warn("         Short limit price: " + limPriceShort.ToString("F2"));
                return true;
            }
            // double check spread?

            if (volume <= 0.0001m)
            {
                Log.Info("Opportunity found but volume is less than 0.0001). Trade canceled");
                Log.Info("         Volume:  " + volume);
                return true;
            }

            if ((limPriceShort - limPriceLong) / limPriceLong < conf.SpreadEntry)
            {
                Log.Warn("Opportunity found but limit price less than spread. Trade canceled");
                Log.Warn("         Long:  " + limPriceLong);
                Log.Warn("         Short:  " + limPriceShort);
                return true;
            }


            res.priceLongIn = limPriceLong;
            res.priceShortIn = limPriceShort;

            // Send the orders to the two _exchanges
            PairsTrading(ticker, btcLong, btcShort, volume, limPriceShort, limPriceLong);
            return false;
        }

        private static void PairsTrading(CurrencyPair ticker, Exchange btcLong, Exchange btcShort, decimal volume, decimal limPriceShort, decimal limPriceLong)
        {
            //var longOrderId = btcLong.SendBuyOrder(_conf, volumeLong, limPriceLong);
            //var shortOrderId = btcShort.SendSellOrder(_conf, volumeShort, limPriceShort);
            btcShort.SendSellOrder(ticker, volume, limPriceShort);
            btcLong.SendBuyOrder(ticker, volume, limPriceLong);

            //Log.Info("Waiting for the two orders to be filled...");
            //Thread.Sleep(5000);
            //bool isLongOrderComplete = btcLong.IsOrderComplete(_conf, longOrderId);
            //bool isShortOrderComplete = btcShort.IsOrderComplete(_conf, shortOrderId);
            ////              // Loops until both orders are completed
            //              while (!isLongOrderComplete || !isShortOrderComplete) {
            //                sleep_for(millisecs(3000));
            //                if (!isLongOrderComplete) {
            //                  Log << "Long order on " << _conf.exchName[res.idExchLong] << " still open..." << std::endl;
            //                  isLongOrderComplete = isOrderComplete[res.idExchLong] (_conf, longOrderId);
            //                }
            //                if (!isShortOrderComplete) {
            //                  Log << "Short order on " << _conf.exchName[res.idExchShort] << " still open..." << std::endl;
            //                  isShortOrderComplete = isOrderComplete[res.idExchShort] (_conf, shortOrderId);
            //                }
            //              }

        }
    }
}
