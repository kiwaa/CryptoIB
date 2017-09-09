using System;
using System.Collections.Generic;
using System.Linq;
using CIB.Exchange;
using CIB.Exchange.Cexio;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Hubs;
using CIB.OrderManagement.WebUI.Logic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CIB.OrderManagement.WebUI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());

            // Add framework services.
            services.AddSignalR(options => options.JsonSerializerSettings = settings);
            services.AddMvc();

            var reactiveExchangeGateways = GetRoutes();
            services.AddSingleton<IOrderManagement>(s => new OrderManagementS(reactiveExchangeGateways.ToDictionary(x => x.Name)));
            services.AddSingleton(s => new MarketDataPublisher(reactiveExchangeGateways, s.GetService<IHubContext<QuotesHub>>()));
            services.AddSingleton<OrderStorage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseWebSockets();
            app.UseSignalR(routes =>
            {
                routes.MapHub<OrdersHub>("orders");
                routes.MapHub<QuotesHub>("quotes");
            });
            app.ApplicationServices.GetService<MarketDataPublisher>();
        }

        private IEnumerable<IReactiveExchangeGateway> GetRoutes()
        {
            var tickers = new[]
            {
                new CurrencyPair("BTC", "EUR"),
                new CurrencyPair("ETH", "EUR"),
                //new CurrencyPair("LTC", "EUR")
            };
            yield return CreateGateway("Kraken", tickers);
            yield return CreateGateway("CEX", tickers);
        }

        private IReactiveExchangeGateway CreateGateway(string exchange, CurrencyPair[] tickers)
        {
            IReactiveExchangeGateway gateway = CreateExchangeGateway(exchange, tickers);
            if (Configuration["demo"] == "false")
                gateway = new DemoReactiveExchangeGateway(gateway);
            return gateway;
        }

        private IReactiveExchangeGateway CreateExchangeGateway(string exchange, CurrencyPair[] tickers)
        {
            switch (exchange)
            {
                case "Kraken":
                    return new ReactiveExchangeGatewayAdapter(new KrakenExchangeGateway(Configuration["kraken:key"], Configuration["kraken:secret"]), tickers);
                case "CEX":
                    return new CexioReactiveExchangeGateway(Configuration["cexio:key"], Configuration["cexio:secret"], tickers);
                default:
                    throw new NotSupportedException();
            }
        }

    }
}
