using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;
using static Arcus.Observability.Telemetry.Core.ContextProperties.RequestTracking.EventHubs;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    public class EventHubsRequestLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogWithoutConsumerGroupWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string name = BogusGenerator.Lorem.Word();
            string @namespace = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsRequest(@namespace, name, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.AzureEventHubs, entry.SourceSystem);
            Assert.Equal(name, Assert.Contains(Name, entry.Context));
            Assert.Equal(@namespace, Assert.Contains(Namespace, entry.Context));
            Assert.Equal(DefaultConsumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.NotNull(entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutConsumerGroupWithDurationMeasurement_WithoutEventHubsNamespace_Fails(string @namespace)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubsName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutConsumerGroupWithDurationMeasurement_WithoutEventHubsName_Fails(string eventHubsName)
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, measurement));
        }

        [Fact]
        public void LogWithoutConsumerGroupWithDurationMeasurement_WithoutDurationMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string @namespace = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, measurement: null));
        }

        [Fact]
        public void LogWithoutConsumerGroup_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string name = BogusGenerator.Lorem.Word();
            string @namespace = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsRequest(@namespace, name, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.AzureEventHubs, entry.SourceSystem);
            Assert.Equal(name, Assert.Contains(Name, entry.Context));
            Assert.Equal(@namespace, Assert.Contains(Namespace, entry.Context));
            Assert.Equal(DefaultConsumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.NotNull(entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(duration, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutConsumerGroup_WithoutEventHubsNamespace_Fails(string @namespace)
        {
            // Arrange
            var logger = new TestLogger();
            string eventHubsName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, duration, startTime));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithoutConsumerGroup_WithoutEventHubsName_Fails(string eventHubsName)
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, duration, startTime));
        }

        [Fact]
        public void LogWithoutConsumerGroup_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, eventHubsName, isSuccessful, duration, startTime));
        }

         [Fact]
        public void LogWithConsumerGroupWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.AzureEventHubs, entry.SourceSystem);
            Assert.Equal(@namespace, Assert.Contains(Namespace, entry.Context));
            Assert.Equal(consumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.Equal(eventHubsName, Assert.Contains(Name, entry.Context));
            Assert.Equal(consumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithConsumerGroupWithDurationMeasurement_WithoutEventHubsNamespace_Fails(string @namespace)
        {
            // Arrange
            var logger = new TestLogger();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithConsumerGroupWithDurationMeasurement_WithoutEventHubsName_Fails(string eventHubsName)
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement));
        }

        [Fact]
        public void LogWithConsumerGroupWithDurationMeasurement_WithoutDurationMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, measurement: null));
        }

        [Fact]
        public void LogWithConsumerGroup_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, duration, startTime, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(RequestSourceSystem.AzureEventHubs, entry.SourceSystem);
            Assert.Equal(@namespace, Assert.Contains(Namespace, entry.Context));
            Assert.Equal(consumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.Equal(eventHubsName, Assert.Contains(Name, entry.Context));
            Assert.Equal(consumerGroup, Assert.Contains(ConsumerGroup, entry.Context));
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(isSuccessful, entry.ResponseStatusCode is 200);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(duration, entry.RequestDuration);
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithConsumerGroup_WithoutEventHubsNamespace_Fails(string @namespace)
        {
            // Arrange
            var logger = new TestLogger();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, duration, startTime));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogWithConsumerGroup_WithoutEventHubsName_Fails(string eventHubsName)
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, duration, startTime));
        }

        [Fact]
        public void LogWithConsumerGroup_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            string @namespace = BogusGenerator.Lorem.Word();
            string consumerGroup = BogusGenerator.Lorem.Word();
            string eventHubsName = BogusGenerator.Lorem.Word();
            string operationName = BogusGenerator.Lorem.Word();
            bool isSuccessful = BogusGenerator.PickRandom(false, true);
            TimeSpan duration = BogusGenerator.Date.Timespan().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogEventHubsRequest(@namespace, consumerGroup, eventHubsName, operationName, isSuccessful, duration, startTime));
        }
    }
}
