using System.Collections.Generic;

namespace CIB.Exchange.Coinbase.DTO
{
    public class CoinbaseOrderBook1
    {
        public long sequence { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
    }
}