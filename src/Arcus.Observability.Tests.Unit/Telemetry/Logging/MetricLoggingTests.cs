using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class MetricLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogCustomMetric_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Lorem.Word();
            double metricValue = _bogusGenerator.Random.Double();

            // Act
            logger.LogCustomMetric(metricName, metricValue);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Collection(metric.Context, item =>
            {
                Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                Assert.Equal(TelemetryType.Metrics, item.Value);
            });
        }

        [Fact]
        public void LogCustomMetric_ValidArgumentsWithTimestamp_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Lorem.Word();
            double metricValue = _bogusGenerator.Random.Double();
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();

            // Act
            logger.LogCustomMetric(metricName, metricValue, timestamp);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Equal(timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat), metric.Timestamp);
            Assert.Collection(metric.Context, item =>
            {
                Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                Assert.Equal(TelemetryType.Metrics, item.Value);
            });
        }

        [Fact]
        public void LogCustomMetric_ValidArgumentsWithCustomContext_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Lorem.Word();
            double metricValue = _bogusGenerator.Random.Double();
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCustomMetric(metricName, metricValue, timestamp, context);

            // Assert
            MetricLogEntry metric = logger.GetMessageAsMetric();
            Assert.Equal(metricName, metric.MetricName);
            Assert.Equal(metricValue, metric.MetricValue);
            Assert.Equal(timestamp.ToString(FormatSpecifiers.InvariantTimestampFormat), metric.Timestamp);
            Assert.Collection(metric.Context,
                item =>
                {
                    Assert.Equal(key, item.Key);
                    Assert.Equal(value, item.Value);
                },
                item =>
                {
                    Assert.Equal(ContextProperties.General.TelemetryType, item.Key);
                    Assert.Equal(TelemetryType.Metrics, item.Value);
                });
        }

        [Fact]
        public void LogCustomMetric_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Random.Word();
            double metricValue = _bogusGenerator.Random.Double();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogCustomMetric(metricName, metricValue, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
