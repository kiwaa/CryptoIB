using System;
using System.Reactive.Linq;
using CIB.Exchange.Model;
using log4net;

namespace CIB.Edge.Reactive
{
    public class SpreadWatcher
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpreadWatcher));
        private const decimal SpreadEntry = 0.005m;
        private const int MaxTimeDelta = 30000;

        private readonly CIB.Exchange.Exchange _longExchange;
        private readonly CIB.Exchange.Exchange _shortExchange;
        private readonly CurrencyPair _ticker;
        private decimal _maxSpread;
        private decimal _minSpread;

        public SpreadWatcher(CIB.Exchange.Exchange longExchange, CIB.Exchange.Exchange shortExchange, CurrencyPair ticker)
        {
            if (longExchange == null) throw new ArgumentNullException(nameof(longExchange));
            if (shortExchange == null) throw new ArgumentNullException(nameof(shortExchange));
            if (ticker == null) throw new ArgumentNullException(nameof(ticker));
            _longExchange = longExchange;
            _shortExchange = shortExchange;
            _ticker = ticker;
        }
        
        public IObservable<CombinedQuote> GetSpread()
        {
            var s1 = _longExchange.GetMarketData()
                .Where(quote => quote.Pair.Equals(_ticker));
            var s2 = _shortExchange.GetMarketData()
                .Where(quote => quote.Pair.Equals(_ticker));
            return s1.CombineLatest(s2, (q1, q2) => new CombinedQuote($"{q1.Exchange}/{q2.Exchange}", q1.Pair, q2.Bid, q1.Ask, q1.TimestampUtc - q2.TimestampUtc))
                .DistinctUntilChanged(new CombinedQuoteValueComparer())
                .Throttle(TimeSpan.FromSeconds(10)) // let's slowdown things
                .Where(IsTradeOpportunity);
        }

        private bool IsTradeOpportunity(CombinedQuote quote)
        {
            if (Math.Abs(quote.TimeDiff.TotalMilliseconds) > MaxTimeDelta)
            {
                Log.Info($"{_ticker} ({quote.CombinedExchanges}):\t...too old");
                return false;
            }
            var spread = quote.Bid / quote.Ask - 1;
            // We update the max and min spread
            _maxSpread = Math.Max(spread, _maxSpread);
            _minSpread = Math.Min(spread, _minSpread);

            Log.Info($"{_ticker} ({quote.CombinedExchanges}):\t {spread:P}");
            Log.Info($"{_ticker} ({quote.CombinedExchanges}) [target {SpreadEntry:P}, min {_minSpread:P}, max {_maxSpread:P}]");

            return spread > SpreadEntry;
        }

    }


}