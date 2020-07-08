using Arcus.Observability.Tests.Unit.Telemetry.AzureFunctions.Fixture;
using Microsoft.Azure.WebJobs.Script.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.AzureFunctions
{
    public class ILoggerBuilderExtensionsTests
    {
        [Fact]
        public void ClearProvidersExceptFunctions_WithLoggerBuilderContainingProviders_RemovesNonFunctionProviders()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerProvider, HostFileLoggerProvider>();
            services.AddSingleton<ILoggerProvider, FunctionFileLoggerProvider>();
            services.AddSingleton<ILoggerProvider, TestLoggerProvider>();
            var builder = new Mock<ILoggingBuilder>();
            builder.Setup(b => b.Services).Returns(services);

            // Act
            builder.Object.ClearProvidersExceptFunctionProviders();

            // Assert
            Assert.NotEmpty(services);
            Assert.Equal(2, services.Count);
            Assert.All(services, desc => Assert.NotEqual(typeof(TestLoggerProvider), desc.ImplementationType));
        }
    }
}
