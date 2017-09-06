using System;
using System.Collections.Generic;
using System.Text;
using CIB.Exchange.Model;
using Edge;
using log4net;

namespace CIB.Edge.Reactive
{
    public class SpreadTrader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpreadWatcher));
        private const decimal SpreadEntry = 0.005m;
        private const int OrderBookFactor = 3;

        private readonly CIB.Exchange.Exchange _longExchange;
        private readonly CIB.Exchange.Exchange _shortExchange;
        private readonly CurrencyPair _ticker;
        private readonly SpreadWatcher _spreadWatcher;

        public SpreadTrader(CIB.Exchange.Exchange longExchange, CIB.Exchange.Exchange shortExchange, CurrencyPair ticker)
        {
            if (longExchange == null) throw new ArgumentNullException(nameof(longExchange));
            if (shortExchange == null) throw new ArgumentNullException(nameof(shortExchange));
            if (ticker == null) throw new ArgumentNullException(nameof(ticker));
            _longExchange = longExchange;
            _shortExchange = shortExchange;
            _ticker = ticker;
            _spreadWatcher = new SpreadWatcher(_longExchange, _shortExchange, _ticker);
        }

        public IDisposable Subscribe()
        {
            return _spreadWatcher
                        .GetSpread()
                        .Subscribe(Trade);
        }

        private void Trade(CombinedQuote quote)
        {
            var longBook = _longExchange.GetOrderBook(_ticker);
            var shortBook = _shortExchange.GetOrderBook(_ticker);
            var volume = ArbitrageHelper.GetVolume(SpreadEntry, shortBook, longBook) / OrderBookFactor;
            if (volume == 0m)
            {
                Log.Info("Opportunity is gone. Trade canceled\n");
                return;
            }

            var leg1Balance = _shortExchange.GetBalance(_ticker.Base);
            var leg2Balance = _longExchange.GetBalance(_ticker.Quote);

            //var longLastQuote = btcLong.GetLastQuote(ticker);
            decimal volumeLong = Math.Min(volume, leg2Balance / quote.Ask);
            decimal volumeShort = Math.Min(volume, leg1Balance);

            // recalculated volume given balances
            volume = Math.Min(volumeLong, volumeShort);

            // we're playing safe there
            decimal limPriceLong = ArbitrageHelper.GetLimitPrice(longBook, volume * OrderBookFactor, false, OrderBookFactor);
            decimal limPriceShort = ArbitrageHelper.GetLimitPrice(shortBook, volume * OrderBookFactor, true, OrderBookFactor);

            // recheck volume again given long limit price
            volumeLong = Math.Truncate(Math.Min(volume, leg2Balance / limPriceLong) * 100000000) / 100000000;
            //if (volumeLong * limPriceLong >= leg2Balance)
            //    volumeLong -= 0.00000001m;
            volume = Math.Min(volumeLong, volumeShort);

            if (limPriceLong == 0.0m || limPriceShort == 0.0m)
            {
                Log.Warn(
                    "Opportunity found but error with the order books (limit price is null). Trade canceled");
                Log.Warn("         Long limit price:  " + limPriceLong.ToString("F2"));
                Log.Warn("         Short limit price: " + limPriceShort.ToString("F2"));
                return;
            }
            // double check spread?

            if (volume <= 0.0001m)
            {
                Log.Info("Opportunity found but volume is less than 0.0001). Trade canceled");
                Log.Info("         Volume:  " + volume);
                return;
            }

            if ((limPriceShort - limPriceLong) / limPriceLong < SpreadEntry)
            {
                Log.Warn("Opportunity found but limit price less than spread. Trade canceled");
                Log.Warn("         Long:  " + limPriceLong);
                Log.Warn("         Short:  " + limPriceShort);
                return;
            }

            // Send the orders to the two _exchanges
            TradePair(volume, limPriceShort, limPriceLong);
        }

        private void TradePair(decimal volume, decimal limPriceShort, decimal limPriceLong)
        {
            var sellOrder = new Order(_ticker, Side.Sell, volume, OrderType.Limit, limPriceShort);
            _shortExchange.SendOrder(sellOrder);
            var buyOrder = new Order(_ticker, Side.Buy, volume, OrderType.Limit, limPriceLong);
            _longExchange.SendOrder(buyOrder);
        }
    }
}
