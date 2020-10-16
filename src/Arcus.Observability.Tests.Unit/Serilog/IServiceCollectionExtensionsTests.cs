using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAssemblyAppVersion_WithoutConsumerType_Throws()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => services.AddAssemblyAppVersion(consumerType: null));
        }
    }
}
