using System;
using System.Collections.Generic;
using CIB.Exchange.Model;

namespace CIB.Exchange.Kraken.DTO
{
    internal sealed class OrderBookDto
    {
        public List<List<string>> asks { get; set; }
        public List<List<string>> bids { get; set; }

        public global::CIB.Exchange.Model.OrderBook ToDomain(CurrencyPair ticker)
        {
            return new global::CIB.Exchange.Model.OrderBook(ticker, ConvertToDomain(asks, bids), DateTime.UtcNow);
        }

        private IEnumerable<OrderBookLevel> ConvertToDomain(List<List<string>> asks, List<List<string>> bids)
        {
            var depth = Math.Min(asks.Count, bids.Count);
            for (int i = 0; i < depth; i++)
            {
                var ask = asks[i];
                var bid = bids[i];
                yield return new OrderBookLevel(decimal.Parse(ask[0]), decimal.Parse(ask[1]), decimal.Parse(bid[0]), decimal.Parse(bid[1]));
            }
        }
    }
}
