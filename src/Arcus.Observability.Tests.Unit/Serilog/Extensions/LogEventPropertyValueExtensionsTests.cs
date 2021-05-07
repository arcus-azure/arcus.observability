using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog.Extensions
{
    public class LogEventPropertyValueExtensionsTests
    {
        private readonly Faker _bogusGenerator = new Faker();
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void GetAsStructureValue_WithoutPropertyKey_Fails(string propertyKey)
        {
            // Arrange
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>());
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => properties.GetAsStructureValue(propertyKey));
        }

        [Fact]
        public void GetAsStructureValue_WithFoundPropertyWithoutAssociatedStructureValue_Succeeds()
        {
            string propertyKey = _bogusGenerator.Random.Word();
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
            string propertyKey = _bogusGenerator.Random.Word();
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

        [Theory]
        [ClassData(typeof(Blanks))]
        public void GetAsEnum_WithoutPropertyKey_Fails(string propertyKey)
        {
            // Arrange
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>());
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => properties.GetAsEnum<TelemetryType>(propertyKey));
        }

        [Fact]
        public void GetAsEnum_WithoutPropertyValue_ReturnsNull()
        {
            // Arrange
            string propertyKey = _bogusGenerator.Random.Word();
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    [propertyKey] = null
                });
            
            // Act
            TelemetryType? result = properties.GetAsEnum<TelemetryType>(propertyKey);
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAsEnum_WithoutParsablePropertyValue_Fails()
        {
            // Arrange
            string propertyKey = _bogusGenerator.Random.Word();
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    [propertyKey] = new ScalarValue(_bogusGenerator.Random.Word())
                });
            
            // Act / Assert
            Assert.ThrowsAny<FormatException>(() => properties.GetAsEnum<TelemetryType>(propertyKey));
        }
        
        [Fact]
        public void GetAsEnum_WithParsablePropertyValue_Succeeds()
        {
            // Arrange
            string propertyKey = _bogusGenerator.Random.Word();
            TelemetryType expected = _bogusGenerator.Random.Enum<TelemetryType>();
            var properties = new ReadOnlyDictionary<string, LogEventPropertyValue>(
                new Dictionary<string, LogEventPropertyValue>
                {
                    [propertyKey] = new ScalarValue(expected)
                });
            
            // Act
            TelemetryType? actual = properties.GetAsEnum<TelemetryType>(propertyKey);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
