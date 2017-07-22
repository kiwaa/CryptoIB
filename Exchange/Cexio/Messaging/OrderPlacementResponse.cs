namespace CIB.Exchange.Cexio.Messaging
{
    class OrderPlacementResponse
    {
        public bool complete { get; set; }
        public string id { get; set; }
        public long time { get; set; }
        public string pending { get; set; }
        public string amount { get; set; }
        public string type { get; set; }
        public string price { get; set; }
        public string error { get; set; }
    }
}
