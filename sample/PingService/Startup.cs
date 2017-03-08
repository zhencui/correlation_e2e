using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Net.Http.Headers;

namespace PingService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //temporary, simulates ASP.NET Core Activity creation and events
            AspNetCoreTmp.AspNetDiagnosticListener.Enable();

            app.Run(async (context) =>
            {
                context.Response.Headers.Add("Request-Id", Activity.Current.Id);
                if (Activity.Current.Baggage.Any())
                {
                    string[] correlationContext = Activity.Current.Baggage.Select(item => new NameValueHeaderValue(item.Key, item.Value).ToString()).ToArray();
                    context.Response.Headers.Add("Correlation-Context", new StringValues(correlationContext));
                }
                await context.Response.WriteAsync("Ping!");
            });
        }
    }
}
