using CIB.Exchange.Model;

namespace CIB.OrderManagement.WebUI.Dto
{
    public class OrderDto
    {
        public static OrderDto FromDomain(Order order)
        {
            return new OrderDto()
            {
                Id = order.Id,
                Exchange = order.Exchange,
                ExchangeOrderId = order.ExchangeOrderId,
                Currency = order.Pair.Base,
                Side = order.Side,
                Volume = order.Volume,
                Type = order.Type,
                Price = order.Price,
                State = order.State
            };
        }

        public long Id { get; set; }
        public string Exchange { get; set; }
        public string ExchangeOrderId { get; set; }
        public string Currency { get; set; }
        public Side Side { get; set; }
        public decimal Volume { get; set; }
        public OrderType Type { get; set; }
        public decimal? Price { get; set; }
        public OrderState State { get; set; }
    }
}