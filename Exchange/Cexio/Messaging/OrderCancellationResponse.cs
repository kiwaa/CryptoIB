namespace CIB.Exchange.Cexio.Messaging
{
    class OrderCancellationResponse
    {
        public string order_id { get; set; }
        public string fremains { get; set; }
        public long time { get; set; }
        public string error { get; set; }
    }
}
