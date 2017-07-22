namespace CIB.Exchange.Coinbase.DTO
{
    internal class CoinbaseQuote
    {
        public string message { get; set; }
        public int trade_id { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string bid { get; set; }
        public string ask { get; set; }
        public string volume { get; set; }
        public string time { get; set; }

    }
}