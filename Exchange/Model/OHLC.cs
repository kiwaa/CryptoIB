using System;

namespace CIB.Exchange.Model
{
    public class OHLC
    {
        public string Exchange { get; }
        public CurrencyPair Pair { get; }
        public DateTime TimestampUtc { get; }
        public decimal Open { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Close { get; }

        public OHLC(string exchange, CurrencyPair pair, DateTime timestampUtc, decimal open, decimal high, decimal low, decimal close)
        {
            if (timestampUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("DateTime should have UTC kind");
            Exchange = exchange;
            Pair = pair;
            TimestampUtc = timestampUtc;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
    }
}
