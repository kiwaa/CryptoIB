using System.Collections.Generic;

namespace CIB.Exchange.BTCe.DTO
{
    internal class BTCeOrderBook
    {
        public List<List<decimal>> asks { get; set; }
        public List<List<decimal>> bids { get; set; }
    }
}
