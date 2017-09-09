using CIB.Exchange.Model;

namespace CIB.OrderManagement.WebUI.Dto
{
    public class QuoteDto
    {
        public static QuoteDto FromDomain(Quote quote)
        {
            return new QuoteDto
            {
                Exchange = quote.Exchange,
                Pair = quote.Pair.ToString(),
                Bid = quote.Bid,
                Ask = quote.Ask
            };
        }

        public string Status => "Live";
        public string Exchange { get; set; }
        public string Pair { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }

    }
}
