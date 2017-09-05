using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CIB.Exchange;
using CIB.Exchange.Cexio;
using CIB.Exchange.Kraken;
using CIB.Exchange.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            // Add framework services.
            services.AddMvc();

            services.AddSingleton<IOrderManagement>(s => new OrderManagementS(GetRoutes()));
        }

        private IDictionary<string, IReactiveExchangeGateway> GetRoutes()
        {
            var tickers = new CurrencyPair[]
            {
                new CurrencyPair("BTC", "EUR"),
                new CurrencyPair("ETH", "EUR"),
                //new CurrencyPair("LTC", "EUR")
            };
            return new Dictionary<string, IReactiveExchangeGateway>()
                {
                    {"Kraken", new DemoReactiveExchangeGateway(new ReactiveExchangeGatewayAdapter(new KrakenExchangeGateway(Configuration["kraken:key"], Configuration["kraken:secret"]), tickers))},
                    {"CEX", new DemoReactiveExchangeGateway(new CexioReactiveExchangeGateway(Configuration["cexio:key"], Configuration["cexio:secret"], tickers))}
                    //{"Kraken", new DemoReactiveExchangeGateway(new ReactiveExchangeGatewayAdapter(new KrakenExchangeGateway(Configuration["kraken:key"], Configuration["kraken:secret"]), tickers))},
                    //{"CEX", new DemoReactiveExchangeGateway(new CexioReactiveExchangeGateway(Configuration["cexio:key"], Configuration["cexio:secret"], tickers))}
                };

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
        }
    }
}
