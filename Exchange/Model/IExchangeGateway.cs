using System.Collections.Generic;

namespace CIB.Exchange.Model
{
    public interface IExchangeGateway
    {
        string Name { get; }
        List<Quote> GetQuote(IEnumerable<CurrencyPair> ticker);
        OrderBook GetOrderBook(CurrencyPair ticker);
        AccountBalance GetBalance();
        OrderStatus AddOrder(Order order);
        OrderStatus CancelOrder(Order order);
    }
}
