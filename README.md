# Correlation End-to-End Demo
This repo demonstrates AppInsights prototype with Activity (correlation) support and gives instruction on how to use it in client application.
You may create your own services from the template or use samples. 

## Prerequisites
* [VisualStudio 2017](https://www.visualstudio.com/downloads/)
* [dotnet cli 2.0.0](https://github.com/dotnet/cli#installers-and-binaries)

## Creating an application
1. Clone this repo. It contains [template](https://github.com/lmolkova/correlation_e2e/tree/master/template) project that is configured to run on .NET core 2.0 and simulates correlation support in ASP.NET Core. It is self-contained and could be deployed as Azure Web App. It also has AppInsights prototype with correlation support pre-installed.

2. Create back-end service from the template; sample: [ping service](https://github.com/lmolkova/correlation_e2e/tree/master/sample/PingService)
  * Configure AppInsights: add instrumentation key to `appsettings.json`
  ```json
    "ApplicationInsights": {
      "InstrumentationKey": "**your app insights instrumentation key**"
    }
  ```
  * Publish it to Azure WebApp or run locally

3. Create front-end service from the tempate; sample [front-end service](https://github.com/lmolkova/correlation_e2e/tree/master/sample/FrontEnd)
  * Configure AppInsights similarly to front-end service. Note that AppInsights usage model assumes instrumentation-key (and resource) per-application. So backend and frontend would need different keys.
  * Enable ApplicationInsights dependency (outgoing HTTP requests) tracing. 
  
    ```C#
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
         Microsoft.ApplicationInsights.DependencyCollector.DependencyCollectorDiagnosticListener.Enable();
         ...
    }
    ```
    
 * Call backend service on incoming request:
 
   ```C#
    app.Run(async (context) =>
    {
        var response = await client.GetAsync("http://<your back end url>/");
        await context.Response.WriteAsync("Hello World!");
    });
   ```
   
 * Publish it or run locally

## Sample services
You can play with sample services
- http://frontend-e2e.azurewebsites.net (front-end) and its [AppInsights](https://ms.portal.azure.com/#resource/subscriptions/94a08dfb-8ec7-4234-af9c-0e91eb2045c1/resourceGroups/correlation/providers/microsoft.insights/components/correlation_end2end/overview)
- http://pingservice-e2e.azurewebsites.net (back-end) and its [Appinsights](https://ms.portal.azure.com/#resource/subscriptions/94a08dfb-8ec7-4234-af9c-0e91eb2045c1/resourceGroups/correlation/providers/microsoft.insights/components/cloudsample/overview)

Please let @lmolkova know if you want to access their AppInsights resources in the portal, I'll give you access.

## Demo

#### 1. Call front-end service from the browser

#### 2. Go to front-end Application Insights resource in http://ms.portal.azure.com/
  * Open Search blade, wait for the events to appear
  
![](https://cloud.githubusercontent.com/assets/2347409/23688963/785a965c-036c-11e7-906b-270042f3940b.PNG)

  * Click on any event: blade with event description opens, select "All available telemetry for this root operation"
   
![](https://cloud.githubusercontent.com/assets/2347409/23688967/787609dc-036c-11e7-867a-d97b00159fd3.PNG)
Note:
  * there is one request event from frontend (request from backend appears in backend AppInsights resource). 
  * there is dependency event for backend call
  * traces also share the same operation Id and could be correlated.

#### 3. Search by Request-Id

![](https://cloud.githubusercontent.com/assets/2347409/23688965/78623aec-036c-11e7-8d85-29cc8124af86.PNG)

#### 4. Analytics (Kusto)
```
union requests, dependencies, traces | where operation_Id == "RD0003FF21406D-18b39d3-1"

```
![](https://cloud.githubusercontent.com/assets/2347409/23688966/786f2eaa-036c-11e7-8703-568d2f02dafb.PNG)

## Known Issues
1. Dependency tracing should be enabled be default, users should not explicitly call `DependencyCollectorDiagnosticListener.Enable()`; Implementation is planned in AppInsights
2. "Request starting" traces do not have any context: this trace is written be AspNetCore and will have context after AspNetCore will implement correlation support.
