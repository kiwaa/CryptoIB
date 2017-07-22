using System;

namespace CIB.Exchange.Model
{
    public class CurrencyPair : IEquatable<CurrencyPair>
    {
        public string Base { get; }
        public string Quote { get; }

        public string Ticker => Base + "/" + Quote;

        public CurrencyPair(string @base, string quote)
        {
            if (@base == null) throw new ArgumentNullException(nameof(@base));
            if (quote == null) throw new ArgumentNullException(nameof(quote));
            Base = @base.ToUpper();
            Quote = quote.ToUpper();
        }

        public bool Equals(CurrencyPair other)
        {
            return string.Equals(Base, other.Base) && string.Equals(Quote, other.Quote);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CurrencyPair) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Base != null ? Base.GetHashCode() : 0) * 397) ^ (Quote != null ? Quote.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Ticker;
        }
    }
}
