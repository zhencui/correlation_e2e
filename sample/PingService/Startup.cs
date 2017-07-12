using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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

            int count = 0;
            app.Run(async (context) =>
            {
                var telemetryClient = new TelemetryClient();
                telemetryClient.TrackTrace(new TraceTelemetry("Go step 1"));

                telemetryClient.TrackTrace(new TraceTelemetry("Go step 2"));

                telemetryClient.TrackTrace(new TraceTelemetry("Go step 3"));
                if (count++ % 5 == 0)
                {
                    telemetryClient.TrackTrace(new TraceTelemetry("Boom!"));
                    throw new Exception("Unexpected S3 error occurred");
                }


                telemetryClient.TrackTrace(new TraceTelemetry("Go step 4"));
                telemetryClient.TrackTrace(new TraceTelemetry("Go step 5"));

                await context.Response.WriteAsync("Ping!");
            });
        }
    }
}
