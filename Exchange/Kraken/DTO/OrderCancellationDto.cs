using System.Collections.Generic;


namespace CIB.Exchange.Kraken.DTO
{
    internal sealed class OrderCancellationRootDto
    {
        public List<string> error { get; set; }
        public OrderCancellationDto result { get; set; }
    }

    class OrderCancellationDto
    {
        public int count { get; set; }
        public bool pending { get; set; }
    }
}
