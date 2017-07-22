namespace CIB.Exchange.Model
{
    public class OrderStatus
    {
        public long OrderId { get; set; }
        public string ExchangeOrderId { get; }
        public bool AcceptedByExchange { get; }
        public string ErrorMessage { get; }
        public bool IsCancelled { get; }

        // accepted
        public OrderStatus(long orderId, string exchangeOrderId, bool acceptedByExchange)
        {
            OrderId = orderId;
            ExchangeOrderId = exchangeOrderId;
            AcceptedByExchange = acceptedByExchange;
        }

        // rejected
        public OrderStatus(long orderId, string errorMessage)
        {
            OrderId = orderId;
            AcceptedByExchange = false;
            ErrorMessage = errorMessage;
        }

        // cancelled
        public OrderStatus(long orderId, string exchangeOrderId, bool acceptedByExchange, bool canceledByExchange) : this(orderId, exchangeOrderId, acceptedByExchange)
        {
            IsCancelled = canceledByExchange;
        }
    }
}