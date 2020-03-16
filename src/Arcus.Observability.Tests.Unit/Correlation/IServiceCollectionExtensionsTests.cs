using System;
using Arcus.Observability.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Correlation
{
    [Trait("Category", "Unit")]
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCorrelation_DefaultSetup_RegistersDefaultAccessor()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddCorrelation();

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
        }

        [Fact]
        public void AddCorrelation_DefaultSetupWithOptions_RegistersDefaultAccessorWithOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddCorrelation(options => options.Operation.IncludeInResponse = true);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
            Assert.NotNull(serviceProvider.GetService<IOptions<CorrelationInfoOptions>>());
        }

        [Fact]
        public void AddCorrelation_CustomAccessor_RegistersDefaultAccessor()
        {
            // Arrange
            var services = new ServiceCollection();
            var stubAccessor = Mock.Of<ICorrelationInfoAccessor>();
            services.AddCorrelation(stubAccessor);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsNotType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
            Assert.NotNull(Mock.Get(correlationInfoAccessor));
        }

        [Fact]
        public void AddCorrelation_CustomAccessorWithOptions_RegistersCustomAccessorWithOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var stubAccessor = Mock.Of<ICorrelationInfoAccessor>();
            services.AddCorrelation(stubAccessor, options => options.Transaction.IncludeInResponse = true);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsNotType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
            Assert.NotNull(Mock.Get(correlationInfoAccessor));
            Assert.NotNull(serviceProvider.GetService<IOptions<CorrelationInfoOptions>>());
        }

        [Fact]
        public void AddCorrelation_CustomAccessorFactory_RegistersCustomAccessor()
        {
            // Arrange
            var services = new ServiceCollection();
            var stubAccessor = Mock.Of<ICorrelationInfoAccessor>();
            services.AddCorrelation(provider => stubAccessor);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsNotType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
            Assert.NotNull(Mock.Get(correlationInfoAccessor));
        }

        [Fact]
        public void AddCorrelation_CustomAccessorFactoryWithOptions_RegistersCustomAccessorWithOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var stubAccessor = Mock.Of<ICorrelationInfoAccessor>();
            services.AddCorrelation(provider => stubAccessor, options => options.Operation.IncludeInResponse = true);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            Assert.IsNotType<DefaultCorrelationInfoAccessor>(correlationInfoAccessor);
            Assert.NotNull(Mock.Get(correlationInfoAccessor));
            Assert.NotNull(serviceProvider.GetService<IOptions<CorrelationInfoOptions>>());
        }
    }
}
