using System;
using Arcus.Observability.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    // ReSharper disable once InconsistentNaming
    public class ILoggingBuilderExtensionsTests
    {
        [Fact]
        public void AddSerilog_WithImplementationFactory_IncludesSerilogLogger()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddCorrelation();

            // Act
            services.AddLogging(builder => builder.AddSerilog(provider =>
            {
                var accessor = provider.GetRequiredService<ICorrelationInfoAccessor>();
                return new LoggerConfiguration()
                       .Enrich.WithCorrelationInfo(accessor)
                       .WriteTo.Console()
                       .CreateLogger();
            }));

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var loggerProvider = serviceProvider.GetService<ILoggerProvider>();
            Assert.NotNull(loggerProvider);
            Assert.IsType<SerilogLoggerProvider>(loggerProvider);
            Assert.NotNull(serviceProvider.GetService<ILogger<ILoggingBuilderExtensionsTests>>());
        }

        [Fact]
        public void AddSerilog_WithoutImplementationFactory_Fails()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.AddLogging(builder => builder.AddSerilog(implementationFactory: null)));
        }
    }
}
