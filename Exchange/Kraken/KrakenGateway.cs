using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CIB.Exchange.Kraken.DTO;
using CIB.Exchange.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderBookDto = CIB.Exchange.Kraken.DTO.OrderBookDto;

namespace CIB.Exchange.Kraken
{
    public class KrakenExchangeGateway : IExchangeGateway
    {
        private const string ApiUrl = "https://api.kraken.com";
        private const string TimeApi = "/0/public/Time";
        private const string TickerApi = "/0/public/pair";
        private const string OhlcApi = "0/public/OHLC";
        private const string OrderBookApi = "/0/public/Depth";
        private const string AddOrderApi = "/0/private/AddOrder";
        private const string CancelOrderApi = "/0/private/CancelOrder";
        private const string AccountBalanceApi = "/0/private/Balance";

        private const string Bitcoin = "XXBT";
        private const string Ethereum = "XETH";
        private const string Euro = "ZEUR";
        //private const string BtcEur = Bitcoin + Euro;

        public string Name { get; } = "Kraken";

        private readonly string _key;
        private readonly byte[] _secret;
        private HttpClient _client;


        public KrakenExchangeGateway()
        {
            _client = CreateHttpClient();
        }

        public KrakenExchangeGateway(string key, string secret)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (secret == null) throw new ArgumentNullException(nameof(secret));
            _key = key;
            _secret = Convert.FromBase64String(secret);
            _client = CreateHttpClient();
        }

        public List<Quote> GetQuote(IEnumerable<CurrencyPair> tickers)
        {
            var convertTickers = tickers.Select(KrakenConverters.ConvertTicker);
            var reqs = "pair=" + string.Join(",", convertTickers);
            var json = CallApi(TickerApi, reqs, HttpMethod.Get).Result;
            var result = JsonConvert.DeserializeObject<RootObjectDto<QuoteDto>>(json);

            if (result.error.Count == 0)
            {
                return ToDomain(tickers, result.result).ToList();
            }
            throw new NotImplementedException();
        }

        public List<OHLC> GetOhlc(CurrencyPair ticker, int interval)
        {
            var convertTicker = KrakenConverters.ConvertTicker(ticker);
            var reqs = "pair=" + convertTicker + "&interval=" + interval;
            var json = CallApi(OhlcApi, reqs, HttpMethod.Get).Result;
            var result = JsonConvert.DeserializeObject<RootObjectDto<object>>(json);

            if (result.error.Count == 0)
            {
                return ToDomain(ticker, result.result).ToList();
            }
            throw new NotImplementedException();
        }

        private IEnumerable<OHLC> ToDomain(CurrencyPair ticker, Dictionary<string, object> result)
        {
            var convertTicker = KrakenConverters.ConvertTicker(ticker);
            var jarray = result[convertTicker] as JArray;
            var data = jarray.ToObject<string[][]>();
            foreach (var d in data)
            {
                var timestamp = DateTimeUtilities.FromUnixTimestamp(long.Parse(d[0]));
                yield return new OHLC(Name, ticker, timestamp, decimal.Parse(d[1]), decimal.Parse(d[2]), decimal.Parse(d[3]), decimal.Parse(d[4])); 
            }
        }

        private IEnumerable<Quote> ToDomain(IEnumerable<CurrencyPair> tickers, Dictionary<string, QuoteDto> resultResult)
        {
            foreach (var ticker in tickers)
            {
                var convertTicker = KrakenConverters.ConvertTicker(ticker);
                var quote = resultResult[convertTicker].ToDomain(ticker);
                yield return quote;
            }
        }

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            var convertTicker = KrakenConverters.ConvertTicker(ticker);
            var reqs = "pair=" + convertTicker;
            var json = CallApi(OrderBookApi, reqs, HttpMethod.Get).Result;
            var result = JsonConvert.DeserializeObject<RootObjectDto<OrderBookDto>>(json);

            if (result.error.Count == 0)
            {
                return result.result[convertTicker].ToDomain(ticker);
            }
            throw new NotImplementedException();
        }

        public AccountBalance GetBalance()
        {
            VerifyHasKey();
            return GetBalanceInner(0);
        }

        private AccountBalance GetBalanceInner(int attempt)
        {
            // generate a 64 bit nonce using a timestamp at tick resolution
            Int64 nonce = DateTime.UtcNow.Ticks;

            var json = CallApi(AccountBalanceApi, null, HttpMethod.Post, nonce).Result;
            var result = JsonConvert.DeserializeObject<RootObjectDto<decimal>>(json);

            if (result.error.Count == 0)
            {
                return new AccountBalance()
                {
                    Bitcoin = result.result["XXBT"],
                    Ethereum = result.result["XETH"],
                    BitcoinCash = result.result["BCH"],
                    Euro = result.result["ZEUR"]
                };
            }
            // issues with parallel execution:
            if (result.error[0].Contains("nonce") && attempt < 3)
                return GetBalanceInner(attempt+1);

            throw new NotImplementedException();
        }

        private void VerifyHasKey()
        {
            if (string.IsNullOrEmpty(_key))
                throw new InvalidOperationException("Call to private api without key");
        }

        public OrderStatus AddOrder(Order order)
        {
            var reqs = BuildParameters(order);

            // generate a 64 bit nonce using a timestamp at tick resolution
            Int64 nonce = DateTime.UtcNow.Ticks;

            try
            {
                var json = CallApi(AddOrderApi, reqs, HttpMethod.Post, nonce).Result;
                var result = JsonConvert.DeserializeObject<OrderConfirmationRootDto>(json);

                if (result.error.Count == 0)
                {
                    Debug.Assert(result.result.txid.Length == 1);
                    order.ExchangeOrderId = result.result.txid[0];
                    return new OrderStatus(order.Id, result.result.txid[0], OrderState.Accepted);
                }
                else
                {
                    Debug.Assert(result.error.Count == 1);
                    return new OrderStatus(order.Id, null, OrderState.RejectedByExchange, result.error[0]);
                }
            }
            catch (Exception e)
            {
                return new OrderStatus(order.Id, null, OrderState.RejectedByExchange);
            }

            throw new NotImplementedException();
        }

        private string BuildParameters(Order order)
        {
            var convertTicker = KrakenConverters.ConvertTicker(order.Pair);
            var side = KrakenConverters.ConvertDirection(order.Side);
            var orderType = KrakenConverters.ConvertOrderType(order.Type);
            string reqs = $"pair={convertTicker}&type={side}&ordertype={orderType}&volume={order.Volume}";
            if (order.Type == OrderType.Limit)
                reqs += string.Format("&price={0}", order.Price);
            return reqs;
        }

        private string BuildApiUri(string api, string parameters)
        {
            return api + (parameters != null ? "?" + parameters : "");
        }

        private async Task<string> CallApi(string api, string reqs, HttpMethod method, long? nonce = null)
        {
            var requestUri = BuildApiUri(api, reqs);
            if (nonce.HasValue)
            {
                requestUri = api;
                reqs = "nonce=" + nonce + (reqs != null ? "&" + reqs : "");
            }

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            if (nonce.HasValue)
            {
                request.Content = new StringContent(reqs, Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Headers.Add("API-Key", _key);
                var signature = GetSignature(api, reqs, nonce.Value);
                request.Headers.Add("API-Sign", Convert.ToBase64String(signature));
            }

            var response = await _client.SendAsync(request);
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        private byte[] GetSignature(string api, string reqs, long nonce)
        {
            var np = nonce + Convert.ToChar(0) + reqs;
            var hash256Bytes = sha256_hash(np);

            var pathBytes = Encoding.UTF8.GetBytes(api);
            var buf = new byte[pathBytes.Length + hash256Bytes.Length];
            pathBytes.CopyTo(buf, 0);
            hash256Bytes.CopyTo(buf, pathBytes.Length);
            return getHash(_secret, buf);
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(ApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private byte[] sha256_hash(String value)
        {
            using (var algorithm = SHA256.Create())
            {
                // Create the at_hash using the access token returned by CreateAccessTokenAsync.
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

                return hash;
                //// Note: only the left-most half of the hash of the octets is used.
                //// See http://openid.net/specs/openid-connect-core-1_0.html#CodeIDToken
                //identity.AddClaim(JwtRegisteredClaimNames.AtHash, Base64UrlEncoder.Encode(hash, 0, hash.Length / 2));
            }
        }

        private byte[] getHash(byte[] keyByte, byte[] messageBytes)
        {
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {

                Byte[] result = hmacsha512.ComputeHash(messageBytes);

                return result;

            }
        }

        public OrderStatus CancelOrder(Order order)
        {
            var reqs = "txid=" + order.ExchangeOrderId;
            // generate a 64 bit nonce using a timestamp at tick resolution
            Int64 nonce = DateTime.UtcNow.Ticks;

            try
            {
                var json = CallApi(CancelOrderApi, reqs, HttpMethod.Post, nonce).Result;
                var result = JsonConvert.DeserializeObject<OrderCancellationRootDto>(json);

                if (result.error.Count == 0)
                {
                    Debug.Assert(result.result.count == 1);
                    return new OrderStatus(order.Id, order.ExchangeOrderId, OrderState.Cancelled);
                }
                else
                {
                    Debug.Assert(result.error.Count == 1);
                    return new OrderStatus(order.Id, order.ExchangeOrderId, OrderState.CancelReject, result.error[0]);
                }
            }
            catch (Exception e)
            {
                return new OrderStatus(order.Id, order.ExchangeOrderId, OrderState.CancelReject);
            }

            throw new NotImplementedException();
        }
    }
}
