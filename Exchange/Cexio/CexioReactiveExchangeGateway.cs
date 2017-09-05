using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CIB.Exchange.Cexio.Messaging;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.Cexio
{
    public class CexioReactiveExchangeGateway : IReactiveExchangeGateway, IDisposable
    {
        private readonly List<CurrencyPair> _tickers;
        private readonly Uri WebsocketApi = new Uri("wss://ws.cex.io/ws/");
        private readonly string _key;
        private readonly string _secret;
        private readonly ReactiveWebSocket _client;
        private int _messageId = 1;

        private int id = 0;
        private string[] buffer = new string[10];

        private readonly Dictionary<string, Messaging.CexioOrderBook> _orderBooks = new Dictionary<string, Messaging.CexioOrderBook>();
        private readonly IDisposable _subscription;

        private readonly ReplaySubject<OrderBook> _orderBookSubject = new ReplaySubject<OrderBook>();
        private readonly ReplaySubject<AccountBalance> _balanceSubject = new ReplaySubject<AccountBalance>();
        private readonly Subject<OrderStatus> _ordersSubject = new Subject<OrderStatus>();

        private readonly ConcurrentDictionary<string, Order> _request = new ConcurrentDictionary<string, Order>();

        public CexioReactiveExchangeGateway(string key, string secret, IEnumerable<CurrencyPair> tickers)
        {
            _tickers = tickers.ToList();
            _key = key;
            _secret = secret;
            _client = new ReactiveWebSocket(WebsocketApi);
            _subscription = _client.Connect().Subscribe(MessageHandler);
        }

        private async void MessageHandler(string message)
        {
            buffer[id++ % 10] = message;

            var wrapper = JsonConvert.DeserializeObject<CexioMessageWrapper>(message);
            if (wrapper == null)
                return;
            switch (wrapper.e)
            {
                case "connected":
                    await Authorize();
                    break;
                case "auth":
                    if (wrapper.ok == "ok")
                    {
                        foreach (var ticker in _tickers)
                            await SubscribeToOrderBook(ticker);
                        await RequestBalance();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case "ping":
                    await Pong();
                    break;
                case "order-book-subscribe":
                    var orderBook = JsonConvert.DeserializeObject<Messaging.CexioOrderBook>(Convert.ToString(wrapper.data));
                    ReadOrderBook(orderBook);
                    break;
                case "md_update":
                    var mdupdate = JsonConvert.DeserializeObject<CexioMarketDataUpdate>(Convert.ToString(wrapper.data));
                    UpdateOrderBook(mdupdate);
                    break;
                case "get-balance":
                    var getbalance = JsonConvert.DeserializeObject<CexioBalance>(Convert.ToString(wrapper.data));
                    PublishBalance(getbalance);
                    break;
                case "place-order":
                    var placement = JsonConvert.DeserializeObject<OrderPlacementResponse>(Convert.ToString(wrapper.data));
                    PublishOrder(wrapper.oid, wrapper.ok == "ok", placement);
                    break;
                case "cancel-order":
                    var cancellation = JsonConvert.DeserializeObject<OrderCancellationResponse>(Convert.ToString(wrapper.data));
                    PublishOrder(wrapper.oid, wrapper.ok == "ok", cancellation);
                    break;
                case "disconnected":
                    break;
                case "order":
                    // ignore for now
                    break;
                case "balance":
                    // ignore for now
                    break;
                case "obalance":
                    // ignore for now
                    break;
                case "tx":
                    // ignore for now
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void PublishOrder(string oid, bool ok, OrderPlacementResponse placement)
        {
            Order order = null;
            if (!_request.TryRemove(oid, out order))
                throw new NotImplementedException();
            var orderStatus = ok ? new OrderStatus(order.Id, placement.id, true) : new OrderStatus(order.Id, placement.error);
            _ordersSubject.OnNext(orderStatus);
        }

        private void PublishOrder(string oid, bool ok, OrderCancellationResponse placement)
        {
            Order order = null;
            if (!_request.TryRemove(oid, out order))
                throw new NotImplementedException();
            var orderStatus = new OrderStatus(order.Id, placement.order_id, true, ok);
            _ordersSubject.OnNext(orderStatus);
        }

        private void PublishBalance(CexioBalance getbalance)
        {
            var accountBalance = new AccountBalance()
            {
                Bitcoin = getbalance.balance["BTC"],
                Ethereum = getbalance.balance["ETH"],
                BitcoinCash = getbalance.balance["BCH"],
                Euro = getbalance.balance["EUR"]
            };
            _balanceSubject.OnNext(accountBalance);
        }

        private async Task RequestBalance()
        {
            var balance = JsonConvert.SerializeObject(new
            {
                e = "get-balance",
                data = new { },
                oid = GetOID("get-balance")
            });
            await _client.Publish(balance);
        }

        private void UpdateOrderBook(CexioMarketDataUpdate marketDataUpdate)
        {
            var cexioOrderBook = _orderBooks[marketDataUpdate.pair];
            cexioOrderBook.Update(marketDataUpdate);
            PublishOrderBook(cexioOrderBook);
        }

        private void ReadOrderBook(Messaging.CexioOrderBook orderBook)
        {
            _orderBooks.Add(orderBook.pair, orderBook);
            PublishOrderBook(orderBook);
        }

        private async Task Pong()
        {
            var pong = JsonConvert.SerializeObject(new
            {
                e = "pong"
            });
            await _client.Publish(pong);
        }

        private async Task SubscribeToOrderBook(CurrencyPair ticker)
        {
            var orderBook = JsonConvert.SerializeObject(new
            {
                e = "order-book-subscribe",
                data = new
                {
                    pair = new[] { ticker.Base, ticker.Quote },
                    subscribe = true,
                    depth = 10,
                },
                oid = GetOID("order-book-subscribe")
            });
            await _client.Publish(orderBook);
        }

        private string GetOID(string orderBookSubscribe)
        {
            return DateTime.UtcNow.ToUnixTimestamp() + "_" + _messageId++ + "_" + orderBookSubscribe;
        }

        private async Task Authorize()
        {
            var timestamp = DateTime.UtcNow.ToUnixTimestamp();
            var authorisation = JsonConvert.SerializeObject(new
            {
                e = "auth",
                auth = new
                {
                    key = _key,
                    signature = SignHelper.HmacSha256(_secret, timestamp + _key),
                    timestamp
                }
            });
            await _client.Publish(authorisation);
        }

        public IObservable<OrderBook> GetMarketData()
        {
            return _orderBookSubject;
        }

        public async void AddOrder(Order order)
        {
            var value = new
            {
                e = "place-order",
                data = new
                {
                    pair = new[] { order.Pair.Base, order.Pair.Quote },
                    amount = order.Volume,
                    price = order.Price,
                    type = ConvertToDto(order.Side)
                },
                oid = DateTime.UtcNow.ToUnixTimestamp() + "_" + _messageId++ + "_place-order"
            };
            var orderPlacement = JsonConvert.SerializeObject(value: value);
            _request.TryAdd(value.oid, order);
            await _client.Publish(orderPlacement);
        }

        private string ConvertToDto(Side orderSide)
        {
            switch (orderSide)
            {
                case Side.Bid:
                    return "buy";
                case Side.Ask:
                    return "sell";
                default:
                    Debug.Fail("Unkown type");
                    throw new NotImplementedException();
            }
        }

        private void PublishOrderBook(Messaging.CexioOrderBook orderBook)
        {
            _orderBookSubject.OnNext(ToDomain(orderBook));
        }

        private OrderBook ToDomain(Messaging.CexioOrderBook orderBook)
        {
            //var timestamp = DateTimeUtilities.FromUnixTimestamp(orderBook.timestamp);
            var dateTime = DateTime.UtcNow;

            return new OrderBook(CexioConverters.ConvertTicker(orderBook.pair), orderBook.bids, orderBook.asks, dateTime);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        public string Name => "Cexio";
        public IObservable<AccountBalance> GetBalance()
        {
            return _balanceSubject;
        }

        public IObservable<OrderStatus> GetOrders()
        {
            return _ordersSubject;
        }

        public async void CancelOrder(Order order)
        {
            var value = new
            {
                e = "cancel-order",
                data = new
                {
                    order_id = order.ExchangeOrderId,
                },
                oid = GetOID("cancel-order")
            };
            var orderCancelation = JsonConvert.SerializeObject(value: value);
            _request.TryAdd(value.oid, order);
            await _client.Publish(orderCancelation);

        }
    }
}
