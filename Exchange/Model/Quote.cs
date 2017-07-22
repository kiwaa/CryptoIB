using System;

namespace CIB.Exchange.Model
{
    public class Quote
    {
        public string Exchange { get; }
        public CurrencyPair Pair { get; }
        public decimal Bid { get;  }
        public decimal Ask { get; }
        public DateTime TimestampUtc { get; }

        public Quote(string exchange, CurrencyPair pair, decimal bid, decimal ask, DateTime timestampUtc)
        {
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            if (pair == null) throw new ArgumentNullException(nameof(pair));
            if (bid <= 0) throw new ArgumentOutOfRangeException(nameof(bid));
            if (ask <= 0) throw new ArgumentOutOfRangeException(nameof(ask));
            Exchange = exchange;
            Pair = pair;
            Bid = bid;
            Ask = ask;
            TimestampUtc = timestampUtc;
        }
    }
}