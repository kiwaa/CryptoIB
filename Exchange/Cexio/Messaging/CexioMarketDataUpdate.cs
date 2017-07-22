using System.Collections.Generic;

namespace CIB.Exchange.Cexio.Messaging
{
    internal class CexioMarketDataUpdate
    {
        public long id { get; set; }
        public string pair { get; set; }
        public long time { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
    }
}
