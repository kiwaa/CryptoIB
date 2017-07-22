using System.Collections.Generic;
using CIB.Exchange.Model;

namespace Edge
{
    public class Configuration
    {
        public readonly decimal OrderBookFactor = 3m;

        public readonly decimal SpreadEntry = 0.01m; // fees ~1%
        public readonly decimal MarketRebalanceSpread = 0.005m;
        public bool AllowRebalance = true;


        public List<CurrencyPair> Pairs = new List<CurrencyPair>
        {
            new CurrencyPair("BTC", "EUR"),
            //new CurrencyPair("BTC", "USD"),
            new CurrencyPair("ETH", "BTC"),
            new CurrencyPair("ETH", "EUR"),
            //new CurrencyPair("ETH", "USD"),
            //new CurrencyPair("LTC", "BTC"),
            //new CurrencyPair("LTC", "EUR"),
            //new CurrencyPair("LTC", "USD"),
            //new CurrencyPair("DSH", "EUR"),
            //new CurrencyPair("DSH", "USD"),
        };

        //UseFullExposure=false
        //TestedExposure=25.00
        //MaxExposure=25000.00
        //MaxLength=5184000

        //# Strategy parameters
        //Interval = 3.0
        //PriceDeltaLimit=0.10
        //TrailingSpreadLim=0.0008
        //TrailingSpreadCount=1
        //OrderBookFactor=3.0

        //# Kraken
        //KrakenApiKey=
        //KrakenSecretKey=
        //KrakenFees=0.0025
        //KrakenEnable=true

        //# BTCe
        //BTCeApiKey=
        //BTCeSecretKey=
        //BTCeFees=0.0020
        //BTCeEnable=true

        //# Poloniex
        //PoloniexApiKey=
        //PoloniexSecretKey=
        //PoloniexFees=0.0020
        //PoloniexEnable=true

    }


}
