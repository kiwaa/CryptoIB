using System;
using System.Collections.Generic;
using CIB.Exchange.Model;

namespace CIB.Exchange.Kraken.DTO
{
    internal sealed class QuoteDto
    {
        public List<string> a { get; set; }
        public List<string> b { get; set; }
        public List<string> c { get; set; }
        public List<string> v { get; set; }
        public List<string> p { get; set; }
        public List<int> t { get; set; }
        public List<string> l { get; set; }
        public List<string> h { get; set; }
        public string o { get; set; }

        public Quote ToDomain(CurrencyPair ticker)
        {
            return new Quote("Kraken", ticker, decimal.Parse(b[0]), decimal.Parse(a[0]), DateTime.UtcNow);
        }
    }
}
