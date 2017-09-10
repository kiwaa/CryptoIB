using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CIB.Exchange;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Dto;
using CIB.OrderManagement.WebUI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CIB.OrderManagement.WebUI.Logic
{
    public class MarketDataPublisher : IDisposable
    {
        private readonly IHubContext<QuotesHub> _quotesHub;
        private IDisposable _allSubscription;
        
        public MarketDataPublisher(IEnumerable<IReactiveExchangeGateway> gateways, IHubContext<QuotesHub> quotesHub)
        {
            _quotesHub = quotesHub;
            SubscribeToMarket(gateways);
        }

        private void SubscribeToMarket(IEnumerable<IReactiveExchangeGateway> gateways)
        {
            IReadOnlyList<Exchange.Exchange> exchanges = gateways.Select(x => new Exchange.Exchange(x)).ToList();
            List<IDisposable> subscriptions = new List<IDisposable>();
            foreach (var exchange in exchanges)
            {
                exchange.Subscribe();
                var disposable1 = exchange.GetMarketData()
                    .DistinctUntilChanged(new QuoteValueComparer(QuoteValueComparer.PriceComparison.BidAndAsk))
                    .Subscribe(OnQuote);

                if (exchange.Name == "Kraken")
                {
                    var disposable2 = exchange.GetMarketData()
                        .Where(x => x.Pair.Ticker == "BTC/EUR")
                        .DistinctUntilChanged(new QuoteValueComparer(QuoteValueComparer.PriceComparison.BidAndAsk))
                        .Buffer(TimeSpan.FromMinutes(1))
                        .Select(x => new OHLC(
                                x.First().Exchange,
                                x.First().Pair,
                                x.First().TimestampUtc,
                                // instead of bid, should be 'last trade'
                                x.First().Bid,
                                x.Max(_ => _.Bid),
                                x.Min(_ => _.Bid),
                                x.Last().Bid
                            ))
                        .Subscribe(OnOhlc);
                    subscriptions.Add(disposable2);
                }
                subscriptions.Add(disposable1);
            }
            _allSubscription = new CompositeDisposable(subscriptions);
        }

        private async void OnOhlc(OHLC ohlc)
        {
            var dto = new MarketDataDto() { Quote = null, Ohlc = OhlcDto.FromDomain(ohlc) };
            await _quotesHub.Clients.All.InvokeAsync("New", dto);
        }

        private async void OnQuote(Quote quote)
        {
            var dto = new MarketDataDto() { Quote = QuoteDto.FromDomain(quote), Ohlc = null };
            await _quotesHub.Clients.All.InvokeAsync("New", dto);
        }

        public void Dispose()
        {
            _allSubscription?.Dispose();
        }
    }
}
