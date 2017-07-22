namespace CIB.Exchange.Model
{
    public interface IOrderManagement
    {
        Order Create(string exchange, CurrencyPair tickr, Side side, decimal volume, OrderType limit, decimal price);
        void Send(Order order);
        void Cancel(Order order);
    }
}