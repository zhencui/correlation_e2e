using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace FrontEnd
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

            //enable Dependency tracking in AI, TODO: move to AppInsights initialization
            Microsoft.ApplicationInsights.DependencyCollector.DependencyCollectorDiagnosticListener.Enable();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Information); //needed?

            var logger = loggerFactory.CreateLogger("RequestLogger");
            HttpClient client = new HttpClient();
            app.Run(async (context) =>
            {
                var response = await client.GetAsync("http://pingservice-e2e.azurewebsites.net/");

                logger.LogInformation($"Got response from ping service, status: {response.StatusCode}");

                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
