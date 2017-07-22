
using System.Collections.Generic;

namespace CIB.Exchange.Kraken.DTO
{
    internal sealed class OrderConfirmationRootDto
    {
        public List<string> error { get; set; }
        public OrderConfirmationDto result { get; set; }
    }

    internal sealed class OrderConfirmationDto
    {
        public OrderDescriptionInfoDto descr { get; set; }
        public string[] txid { get; set; }
    }
}