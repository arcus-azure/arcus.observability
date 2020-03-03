using System;
using System.Globalization;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    [Trait("Category", "Unit")]
    public class ILoggerExtensionsTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogMetric_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
            double metricValue = _bogusGenerator.Random.Double();

            // Act
            logger.LogMetric(metricName, metricValue);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.StartsWith(MessagePrefixes.Metric, logMessage);
            Assert.Contains(metricName, logMessage);
            Assert.Contains(metricValue.ToString(CultureInfo.InvariantCulture), logMessage);
        }

        [Fact]
        public void LogMetric_NoMetricNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = null;
            double metricValue = _bogusGenerator.Random.Double();

            // Act & Arrange
            Assert.Throws<ArgumentException>(()=> logger.LogMetric(metricName, metricValue));
        }
    }
}