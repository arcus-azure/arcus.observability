using System;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Tests.Unit.Telemetry.Logging;
using Bogus;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    // ReSharper disable once InconsistentNaming
    public class IServiceCollectionExtensionsTests
    {
        private static readonly Faker BogusGenerator = new Faker();

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
            Assert.IsType<DummyAppVersion>(service);
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
            var service = serviceProvider.GetService<IAppVersion>();
            Assert.NotNull(service);
            Assert.IsType<DummyAppVersion>(service);
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

        [Fact]
        public void AddAppName_WithComponentName_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            string componentName = BogusGenerator.Commerce.ProductName();

            // Act
            services.AddAppName(componentName);

            // Assert
            IServiceProvider provider = services.BuildServiceProvider();
            var appName = provider.GetService<IAppName>();
            Assert.NotNull(appName);
            Assert.Equal(componentName, appName.GetApplicationName());

            var initializer = provider.GetService<ITelemetryInitializer>();
            Assert.NotNull(initializer);
            var telemetry = new TraceTelemetry();
            initializer.Initialize(telemetry);
            Assert.Equal(componentName, telemetry.Context.Cloud.RoleName);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void AddAppName_WithoutComponentName_Fails(string componentName)
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.AddAppName(componentName));
        }

        [Fact]
        public void AddAppName_WithImplementationFactory_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            string componentName = BogusGenerator.Commerce.ProductName();
            var stub = new DefaultAppName(componentName);

            // Act
            services.AddAppName(provider => stub);

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var appName = serviceProvider.GetService<IAppName>();
            Assert.NotNull(appName);
            Assert.Same(stub, appName);

            var initializer = serviceProvider.GetService<ITelemetryInitializer>();
            Assert.NotNull(initializer);
            var telemetry = new TraceTelemetry();
            initializer.Initialize(telemetry);
            Assert.Equal(componentName, telemetry.Context.Cloud.RoleName);
        }

        [Fact]
        public void AddAppName_WithoutImplementationFactory_Throws()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.AddAppName(implementationFactory: null));
        }

        [Fact]
        public void AddAppVersion_WithImplementation_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            var version = BogusGenerator.System.Version().ToString();
            var stub = new Mock<IAppVersion>();
            stub.Setup(v => v.GetVersion()).Returns(version);

            // Act
            services.AddAppVersion(provider => stub.Object);

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var appVersion = serviceProvider.GetService<IAppVersion>();
            Assert.NotNull(appVersion);
            Assert.Same(stub.Object, appVersion);

            var initializer = serviceProvider.GetService<ITelemetryInitializer>();
            Assert.NotNull(initializer);
            var telemetry = new TraceTelemetry();
            initializer.Initialize(telemetry);
            Assert.Equal(version, telemetry.Context.Component.Version);
        }
    }
}
