using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.Cexio
{
    public class CexioExchangeGateway : IExchangeGateway
    {
        private string PublicApiUrl = "https://cex.io";
        private string TickerApi = "/api/tickers/";
        private string OrderBookApi = "/api/order_book/";

        private const int OrderBookDepth = 10;
        public string Name => "Cexio";

        public List<Quote> GetQuote(IEnumerable<CurrencyPair> tickers)
        {
            var uniq = new HashSet<string>();
            foreach (var ticker in tickers)
            {
                uniq.Add(ticker.Base);
                uniq.Add(ticker.Quote);
            }
            var reqs = string.Join("/", uniq);
            var json = QueryPublic(TickerApi + "/" + reqs).Result;
            var result = JsonConvert.DeserializeObject<RootObject<CexioQuote>>(json);

            if (result.ok == "ok")
            {
                return ToDomain(tickers, result.data).ToList();
            }
            throw new NotImplementedException();
        }

        private IEnumerable<Quote> ToDomain(IEnumerable<CurrencyPair> tickers, List<CexioQuote> result)
        {
            foreach (var ticker in tickers)
            {
                var convertTicker = CexioConverters.ConvertTicker(ticker);
                var cexioQuote = result.SingleOrDefault(r => r.pair == convertTicker);
                yield return new Quote("Cexio", ticker, cexioQuote.bid, cexioQuote.ask, DateTime.Parse(cexioQuote.timestamp));
            }
        }

        private async Task<string> QueryPublic(string requestUri)
        {
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

        public OrderBook GetOrderBook(CurrencyPair ticker)
        {
            //ETH / EUR /? depth = 10
            var converterTicker = ticker.Base + "/" + ticker.Quote;

            var json = QueryPublic(OrderBookApi + converterTicker + "/?depth=" + OrderBookDepth).Result;
            var result = JsonConvert.DeserializeObject<CexioOrderBook>(json);

            if (string.IsNullOrEmpty(result.error))
            {
                return new OrderBook(ticker, ConvertToDomain(result.asks, result.bids), new DateTime(result.timestamp));
            }
            throw new NotImplementedException();
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

    public class CexioQuote
    {
        public string timestamp { get; set; }
        public string pair { get; set; }
        public string low { get; set; }
        public string high { get; set; }
        public string last { get; set; }
        public string volume { get; set; }
        public string volume30d { get; set; }
        public decimal bid { get; set; }
        public decimal ask { get; set; }
    }

    public class CexioOrderBook
    {
        public string error { get; set; }
        public int timestamp { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
        public string pair { get; set; }
        public int id { get; set; }
        public string sell_total { get; set; }
        public string buy_total { get; set; }
    }

    public class RootObject<T>
    {
        public string e { get; set; }
        public string ok { get; set; }
        public List<T> data { get; set; }
    }

    public class CexioConverters
    {
        public static string ConvertTicker(CurrencyPair arg)
        {
            return arg.Base + ":" + arg.Quote;
        }

        public static CurrencyPair ConvertTicker(string pair)
        {
            var strings = pair.Split(new [] {":"}, StringSplitOptions.RemoveEmptyEntries);
            return new CurrencyPair(strings[0], strings[1]);
        }
    }
}
