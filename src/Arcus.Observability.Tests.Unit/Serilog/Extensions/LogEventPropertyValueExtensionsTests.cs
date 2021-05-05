using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Extensions
{
    public class LogEventPropertyValueExtensionsTests
    {
        [Fact]
        public void GetAsStructureValue_WithoutPropertyKey_Fails()
        {
            // Arrange
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>());
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => properties.GetAsStructureValue(propertyKey: null));
        }

        [Fact]
        public void GetAsStructureValue_WithFoundPropertyWithoutAssociatedStructureValue_Succeeds()
        {
            string propertyKey = $"prop-{Guid.NewGuid()}";
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    [propertyKey] = new ScalarValue("some value")
                });
            
            // Act
            StructureValue result = properties.GetAsStructureValue(propertyKey);
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Properties);
        }

        [Fact]
        public void GetAsStructureValue_WithFoundPropertyKeyAssociatedWithStructureValue_Succeeds()
        {
            // Arrange
            string propertyKey = $"prop-{Guid.NewGuid()}";
            var expected = new StructureValue(new LogEventProperty[0]);
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    [propertyKey] = expected
                });
            
            // Act
            StructureValue actual = properties.GetAsStructureValue(propertyKey);
            
            // Assert
            Assert.Same(expected, actual);
        }
    }
}
