using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAppVersion_WithType_RegistersAppVersion()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAppVersion<DummyAppVersion>();

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<IAppVersion>();
            Assert.NotNull(service);
            Assert.IsType<StubAppVersion>(service);
        }

        [Fact]
        public void AddVersion_WithFactoryImplementation_RegistersAppVersion()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAppVersion(provider => new DummyAppVersion());

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<IAppVersion>();
            Assert.NotNull(service);
            Assert.IsType<StubAppVersion>(service);
        }

        [Fact]
        public void AddAppVersion_WithoutFactoryImplementation_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.AddAppVersion(createImplementation: null));
        }
    }
}
