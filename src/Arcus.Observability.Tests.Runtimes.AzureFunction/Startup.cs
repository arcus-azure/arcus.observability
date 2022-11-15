using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Tests.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Arcus.Observability.Tests.Runtimes.AzureFunctions
{
    internal class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigurationBuilder.AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration appConfig = builder.GetContext().Configuration;
            var instrumentationKey = appConfig.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            builder.Services.AddApplicationInsightsTelemetryWorkerService();
            builder.Services.AddLogging(logging =>
            {
                logging.RemoveMicrosoftApplicationInsightsLoggerProvider();
                logging.Services.AddSingleton<ILoggerProvider>(provider =>
                {
                    var logConfig = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, instrumentationKey);

                    return new SerilogLoggerProvider(logConfig.CreateLogger(), dispose: true);
                });
            });
        }
    }
}
