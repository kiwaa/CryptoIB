using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CIB.Exchange.Coinbase.Messaging;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.Coinbase
{
    public class CoinbaseReactiveExchangeGateway : IReactiveExchangeGateway, IDisposable
    {
        private readonly List<CurrencyPair> _tickers;
        private readonly Uri WebsocketApi = new Uri("wss://ws-feed.gdax.com");
        private readonly ReactiveWebSocket _client;
        private IDisposable _subscription;

        private Dictionary<string, CoinbaseRealTimeOrderBook> _orderBooks = new Dictionary<string, CoinbaseRealTimeOrderBook>();
        private ReplaySubject<OrderBook> _orderBookSubject = new ReplaySubject<OrderBook>();
        private ManualResetEventSlim _initiated = new ManualResetEventSlim(false);


        public CoinbaseReactiveExchangeGateway(List<CurrencyPair> tickers)
        {
            _tickers = tickers;
            _client = new ReactiveWebSocket(WebsocketApi);
            _subscription = _client.Connect().Subscribe(MessageHandler);

            InitAsync();
        }

        private async void InitAsync()
        {
            var subscribe = JsonConvert.SerializeObject(new
            {
                type = "subscribe",
                product_ids = _tickers.Select(CoinbaseConverter.ConvertTicker).ToArray()
            });
            await _client.Publish(subscribe);

            ReadOrderBooks();
            _initiated.Set();
        }

        private int id = 0;
        private string[] buffer = new string[10];
        private void MessageHandler(string message)
        {
            try
            {
                _initiated.Wait();
                
                buffer[id++ % 10] = message;
                if (message.Contains("received"))
                {
                    var received = JsonConvert.DeserializeObject<OrderBookReceived>(message);
                    var orderbook = _orderBooks[received.product_id];
                    orderbook.Update(received);
                }
                if (message.Contains("open"))
                {
                    var open = JsonConvert.DeserializeObject<OrderBookOpen>(message);
                    var orderbook = _orderBooks[open.product_id];
                    orderbook.Update(open);
                    PublishOrderBook(orderbook);
                }
                if (message.Contains("done"))
                {
                    var done = JsonConvert.DeserializeObject<OrderBookDone>(message);
                    var orderbook = _orderBooks[done.product_id];
                    orderbook.Update(done);
                    PublishOrderBook(orderbook);
                }
                if (message.Contains("match"))
                {
                    var match = JsonConvert.DeserializeObject<OrderBookMatch>(message);
                    var orderbook = _orderBooks[match.product_id];
                    orderbook.Update(match);
                    PublishOrderBook(orderbook);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public IObservable<OrderBook> GetMarketData()
        {
            return _orderBookSubject;
        }

        private void PublishOrderBook(CoinbaseRealTimeOrderBook orderbook)
        {
            _orderBookSubject.OnNext(orderbook.ToDomain());
        }

        private void ReadOrderBooks()
        {
            var gateway = new CoinbaseExchangeGateway();
            foreach (var ticker in _tickers)
            {
                var dto = gateway.GetFullOrderBook(ticker);
                _orderBooks.Add(CoinbaseConverter.ConvertTicker(ticker), CoinbaseRealTimeOrderBook.FromDto(ticker, dto));
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        public string Name => "GDAX";
        public IObservable<AccountBalance> GetBalance()
        {
            throw new NotImplementedException();
        }

        public void AddOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public IObservable<OrderStatus> GetOrders()
        {
            throw new NotImplementedException();
        }

        public void CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        //private static async Task<string> ReceiveMessage(CancellationToken cancellationToken, ClientWebSocket webSocketClient)
        //{
        //    using (var stream = new MemoryStream(1024))
        //    {
        //        var receiveBuffer = new ArraySegment<byte>(new byte[1024 * 8]);
        //        WebSocketReceiveResult webSocketReceiveResult;
        //        do
        //        {
        //            webSocketReceiveResult = await webSocketClient.ReceiveAsync(receiveBuffer, cancellationToken);
        //            await stream.WriteAsync(receiveBuffer.Array, receiveBuffer.Offset, receiveBuffer.Count, cancellationToken);
        //        } while (!webSocketReceiveResult.EndOfMessage);

        //        var message = stream.ToArray().Where(b => b != 0).ToArray();
        //        return Encoding.ASCII.GetString(message, 0, message.Length);
        //    }
        //}

        //private static ArraySegment<byte> ToBytes(string requestString)
        //{
        //    var requestBytes = Encoding.UTF8.GetBytes(requestString);
        //    var subscribeRequest = new ArraySegment<byte>(requestBytes);
        //    return subscribeRequest;
        //}

    }

}
