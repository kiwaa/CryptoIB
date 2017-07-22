using System;
using CIB.Exchange.Model;

namespace CIB.Exchange.Coinbase
{
    public class CoinbaseConverter
    {
        public static string ConvertTicker(CurrencyPair arg)
        {
            return arg.Base + "-" + arg.Quote;
        }

        public static CurrencyPair ConvertTicker(string product)
        {
            var strings = product.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            return new CurrencyPair(strings[0], strings[1]);
        }
    }
}