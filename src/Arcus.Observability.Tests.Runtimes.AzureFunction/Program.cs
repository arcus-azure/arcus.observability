using Arcus.Observability.Telemetry.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Runtimes.AzureFunction
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = 
                Host.CreateDefaultBuilder(args)
                    .ConfigureFunctionsWorkerDefaults(builder =>
                    {
                        builder.UseMiddleware<RequestTrackingMiddleware>();

                        builder.Services.AddApplicationInsightsTelemetryWorkerService();
                        builder.Services.ConfigureFunctionsApplicationInsights();
                    })
                    .UseSerilog((context, provider, logConfig) =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        string instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

                        logConfig.MinimumLevel.Debug()
                                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                 .Enrich.FromLogContext()
                                 .Enrich.WithComponentName("Timer")
                                 .Enrich.WithCorrelationInfo(provider)
                                 .WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, instrumentationKey)
                                 .CreateLogger();
                    })
                    .Build();

            host.Run();
        }
    }

    public class RequestTrackingMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            using (var measurement = DurationMeasurement.Start())
            {
                try
                {
                    await next(context);
                }
                finally
                {
                    ILogger log = context.GetLogger<RequestTrackingMiddleware>();
                    log.LogCustomRequest("Timer", "Triggered", isSuccessful: true, measurement);
                }
            }
            await next(context);
        }
    }
}