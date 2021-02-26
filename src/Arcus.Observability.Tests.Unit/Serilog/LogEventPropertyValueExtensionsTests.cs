using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Tests.Core;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    public class LogEventPropertyValueExtensionsTests
    {
        [Fact]
        public void CanCorrectlyParseDateTimeWithTicksFromString()
        {
            var timestamp = DateTimeOffset.Now;
            string propertyKey = "timestamp";

            var spySink = new InMemoryLogSink();
            
            ILogger logger = CreateLogger(
                spySink, config =>
                {
                    config.Enrich.WithProperty(propertyKey, timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat));
                    return config;
                });

            logger.LogWarning("test");

            var retrievedTimestamp = spySink.CurrentLogEmits.First().Properties.GetAsDateTimeOffset(propertyKey);

            Assert.Equal(timestamp, retrievedTimestamp);
            Assert.Equal(timestamp.Ticks, retrievedTimestamp.Ticks);
        }

        private static ILogger CreateLogger(ILogEventSink sink, Func<LoggerConfiguration, LoggerConfiguration> configureLoggerConfiguration = null)
        {
            LoggerConfiguration config = new LoggerConfiguration().WriteTo.Sink(sink);
            config = configureLoggerConfiguration?.Invoke(config) ?? config;
            Logger logger = config.CreateLogger();

            var factory = new SerilogLoggerFactory(logger);
            return factory.CreateLogger<ApplicationInsightsTelemetryConverterTests>();
        }
    }
}
