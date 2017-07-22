using System.Collections.Generic;
using CIB.Exchange.Model;

namespace CIB.Exchange
{
    public class QuoteValueComparer : IEqualityComparer<Quote>
    {
        private readonly PriceComparison _prceComparison;

        public enum PriceComparison
        {
            BidOnly,
            AskOnly,
            BidAndAsk
        }

        public QuoteValueComparer(PriceComparison prceComparison)
        {
            _prceComparison = prceComparison;
        }

        public bool Equals(Quote x, Quote y)
        {
            if (ReferenceEquals(x, y))
                return true;
            return x.Exchange == y.Exchange &&
                   (_prceComparison == PriceComparison.BidOnly || x.Ask == y.Ask) &&
                   (_prceComparison == PriceComparison.AskOnly || x.Bid == y.Bid);
        }

        public int GetHashCode(Quote quote)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + quote.Exchange.GetHashCode();
                hash = hash * 23 + quote.Ask.GetHashCode();
                hash = hash * 23 + quote.Bid.GetHashCode();
                return hash;
            }
        }
    }
}