namespace CIB.Exchange.Coinbase.Messaging
{
        internal class OrderBookReceived
        {
            public string type { get; set; }
            public string time { get; set; }
            public string product_id { get; set; }
            public long sequence { get; set; }
            public string order_id { get; set; }
            public string size { get; set; }
            public string price { get; set; }
            public string side { get; set; }
            public string order_type { get; set; }
            public string funds { get; set; }
        }

}
