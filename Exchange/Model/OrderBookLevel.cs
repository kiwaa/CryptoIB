namespace CIB.Exchange.Model
{
    public class OrderBookLevel
    {
        public OrderBookLevel(decimal askprice, decimal askvolume, decimal bidprice, decimal bidvolume)
        {
            BidVolume = bidvolume;
            BidPrice = bidprice;
            AskVolume = askvolume;
            AskPrice = askprice;
        }

        public decimal BidVolume { get; }
        public decimal BidPrice { get; }
        public decimal AskVolume { get; }
        public decimal AskPrice { get; }
    }
}