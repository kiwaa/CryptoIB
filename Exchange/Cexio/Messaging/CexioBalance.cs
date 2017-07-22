using System;
using System.Collections.Generic;
using System.Text;

namespace CIB.Exchange.Cexio.Messaging
{
    public class CexioBalance
    {
        public Dictionary<string, decimal> balance { get; set; }
        public Dictionary<string, decimal> obalance { get; set; }
    }
}
