namespace CIB.Exchange.Coinbase.Messaging
{
    internal class OrderBookDone
    {
        public string type { get; set; }
        public string time { get; set; }
        public string product_id { get; set; }
        public long sequence { get; set; }
        public decimal price { get; set; }
        public string order_id { get; set; }
        public string reason { get; set; }
        public string side { get; set; }
        public decimal remaining_size { get; set; }

    }
}
