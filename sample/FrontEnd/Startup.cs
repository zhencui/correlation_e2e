using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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

            app.UseDeveloperExceptionPage();

            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Information); 

            var logger = loggerFactory.CreateLogger("RequestLogger");
            HttpClient client = new HttpClient();
            app.Run(async (context) =>
            {
                for (var i = 0; i < 2; i++)
                {
                    var response = await client.GetAsync(string.Format("http://backend{0}.azurewebsites.net/", i + 3));
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"500 Internal Server Error occurred.\r\nCorrelation Id: {Activity.Current.RootId}");
                    }

                    logger.LogInformation($"Got response from ping service, status: {response.StatusCode}");
                    await context.Response.WriteAsync("Hello World!");
                }
            });
        }
    }
}
