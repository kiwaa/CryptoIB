using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CIB.Exchange.Model;
using Newtonsoft.Json;

namespace CIB.Exchange.Bitstamp
{
    public class BitstampExchangeGateway : IExchangeGateway
    {
        private string PublicApiUrl = "https://www.bitstamp.net";
        private string TickerApi = "/api/v2/ticker/{0}/";
        private string OrderBookApi = "/api/v2/order_book/{0}/";

        public string Name => "Bitstamp";

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
            var convertTicker = ticker.Base+ticker.Quote;
            var api = string.Format(TickerApi, convertTicker);
            var json = QueryPublic(api).Result;
            var result = JsonConvert.DeserializeObject<BitstampQuote>(json);

                return ToDomain(ticker, result);

            //throw new NotImplementedException();
        }


        private Quote ToDomain(CurrencyPair ticker, BitstampQuote result)
        {
            throw new NotImplementedException();

            return new Quote("Bitstamp", ticker, decimal.Parse(result.bid), decimal.Parse(result.ask), DateTime.Parse(result.timestamp));
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
            var api = string.Format(OrderBookApi, ticker.Base + ticker.Quote);
            var json = QueryPublic(api).Result;
            var result = JsonConvert.DeserializeObject<BitstampOrderBook>(json);

            throw new NotImplementedException();
            return new OrderBook(ticker, ConvertToDomain(result.asks, result.bids), DateTime.Parse(result.timestamp));
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

    public class BitstampOrderBook
    {
        public string timestamp { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
    }

    internal class BitstampQuote
    {
        public string high { get; set; }
        public string last { get; set; }
        public string timestamp { get; set; }
        public string bid { get; set; }
        public string vwap { get; set; }
        public string volume { get; set; }
        public string low { get; set; }
        public string ask { get; set; }
        public string open { get; set; }
    }

}
