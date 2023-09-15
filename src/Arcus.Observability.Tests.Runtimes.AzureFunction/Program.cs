using System.Diagnostics;
using Arcus.Observability.Correlation;
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

                        builder.Services.AddCorrelation(_ => new ActivityCorrelationInfoAccessor());
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
                                 .WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, instrumentationKey);
                    })
                    .Build();

            host.Run();
        }
    }

    /// <summary>
    /// Represents an <see cref="ICorrelationInfoAccessor"/> implementation that solely retrieves the correlation information from the <see cref="Activity.Current"/>.
    /// Mostly used for places where the Application Insights is baked in and there is no way to hook in custom Arcus functionality.
    /// </summary>
    internal class ActivityCorrelationInfoAccessor : ICorrelationInfoAccessor
    {
        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        public CorrelationInfo GetCorrelationInfo()
        {
            var activity = Activity.Current;
            if (activity == null)
            {
                return null;
            }

            if (activity.IdFormat == ActivityIdFormat.W3C)
            {
                string operationParentId = DetermineW3CParentId(activity);
                return new CorrelationInfo(
                    activity.SpanId.ToHexString(),
                    activity.TraceId.ToHexString(),
                    operationParentId);
            }

            return new CorrelationInfo(
                activity.Id,
                activity.RootId,
                activity.ParentId);
        }

        private static string DetermineW3CParentId(Activity activity)
        {
            if (activity.ParentSpanId != default)
            {
                return activity.ParentSpanId.ToHexString();
            }
            
            if (!string.IsNullOrEmpty(activity.ParentId))
            {
                // W3C activity with non-W3C parent must keep parentId
                return activity.ParentId;
            }

            return null;
        }

        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        public void SetCorrelationInfo(CorrelationInfo correlationInfo)
        {
            throw new InvalidOperationException(
                "Cannot set new correlation information in Azure Functions in-process model");
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