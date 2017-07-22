using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CIB.Exchange
{
    public class ReactiveWebSocket
    {
        private readonly Uri _websocketUri;
        private readonly ClientWebSocket _client;

        public ReactiveWebSocket(Uri websocketUri)
        {
            if (websocketUri == null) throw new ArgumentNullException(nameof(websocketUri));
            _websocketUri = websocketUri;
            _client = new ClientWebSocket();
        }

        public IObservable<string> Connect()
        {
            return Observable.Create<string>(observer =>
            {
                return Scheduler.Default.ScheduleAsync(async (ctrl, ct) => await Connect(observer, ct));
            });
        }

        public async Task Publish(string message)
        {
            var bytes = ToBytes(message);

            while (_client.State != WebSocketState.Open)
            {
                // active waiting
            }
            await _client.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Connect(IObserver<string> observer, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _client.ConnectAsync(_websocketUri, cancellationToken);

                while (_client.State == WebSocketState.Open)
                {
                    var message = await ReceiveMessage(cancellationToken, _client);
                    observer.OnNext(message);
                }
            }
        }

        private static async Task<string> ReceiveMessage(CancellationToken cancellationToken, ClientWebSocket webSocketClient)
        {
            const int bufferSize = 1024;
            using (var stream = new MemoryStream(bufferSize))
            {
                var receiveBuffer = new ArraySegment<byte>(new byte[bufferSize * 8]);
                WebSocketReceiveResult webSocketReceiveResult;
                do
                {
                    webSocketReceiveResult = await webSocketClient.ReceiveAsync(receiveBuffer, cancellationToken);
                    await stream.WriteAsync(receiveBuffer.Array, receiveBuffer.Offset, webSocketReceiveResult.Count, cancellationToken);
                } while (!webSocketReceiveResult.EndOfMessage);

                var message = stream.ToArray().Where(b => b != 0).ToArray();
                return Encoding.ASCII.GetString(message, 0, message.Length);
            }
        }

        private static ArraySegment<byte> ToBytes(string requestString)
        {
            var requestBytes = Encoding.UTF8.GetBytes(requestString);
            return new ArraySegment<byte>(requestBytes);
        }
    }
}
