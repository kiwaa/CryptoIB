using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CIB.Exchange.BTCe.DTO;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.BTCe
{
    public class BTCeExchangeGateway : IExchangeGateway
    {
        private const string PublicApiUrl = "https://btc-e.com";
        private const string PrivateApiUrl = "https://btc-e.com/tapi";
        private const string TickerApi = "/api/3/ticker/";
        private const string OrderBookApi = "/api/3/depth/";

        private const string Bitcoin = "btc";
        private const string Ethereum = "eth";
        private const string Euro = "eur";

        public string Name { get; } = "BTC-E";

        private string _key;
        private readonly string _secret;

        public BTCeExchangeGateway()
        {
        }

        public BTCeExchangeGateway(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }

        public List<Quote> GetQuote(IEnumerable<CurrencyPair> tickers)
        {
            var converterTickers = tickers.Select(BTCeConverters.ConvertTicker);
            var reqs = string.Join("-", converterTickers);
            var json = QueryPublic(TickerApi + reqs).Result;
            var result = JsonConvert.DeserializeObject<Dictionary<string, BTCeQuote>>(json);

            if (result.Any())
            {
                return ToDomain(tickers, result).ToList();
            }
            throw new NotImplementedException();
        }

        private IEnumerable<Quote> ToDomain(IEnumerable<CurrencyPair> tickers, Dictionary<string, BTCeQuote> result)
        {
            throw new NotImplementedException();

            foreach (var ticker in tickers)
            {
                var converterTicker = BTCeConverters.ConvertTicker(ticker);
                var btceQuote = result[converterTicker];
                //yield return new Quote("BTC-e", ticker, btceQuote.buy, btceQuote.sell, new DateTime(btceQuote.updated));
            }
        }

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            var converterTicker = BTCeConverters.ConvertTicker(ticker);

            var json = QueryPublic(OrderBookApi + converterTicker).Result;
            var result = JsonConvert.DeserializeObject<Dictionary<string, BTCeOrderBook>>(json);
            if (result.Any())
            {
                var orderbook = result[converterTicker];
                //return new OrderBook(ticker, ConvertToDomain(orderbook.asks, orderbook.bids));
            }
            throw new NotImplementedException();
        }

        public AccountBalance GetBalance()
        {
            var json = CallApi("getInfo").Result;
            var result = JsonConvert.DeserializeObject<BTCeRootObject<BTCeInfo>>(json);
            if (result.success)
            {
                return new AccountBalance()
                {
                    Bitcoin = result.@return.Funds[Bitcoin],
                    Ethereum = result.@return.Funds[Ethereum],
                    Euro = result.@return.Funds[Euro]
                };
            }
            throw new NotImplementedException();
        }

        public OrderStatus AddOrder(Order order)
        {
            var reqs = "pair=" + BTCeConverters.ConvertTicker(order.Pair) + "&type=" + BTCeConverters.ConvertSide(order.Side) + "&rate=" + order.Price + "&amount=" +
                    order.Volume;
            var json = CallApi("Trade", reqs).Result;
            var result = JsonConvert.DeserializeObject<BTCeRootObject<BTCeInfo>>(json);
            throw new NotImplementedException();

            //if (result.success)
            //{
            //    // nop
            //}
            //throw new NotImplementedException();
        }

        private async Task<string> CallApi(string method, string reqs = null)
        {
            reqs = "method=" + method + reqs;
            reqs += "&nonce=" + (UInt32)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; ;

            //var data = Encoding.UTF8.GetBytes(reqs);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, PrivateApiUrl);
            request.Content = new StringContent(reqs, Encoding.UTF8, "application/x-www-form-urlencoded");

            //request.Method = "POST";
            //request.Timeout = 15000;
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;
            var signature = SignHelper.HmacSha512(_secret, reqs);

            request.Headers.Add("Key", _key);
            request.Headers.Add("Sign", signature);
            //var reqStream = request.GetRequestStream();
            //reqStream.Write(data, 0, data.Length);
            //reqStream.Close();
            //return new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

            var response = await client.SendAsync(request);
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }

        }

        //private string CreateSignature()
            //{
            //    using (var hmacsha512 = new HMACSHA512(keyByte))
            //    {
            //        Byte[] result = hmacsha512.ComputeHash(data);
            //        return ByteArrayToString().ToLower();

            //        return result;

            //    }
            //}

            //static string ByteArrayToString(byte[] ba)
            //{
            //    return BitConverter.ToString(ba).Replace("-", "");
            //}

            private IEnumerable<OrderBookLevel> ConvertToDomain(List<List<decimal>> asks, List<List<decimal>> bids)
        {
            var depth = Math.Min(asks.Count, bids.Count);
            for (int i = 0; i < depth; i++)
            {
                var ask = asks[i];
                var bid = bids[i];
                yield return new OrderBookLevel(ask[0], ask[1], bid[0], bid[1]);
            }
        }

        private async Task<string> QueryPublic(string requestUri)
        {
            //string address = string.Format("{0}/{1}/public/{2}", _url, _version, a_sMethod);

            var client = new HttpClient();
            client.BaseAddress = new Uri(PublicApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var response = await client.GetAsync(requestUri);
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        public OrderStatus CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
