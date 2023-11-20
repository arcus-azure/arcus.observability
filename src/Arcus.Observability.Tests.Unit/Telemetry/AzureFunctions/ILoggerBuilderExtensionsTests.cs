using Arcus.Observability.Tests.Unit.Telemetry.AzureFunctions.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.AzureFunctions
{
    // ReSharper disable once InconsistentNaming
    public class ILoggerBuilderExtensionsTests
    {
        [Fact]
        public void RemoveMicrosoftApplicationInsightsLoggerProvider_WithLoggerBuilderContainingProvider_RemovesProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ApplicationInsightsLoggerProvider>();

            // Act
            services.AddLogging(logging => logging.RemoveMicrosoftApplicationInsightsLoggerProvider());

            // Assert
            Assert.NotEmpty(services);
            Assert.All(services, desc => Assert.NotEqual(typeof(ApplicationInsightsLoggerProvider), desc.ImplementationType));
        }

        [Fact]
        public void RemoveMicrosoftApplicationInsightsLoggerProvider_WithoutExpectedLoggerProvider_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act / Assert
            services.AddLogging(logging => logging.RemoveMicrosoftApplicationInsightsLoggerProvider());
        }
    }
}
