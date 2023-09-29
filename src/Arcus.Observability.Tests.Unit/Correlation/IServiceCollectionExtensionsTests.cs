using System;
using Arcus.Observability.Correlation;
using Microsoft.Extensions.DependencyInjection;
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
        }

        [Fact]
        public void AddCorrelation_CustomCorrelationFactory_RegisterCustomCorrelation()
        {
            // Arrange
            var services = new ServiceCollection();
            var testCorrelationInfoAccessor = new TestCorrelationInfoAccessor();
            services.AddCorrelation<TestCorrelationInfoAccessor, TestCorrelationInfo>(provider => testCorrelationInfoAccessor);

            // Act
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            var correlationInfoAccessor = serviceProvider.GetService<ICorrelationInfoAccessor>();
            Assert.NotNull(correlationInfoAccessor);
            var correlationInfoAccessorT = serviceProvider.GetService<ICorrelationInfoAccessor<TestCorrelationInfo>>();
            Assert.NotNull(correlationInfoAccessorT);
            Assert.Same(testCorrelationInfoAccessor, correlationInfoAccessorT);
        }
    }
}
