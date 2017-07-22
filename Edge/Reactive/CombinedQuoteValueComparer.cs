using System;
using System.Collections.Generic;

namespace CIB.Edge.Reactive
{
    public class CombinedQuoteValueComparer : IEqualityComparer<CombinedQuote>
    {
        public bool Equals(CombinedQuote x, CombinedQuote y)
        {
            if (ReferenceEquals(x, y))
                return true;
            return x.Ask == y.Ask && x.Bid == y.Bid;
        }

        public int GetHashCode(CombinedQuote obj)
        {
            throw new NotImplementedException();
        }
    }


}