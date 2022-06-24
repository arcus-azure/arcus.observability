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
            var config = builder.GetContext().Configuration;

            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.RemoveMicrosoftApplicationInsightsLoggerProvider()
                              .AddSerilog(logger);
            });
        }
    }
}
