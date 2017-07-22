using CIB.Edge;
using CIB.Exchange.Model;

namespace Edge
{
    public class RebalanceHelper
    {
        public static bool CheckOpportunity(Exchange btcLong, Exchange btcShort, CurrencyPair ticker, Configuration conf)
        {
            // Gets the prices and computes the spread
            var priceLong = btcLong.GetLastQuote(ticker).Ask;
            var priceShort = btcShort.GetLastQuote(ticker).Bid;

            var spread = ArbitrageHelper.GetSpread(priceLong, priceShort);
            if (spread > conf.MarketRebalanceSpread)
                return true;
            return false;
        }
    }
}