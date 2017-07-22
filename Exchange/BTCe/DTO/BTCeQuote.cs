namespace CIB.Exchange.BTCe.DTO
{
    internal class BTCeQuote
    {
        public double high { get; set; }
        public double low { get; set; }
        public double avg { get; set; }
        public double vol { get; set; }
        public double vol_cur { get; set; }
        public double last { get; set; }
        public decimal buy { get; set; }
        public decimal sell { get; set; }
        public int updated { get; set; }
    }
}
