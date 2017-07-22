using System.Collections.Generic;

namespace CIB.Exchange.Coinbase.DTO
{
    public class CoinbaseFullOrderBook
    {
        public long sequence { get; set; }
        public List<string[]> bids { get; set; }
        public List<string[]> asks { get; set; }
    }
}