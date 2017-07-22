using System.Collections.Generic;
using CIB.Exchange;
using CIB.Exchange.Cexio;
using CIB.Exchange.Coinbase;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;

namespace CIB.Edge.ReactiveClient
{
    public static class ExchangeFactory
    {
        public static IReactiveExchangeGateway CreateKrakenExchange(List<CurrencyPair> tickers)
        {
            var key = EntryPoint.Configuration["kraken:key"];
            var secret = EntryPoint.Configuration["kraken:secret"];

            var krakenExchange = new KrakenExchangeGateway(key, secret);
            return new ReactiveExchangeGatewayAdapter(krakenExchange, tickers);
        }

        public static IReactiveExchangeGateway CreateGdaxExchange(List<CurrencyPair> tickers)
        {
            return new CoinbaseReactiveExchangeGateway(tickers);
        }

        public static IReactiveExchangeGateway CreateCexioExchange(List<CurrencyPair> tickers)
        {
            var key = EntryPoint.Configuration["cexio:key"];
            var secret = EntryPoint.Configuration["cexio:secret"];

            return new CexioReactiveExchangeGateway(key, secret, tickers);
        }

    }
}
