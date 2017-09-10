using System;
using CIB.Exchange;
using CIB.Exchange.Model;

namespace CIB.OrderManagement.WebUI.Dto
{
    public class OhlcDto
    {
        public static OhlcDto FromDomain(OHLC domain)
        {
            return new OhlcDto()
            {
                Date = domain.TimestampUtc.ToUnixTimestamp(),
                Open = domain.Open,
                High = domain.High,
                Low = domain.Low,
                Close = domain.Close
            };
        }

        public long Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
