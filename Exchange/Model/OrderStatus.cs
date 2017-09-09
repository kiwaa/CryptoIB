namespace CIB.Exchange.Model
{
    public class OrderStatus
    {
        public long OrderId { get; set; }
        public string ExchangeOrderId { get; }
        public string ErrorMessage { get; }
        public OrderState NewState { get; }
        
        public OrderStatus(long orderId, string exchangeOrderId, OrderState state)
        {
            OrderId = orderId;
            ExchangeOrderId = exchangeOrderId;
            NewState = state;
        }

        public OrderStatus(long orderId, string exchangeOrderId, OrderState state, string errorMessage) : this(orderId, exchangeOrderId, state)
        {
            ErrorMessage = errorMessage;
        }
    }
}