using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;
using static Arcus.Observability.Telemetry.Core.ContextProperties.RequestTracking;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    public class CustomRequestLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogWithoutOperationNameWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCustomRequest(customRequestSource, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.Custom, entry.SourceSystem);
            Assert.Equal(customRequestSource, entry.CustomRequestSource);
            Assert.Equal(DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutOperationNameWithDurationMeasurement_WithoutEventHubsNamespace_Fails(string customRequestSource)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, isSuccessful, measurement));
        }

        [Fact]
        public void LogWithoutOperationNameWithDurationMeasurement_WithoutDurationMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string customRequestSource = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, eventHubsName, isSuccessful, measurement: null));
        }

        [Fact]
        public void LogWithoutOperationName_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCustomRequest(customRequestSource, isSuccessful, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.Custom, entry.SourceSystem);
            Assert.Equal(customRequestSource, entry.CustomRequestSource);
            Assert.Equal(DefaultOperationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(duration, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutOperationName_WithoutEventHubsNamespace_Fails(string customRequestSource)
        {
            // Arrange
            var logger = new TestLogger();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogWithoutOperationName_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, isSuccessful, startTime, duration));
        }

         [Fact]
        public void LogWithOperationNameWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.Custom, entry.SourceSystem);
            Assert.Equal(customRequestSource, entry.CustomRequestSource);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithOperationNameWithDurationMeasurement_WithoutEventHubsNamespace_Fails(string customRequestSource)
        {
            // Arrange
            var logger = new TestLogger();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, measurement));
        }

        [Fact]
        public void LogWithOperationNameWithDurationMeasurement_WithoutDurationMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, measurement: null));
        }

        [Fact]
        public void LogWithOperationName_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.Custom, entry.SourceSystem);
            Assert.Equal(customRequestSource, entry.CustomRequestSource);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(duration, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithOperationName_WithoutEventHubsNamespace_Fails(string customRequestSource)
        {
            // Arrange
            var logger = new TestLogger();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, startTime, duration));
        }

        [Fact]
        public void LogWithOperationName_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string customRequestSource = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogCustomRequest(customRequestSource, operationName, isSuccessful, startTime, duration));
        }
    }
}
