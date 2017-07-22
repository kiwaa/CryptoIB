using System;
using CIB.Exchange.Model;

namespace CIB.Edge.Reactive
{
    public class CombinedQuote
    {
        public string CombinedExchanges { get; }
        public CurrencyPair Ticker { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public TimeSpan TimeDiff { get; }

        public CombinedQuote(string combinedExchanges, CurrencyPair ticker, decimal bid, decimal asl, TimeSpan timeDiff)
        {
            CombinedExchanges = combinedExchanges;
            Ticker = ticker;
            Bid = bid;
            Ask = asl;
            TimeDiff = timeDiff;
        }
    }


}