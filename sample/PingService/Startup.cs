using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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

            app.UseDeveloperExceptionPage();

            //temporary, simulates ASP.NET Core Activity creation and events
            AspNetCoreTmp.AspNetDiagnosticListener.Enable();


            int count = 0;
            app.Run(async (context) =>
            {
                if (count++ % 5 == 0)
                {
                    throw new Exception("Unexpected S3 error occurred");
                }

                await context.Response.WriteAsync("Ping!");
            });
        }
    }
}
