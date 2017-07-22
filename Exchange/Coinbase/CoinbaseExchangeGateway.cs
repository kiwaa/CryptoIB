using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CIB.Exchange.Coinbase.DTO;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.Coinbase
{
    public class CoinbaseExchangeGateway : IExchangeGateway
    {
        private string PublicApiUrl = "https://api.gdax.com/";
        private string TickerApi = "/products/{0}/ticker";
        private string OrderBookApi = "/products/{0}/book?level=2";
        private string FullOrderBookApi = "/products/{0}/book?level=3";

        //private const int OrderBookDepth = 10;
        public string Name => "GDAX";

        public List<Quote> GetQuote(IEnumerable<CurrencyPair> tickers)
        {
            var result = new List<Quote>();
            foreach (var ticker in tickers)
            {
                result.Add(GetQuote(ticker));
            }
            return result;
        }

        private Quote GetQuote(CurrencyPair ticker)
        {
            var convertTicker = CoinbaseConverter.ConvertTicker(ticker);
            var api = string.Format(TickerApi, convertTicker);
            var json = QueryPublic(api).Result;
            var result = JsonConvert.DeserializeObject<CoinbaseQuote>(json);

            if (result.message == null)
                return ToDomain(ticker, result);

            throw new NotImplementedException();
        }

        private Quote ToDomain(CurrencyPair ticker, CoinbaseQuote result)
        {
            //var convertTicker = CoinbaseConverter.ConvertTicker(ticker);
            //var cexioQuote = result.SingleOrDefault(r => r.pair == convertTicker);
            return new Quote("GDAX", ticker, decimal.Parse(result.bid), decimal.Parse(result.ask), DateTime.Parse(result.time));
        }

        private async Task<string> QueryPublic(string requestUri)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(PublicApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var response = await client.GetAsync(requestUri);
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            var convertTicker = CoinbaseConverter.ConvertTicker(ticker);
            var api = string.Format(OrderBookApi, convertTicker);

            var json = QueryPublic(api).Result;
            var result = JsonConvert.DeserializeObject<CoinbaseOrderBook1>(json);


            return new OrderBook(ticker, ConvertToDomain(result.asks, result.bids), DateTime.UtcNow);
            //if (string.IsNullOrEmpty(result.error))
            //{
            //}
            //throw new NotImplementedException();
        }

        public CoinbaseFullOrderBook GetFullOrderBook(CurrencyPair ticker)
        {
            var convertTicker = CoinbaseConverter.ConvertTicker(ticker);
            var api = string.Format(FullOrderBookApi, convertTicker);

            var json = QueryPublic(api).Result;
            return JsonConvert.DeserializeObject<CoinbaseFullOrderBook>(json);
        }

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

        public AccountBalance GetBalance()
        {
            throw new NotImplementedException();
        }

        public OrderStatus AddOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public OrderStatus CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }
    }

    //public class CoinbaseOrderBook
    //{
    //        public long sequence { get; set; }
    //        public List<List<decimal>> bids { get; set; }
    //        public List<List<decimal>> asks { get; set; }
    //}

    //internal class CoinbaseQuote
    //{
    //    public string message { get; set; }
    //    public int trade_id { get; set; }
    //    public string price { get; set; }
    //    public string size { get; set; }
    //    public string bid { get; set; }
    //    public string ask { get; set; }
    //    public string volume { get; set; }
    //    public string time { get; set; }

    //}

    //internal class CoinbaseConverter
    //{
    //    public static string ConvertTicker(CurrencyPair arg)
    //    {
    //        return arg.Base + "-" + arg.Quote;
    //    }
    //}
}
