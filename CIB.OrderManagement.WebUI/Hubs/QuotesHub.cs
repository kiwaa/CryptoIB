using System.Linq;
using System.Threading.Tasks;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Dto;
using Microsoft.AspNetCore.SignalR;

namespace CIB.OrderManagement.WebUI.Hubs
{
    public class QuotesHub : Hub
    {
        private KrakenExchangeGateway _gateway;

        public QuotesHub()
        {
            _gateway = new KrakenExchangeGateway();
        }

        public override async Task OnConnectedAsync()
        {
            var ohlcList = _gateway.GetOhlc(new CurrencyPair("BTC", "EUR"), 15);
            var dto = ohlcList.Select(OhlcDto.FromDomain);
            await Clients.Client(Context.ConnectionId).InvokeAsync("List", dto);
            await base.OnConnectedAsync();
        }
    }
}
