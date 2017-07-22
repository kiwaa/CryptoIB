using System;
using CIB.Edge;
using CIB.Exchange.Model;
using log4net;

namespace Edge
{
    public static class ArbitrageHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ArbitrageHelper));

        public static bool CheckOpportunity(Exchange btcLong, Exchange btcShort, Result res, CurrencyPair ticker, Configuration conf)
        {
            //if (!btcShort->getHasShort()) return false;

            // Gets the prices and computes the spread
            var priceLong = btcLong.GetLastQuote(ticker).Ask;
            var priceShort = btcShort.GetLastQuote(ticker).Bid;

            res.spreadIn = GetSpread(priceLong, priceShort);
            int longId = btcLong.Id;
            int shortId = btcShort.Id;

            // We update the max and min spread if necessary
            var tickerIndex = conf.Pairs.IndexOf(ticker);
            res.maxSpread[longId][shortId][tickerIndex] = Math.Max(res.spreadIn, res.maxSpread[longId][shortId][tickerIndex]);
            res.minSpread[longId][shortId][tickerIndex] = Math.Min(res.spreadIn, res.minSpread[longId][shortId][tickerIndex]);

            //if (params.verbose) {
            //     params.logFile->precision(2);
            Log.Info("(" + ticker.Ticker + ") " + btcLong.Name + "/" + btcShort.Name + ":\t" + res.spreadIn.ToString("P"));
            Log.Info("(" + ticker.Ticker + ")\t[target " + conf.SpreadEntry.ToString("P") + ", min " + res.minSpread[longId][shortId][tickerIndex].ToString("P") + ", max " + res.maxSpread[longId][shortId][tickerIndex].ToString("P") + "]");
            //    //// The short-term volatility is computed and
            //    //// displayed. No other action with it for
            //    //// the moment.
            //    //if (params.useVolatility) {
            //    //    if (res.volatility[longId][shortId].size() >= params.volatilityPeriod) {
            //    //        auto stdev = compute_sd(begin(res.volatility[longId][shortId]), end(res.volatility[longId][shortId]));
            //    //        *params.logFile << "  volat. " << stdev * 100.0 << "%";
            //    //    } else {
            //    //        *params.logFile << "  volat. n/a " << res.volatility[longId][shortId].size() << "<" << params.volatilityPeriod << " ";
            //    //    }
            //    //}

            //    // Updates the trailing spread
            //    // TODO: explain what a trailing spread is.
            //    // See #12 on GitHub for the moment
            //    if (res.trailing[longId][shortId] != -1.0)
            //    {
            //        *params.logFile << "   trailing " << percToStr(res.trailing[longId][shortId]) << "  " << res.trailingWaitCount[longId][shortId] << "/" << params.trailingCount;
            //    }

            //    // If one of the exchanges (or both) hasn't been implemented,
            //    // we mention in the log file that this spread is for info only.
            //if (!btcLong.IsImplemented || !btcShort.IsImplemented)
            //{
            //    Log.Info("   info only");
            //}
            //}

            // We need both exchanges to be implemented,
            // otherwise we return False regardless of
            // the opportunity found.
            //if (!btcLong.IsImplemented || !btcShort.IsImplemented || res.spreadIn == 0.0m)
            //    return false;

            //// the trailing spread is reset for this pair,
            //// because once the spread is *below*
            //// SpreadEndtry. Again, see #12 on GitHub for
            //// more details.
            if (res.spreadIn < conf.SpreadEntry)
            {
                //res.trailing[longId][shortId] = -1.0;
                //res.trailingWaitCount[longId][shortId] = 0;
                return false;
            }

            //// Updates the trailingSpread with the new value
            //decimal newTrailValue = res.spreadIn - conf.trailingLim;
            //if (res.trailing[longId][shortId] == -1.0)
            //{
            //    res.trailing[longId][shortId] = Math.Max(newTrailValue, conf.spreadEntry);
            //    return false;
            //}

            //if (newTrailValue >= res.trailing[longId][shortId])
            //{
            //    res.trailing[longId][shortId] = newTrailValue;
            //    res.trailingWaitCount[longId][shortId] = 0;
            //}
            //if (res.spreadIn >= res.trailing[longId][shortId])
            //{
            //    res.trailingWaitCount[longId][shortId] = 0;
            //    return false;
            //}

            //if (res.trailingWaitCount[longId][shortId] < params.trailingCount) {
            //    res.trailingWaitCount[longId][shortId]++;
            //    return false;
            //}

            // Updates the Result structure with the information about
            // the two trades and return True (meaning an opportunity
            // was found).
            res.idExchLong = longId;
            res.idExchShort = shortId;
            res.feesLong = btcLong.Fees;
            res.feesShort = btcShort.Fees;
            res.exchNameLong = btcLong.Name;
            res.exchNameShort = btcShort.Name;
            res.priceLongIn = priceLong;
            res.priceShortIn = priceShort;
            //res.exitTarget = res.spreadIn - conf.SpreadTarget - 2.0m * (res.feesLong + res.feesShort);
            //res.trailingWaitCount[longId][shortId] = 0;
            return true;
        }

        public static decimal GetSpread(decimal priceLong, decimal priceShort)
        {
            // If the prices are null we return a null spread
            // to avoid false opportunities
            if (priceLong > 0.0m && priceShort > 0.0m)
            {
                return (priceShort - priceLong) / priceLong;
            }
            return 0.0m;
        }

        public static decimal GetVolume(Exchange btcLong, Exchange btcShort, decimal confSpreadEntry)
        {
            var longBook = btcLong.OrderBook;
            var shortBook = btcShort.OrderBook;

            return GetVolume(confSpreadEntry, shortBook, longBook);
        }

        public static decimal GetVolume(decimal confSpreadEntry, OrderBook shortBook, OrderBook longBook)
        {
            int longLevel = 1;
            int shortLevel = 1;

            var spread = (shortBook.GetLevel(shortLevel).BidPrice - longBook.GetLevel(longLevel).AskPrice) /
                         longBook.GetLevel(longLevel).AskPrice;
            if (spread < confSpreadEntry)
                return Math.Min(0, 0);

            var longVolume = longBook.GetLevel(longLevel).AskVolume;
            var shortVolume = shortBook.GetLevel(longLevel).BidVolume;

            while (true)
            {
                if (shortVolume < longVolume)
                {
                    shortLevel++;
                    if (shortLevel >= shortBook.Depth)
                        return Math.Min(shortVolume, longVolume);

                    var shortOrderBookLevel = shortBook.GetLevel(shortLevel);
                    var longOrderBookLevel = longBook.GetLevel(longLevel);
                    spread = (shortOrderBookLevel.BidPrice - longOrderBookLevel.AskPrice) /
                             longOrderBookLevel.AskPrice;
                    if (spread < confSpreadEntry)
                        return Math.Min(shortVolume, longVolume);
                    shortVolume += shortOrderBookLevel.BidVolume;
                }
                else
                {
                    longLevel++;
                    if (longLevel >= longBook.Depth)
                        return Math.Min(shortVolume, longVolume);

                    var shortOrderBookLevel = shortBook.GetLevel(shortLevel);
                    var longOrderBookLevel = longBook.GetLevel(longLevel);
                    spread = (shortOrderBookLevel.BidPrice - longOrderBookLevel.AskPrice) /
                             longOrderBookLevel.AskPrice;
                    if (spread < confSpreadEntry)
                        return Math.Min(shortVolume, longVolume);
                    longVolume += longOrderBookLevel.BidVolume;
                }
            }
        }

        public static decimal GetLimitPrice(OrderBook orderbook, decimal volume, bool isBid, int orderbookFactor)
        {
            //var orderbook = _gateway.GetOrderBook(ticker);
            if (orderbook == null)
                return 0;

            decimal totVol = 0.0m;
            decimal currPrice = 0;

            if (isBid)
            {
                for (int i = 1; i < orderbook.Depth; i++)
                {
                    // volumes are added up until the requested volume is reached
                    var currVol = orderbook.GetLevel(i).BidVolume;
                    currPrice = orderbook.GetLevel(i).BidPrice;
                    totVol += currVol;
                    if (totVol >= volume * orderbookFactor)
                        break;
                }
            }
            else
            {
                for (int i = 1; i < orderbook.Depth; i++)
                {
                    // volumes are added up until the requested volume is reached
                    var currVol = orderbook.GetLevel(i).AskVolume;
                    currPrice = orderbook.GetLevel(i).AskPrice;
                    totVol += currVol;
                    if (totVol >= volume * orderbookFactor)
                        break;
                }
            }

            return currPrice;

        }
    }
}
