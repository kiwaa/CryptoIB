using System;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace CIB.Exchange.Model
{
    public class Order : IEquatable<Order>
    {
        private readonly BehaviorSubject<OrderState> _state;
        private readonly IOrderManagement _management;

        public long Id { get; }
        public decimal Volume { get; }
        public decimal Price { get; }
        public Side Side { get;  }
        public OrderType Type { get; }
        public CurrencyPair Pair { get; }
        public string ExchangeOrderId { get; set; }

        public OrderState State => _state.Value;

        public string ErrorMessage { get; private set; }
        public string Exchange { get; }

        public Order(IOrderManagement management, string exchange, long id, CurrencyPair pair, Side side, decimal volume, OrderType type, decimal price)
        {
            _management = management;
            Exchange = exchange;
            Id = id;
            Pair = pair;
            Volume = volume;
            Price = price;
            Side = side;
            Type = type;
            _state  = new BehaviorSubject<OrderState>(OrderState.New);
        }

        [Obsolete]
        public Order(long id, CurrencyPair pair, Side side, decimal volume, OrderType type, decimal price)
        {
            Id = id;
            Pair = pair;
            Volume = volume;
            Price = price;
            Side = side;
            Type = type;
            _state = new BehaviorSubject<OrderState>(OrderState.New);
        }

        [Obsolete]
        public Order(CurrencyPair pair, Side side, decimal volume, OrderType type, decimal price)
        {
            Pair = pair;
            Volume = volume;
            Price = price;
            Side = side;
            Type = type;
            _state = new BehaviorSubject<OrderState>(OrderState.New);
        }

        public bool Equals(Order other)
        {
            return Volume == other.Volume && Price == other.Price && Side == other.Side && Type == other.Type && string.Equals(Pair, other.Pair);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Order) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Volume.GetHashCode();
                hashCode = (hashCode * 397) ^ Price.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Side;
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ (Pair?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public void Send()
        {
            _management.Send(this);
        }

        public void Cancel()
        {
            _management.Cancel(this);
        }

        public void ApplyStatusChange(OrderStatus status)
        {
            if (status.AcceptedByExchange)
            {
                if (!status.IsCancelled)
                {
                    ExchangeOrderId = status.ExchangeOrderId;
                    _state.OnNext(OrderState.AcceptedByExchange);
                }
                else
                {
                    Debug.Assert(status.ExchangeOrderId == ExchangeOrderId);
                    _state.OnNext(OrderState.Cancelled);
                }
            }
            else
            {
                ErrorMessage = status.ErrorMessage;
                _state.OnNext(OrderState.RejectedByExchange);
            }
        }

        public IObservable<OrderState> StateNotifications()
        {
            return _state;
        }
    }
}