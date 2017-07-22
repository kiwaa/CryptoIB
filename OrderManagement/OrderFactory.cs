using CIB.Exchange;
using CIB.Exchange.Model;

namespace CIB.OrderManagement
{
    internal class OrderFactory
    {
        private static long NextOrderId = 1;

        public static long GenerateId()
        {
            return NextOrderId++;
        }

        public static Order CreateLimitOrder(CurrencyPair tickr, Side side, decimal volume, decimal price)
        {
            return new Order(GenerateId(), tickr, side, volume, OrderType.Limit, price);
        }

        public static Order CreateLimitOrder(IOrderManagement management, string exchange, CurrencyPair tickr, Side side, decimal volume, decimal price)
        {
            return new Order(management, exchange, GenerateId(), tickr, side, volume, OrderType.Limit, price);
        }
    }
}
