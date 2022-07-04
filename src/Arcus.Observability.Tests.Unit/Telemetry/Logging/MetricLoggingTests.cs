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
        public void LogMetric_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
            double metricValue = _bogusGenerator.Random.Double();

            // Act
            logger.LogMetric(metricName, metricValue);

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
        public void LogCustomMetric_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Name.FullName();
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
        public void LogMetric_ValidArgumentsWithTimestamp_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = _bogusGenerator.Lorem.Word();
            double metricValue = _bogusGenerator.Random.Double();
            DateTimeOffset timestamp = _bogusGenerator.Date.RecentOffset();

            // Act
            logger.LogMetric(metricName, metricValue, timestamp);

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
        public void LogMetric_ValidArgumentsWithCustomContext_Succeeds()
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
            logger.LogMetric(metricName, metricValue, timestamp, context);

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
        public void LogMetric_NoMetricNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = null;
            double metricValue = _bogusGenerator.Random.Double();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogMetric(metricName, metricValue));
        }

        [Fact]
        public void LogCustomMetric_NoMetricNameWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            string metricName = null;
            double metricValue = _bogusGenerator.Random.Double();

            // Act & Arrange
            Assert.Throws<ArgumentException>(() => logger.LogCustomMetric(metricName, metricValue));
        }
    }
}
