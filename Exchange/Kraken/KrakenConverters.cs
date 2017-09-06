using System;
using System.Diagnostics;
using System.Linq;
using CIB.Exchange.Model;

namespace CIB.Exchange.Kraken
{
    internal static class KrakenConverters
    {
        public static string ConvertOrderType(OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Market:
                    Debug.Fail("not tested");
                    return "market";
                case OrderType.Limit:
                    return "limit";
            }
            Debug.Fail("Unknown order type");
            throw new InvalidOperationException("Unknown order type: " + orderType);
        }

        public static string ConvertDirection(Side side)
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

        public static string ConvertTicker(CurrencyPair pair)
        {
            if (pair.Base == "BCH")
                return pair.Base + (pair.Quote == "BTC" ? "XBT" : pair.Quote);

            if (pair.Base == "DSH")
                return "DASH" + pair.Quote;
            return ConvertCurrency(pair.Base) + ConvertCurrency(pair.Quote);
        }

        public static string ConvertCurrency(string currency)
        {
            switch (currency)
            {
                case "BTC":
                    return "XXBT";
                case "ETH":
                    return "XETH";
                case "LTC":
                    return "XLTC";
                case "BCH":
                    return "BCH";
                case "EUR":
                    return "ZEUR";
                case "USD":
                    return "ZUSD";
                default:
                    Debug.Fail("Unkown currency");
                    throw new NotImplementedException("Unkown currency");
            }
        }
    }
}
