using System;
using System.Collections.Generic;
using System.Linq;

namespace CIB.Exchange.Model
{
    public class OrderBook
    {
        private readonly List<OrderBookLevel> _levels;

        public OrderBook(CurrencyPair ticker, IEnumerable<OrderBookLevel> levels, DateTime lastUpdate)
        {
            Ticker = ticker;
            LastUpdate = lastUpdate;
            _levels = levels.ToList();
        }

        public OrderBook(CurrencyPair ticker, List<List<decimal>> orderBookBids, List<List<decimal>> orderBookAsks, DateTime lastUpdate) : this(ticker, ConvertToDomain(orderBookAsks, orderBookBids), lastUpdate)
        {
        }

        public int Depth => _levels.Count;
        public CurrencyPair Ticker { get; }
        public DateTime LastUpdate { get; }

        public OrderBookLevel GetLevel(int level)
        {
            return _levels[level-1];
        }

        private static IEnumerable<OrderBookLevel> ConvertToDomain(List<List<decimal>> asks, List<List<decimal>> bids)
        {
            var depth = Math.Min(asks.Count, bids.Count);
            for (int i = 0; i < depth; i++)
            {
                var ask = asks[i];
                var bid = bids[i];
                yield return new OrderBookLevel(ask[0], ask[1], bid[0], bid[1]);
            }
        }

    }
}