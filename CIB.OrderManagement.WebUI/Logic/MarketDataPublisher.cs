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
                var disposable = exchange.GetMarketData()
                    .DistinctUntilChanged(new QuoteValueComparer(QuoteValueComparer.PriceComparison.BidAndAsk))
                    .Subscribe(OnQuote);
                subscriptions.Add(disposable);
            }
            _allSubscription = new CompositeDisposable(subscriptions);
        }

        private async void OnQuote(Quote quote)
        {
            var dto = QuoteDto.FromDomain(quote);
            await _quotesHub.Clients.All.InvokeAsync("New", dto);
        }

        public void Dispose()
        {
            _allSubscription?.Dispose();
        }
    }
}
