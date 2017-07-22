namespace CIB.Exchange.Coinbase.Messaging
{
        internal class OrderBookMatch
        {
            public string type { get; set; }
            public long trade_id { get; set; }
            public long sequence { get; set; }
            public string maker_order_id { get; set; }
            public string taker_order_id { get; set; }
            public string time { get; set; }
            public string product_id { get; set; }
            public decimal size { get; set; }
            public decimal price { get; set; }
            public string side { get; set; }
        }
}
