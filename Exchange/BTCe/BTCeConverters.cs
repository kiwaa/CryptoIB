using System;
using System.Diagnostics;
using CIB.Exchange.Model;

namespace CIB.Exchange.BTCe
{
    internal static class BTCeConverters
    {
        public static string ConvertSide(Side side)
        {
            switch (side)
            {
                case Side.Buy:
                    return "buy";
                case Side.Sell:
                    return "sell";
            }
            Debug.Fail("Unknown side");
            throw new InvalidOperationException("Unknown side: " + side);
        }

        public static string ConvertTicker(CurrencyPair ticker)
        {
            return ticker.Base.ToLower() + "_" + ticker.Quote.ToLower();
        }
    }
}
