using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CIB.Edge;
using CIB.Exchange.BTCe;
using CIB.Exchange.Cexio;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;

namespace Edge
{
    public class TriangularArbirtrage
    {
        private readonly Exchange _exchange;
        private readonly List<CurrencyPair> _pairs;

        public TriangularArbirtrage()
        {
            _exchange = new Exchange(0, new BTCeExchangeGateway());
            _pairs = new List<CurrencyPair>()
            {
                new CurrencyPair("ETH", "BTC"),
                //new CurrencyPair("ETH", "LTC"),
                //new CurrencyPair("ETH", "RUR"),
                new CurrencyPair("ETH", "USD"),
                new CurrencyPair("ETH", "EUR"),
                //new CurrencyPair("BTC", "RUR"),
                new CurrencyPair("BTC", "USD"),
                new CurrencyPair("BTC", "EUR"),
                //new CurrencyPair("LTC", "RUR"),
                //new CurrencyPair("LTC", "USD"),
                //new CurrencyPair("LTC", "EUR"),
            };
        }

        public void Run()
        {

            _exchange.UpdateQuotes(_pairs);

            Check("ETH", "BTC", "USD");
            Check("ETH", "BTC", "EUR");
            Check("ETH", "USD", "BTC");
            Check("ETH", "EUR", "BTC");
            Check("BTC", "USD", "ETH");
            Check("BTC", "EUR", "ETH");

        }

        private void Check(string fca, string fcb, string dc)
        {
            var pair = _pairs.SingleOrDefault(p => p.Base == fca && p.Quote == fcb);
            var quote = _exchange.GetLastQuote(pair);

            var bid = GetCrossRateBid(fca, fcb, dc);
            var longDiff = bid / quote.Ask - 1;
            Console.WriteLine($"{fca}/{fcb}/{dc} (long): " + longDiff * 100);

            var ask = GetCrossRateAsk(fca, fcb, dc);
            var shortDiff = quote.Bid / ask - 1;
            Console.WriteLine($"{fca}/{fcb}/{dc} (short): " + shortDiff * 100);
        }

        private decimal GetCrossRateAsk(string fca, string fcb, string dc)
        {
            //(FCa / FCb)ask = (FCa / DC)ask ×(DC/FCb)ask
            return GetAsk(fca, dc) * GetAsk(dc, fcb);
        }

        private decimal GetCrossRateBid(string fca, string fcb, string dc)
        {
            //(FCa / FCb)bid = (FCa / DC)bid ×(DC / FCb)bid
            return GetBid(fca, dc) * GetBid(dc, fcb);
        }

        private decimal GetBid(string fca, string dc)
        {
            // (FCa / DC)bid
            var single = _pairs.SingleOrDefault(p => p.Base == fca && p.Quote == dc);
            if (single != null)
            {
                return _exchange.GetLastQuote(single).Bid;
            }
            // (DC / FCa)ask
            single = _pairs.SingleOrDefault(p => p.Base == dc && p.Quote == fca);
            if (single != null)
            {
                return 1 / _exchange.GetLastQuote(single).Ask;
            }
            throw new ArgumentException($"Can't get rate for {fca}/{dc}");
        }

        private decimal GetAsk(string fca, string dc)
        {
            // (FCa / DC)bid
            var single = _pairs.SingleOrDefault(p => p.Base == fca && p.Quote == dc);
            if (single != null)
            {
                return _exchange.GetLastQuote(single).Ask;
            }
            // (DC / FCa)ask
            single = _pairs.SingleOrDefault(p => p.Base == dc && p.Quote == fca);
            if (single != null)
            {
                return 1 / _exchange.GetLastQuote(single).Bid;
            }
            throw new ArgumentException($"Can't get rate for {fca}/{dc}");
        }
    }
}
