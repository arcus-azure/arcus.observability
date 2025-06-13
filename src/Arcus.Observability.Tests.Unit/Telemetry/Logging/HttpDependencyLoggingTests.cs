using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class HttpDependencyLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogHttpDependencyWithRequestMessage_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act
            logger.LogHttpDependency(request, statusCode, startTime, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri.Host, logMessage);
            Assert.Contains(request.RequestUri.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            var isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri.Host, logMessage);
            Assert.Contains(request.RequestUri.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            var isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
            Assert.Contains(dependencyId, logMessage);
        }

        [Fact]
        public void LogHttpDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependency_NoRequestWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            HttpRequestMessage request = null;
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogHttpDependency(request, statusCode, measurement);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri?.Host, logMessage);
            Assert.Contains(request.RequestUri?.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            bool isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri?.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            logger.LogHttpDependency(request, statusCode, measurement, dependencyId);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri?.Host, logMessage);
            Assert.Contains(request.RequestUri?.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            bool isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri?.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
            Assert.Contains(dependencyId, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurementWithDependencyIdWithIntStatusCode_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            logger.LogHttpDependency(request, statusCode, measurement, dependencyId);

            // Assert
            string logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri?.Host, logMessage);
            Assert.Contains(request.RequestUri?.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(statusCode.ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            bool isSuccessful = statusCode >= 200 && statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri?.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
            Assert.Contains(dependencyId, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequestMessage)null, statusCode, measurement));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurementWithDependencyId_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequestMessage)null, statusCode, measurement, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurementWithDependencyIdWithIntStatusCode_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequestMessage)null, statusCode, measurement, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = (HttpStatusCode)BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: null, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurementWithDependencyIdWithIntStatusCode_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.Random.Int(100, 599);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: null, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessage_WithHttpStatusCodeOutsideAllowedRange_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = BogusGenerator.PickRandom(
                BogusGenerator.Random.Int(max: 99),
                BogusGenerator.Random.Int(min: 600)
            );
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, DateTimeOffset.Now, TimeSpan.Zero, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = (int) BogusGenerator.PickRandom<HttpStatusCode>();
            string dependencyId = BogusGenerator.Random.Guid().ToString();
            using var measurement = DurationMeasurement.Start();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogHttpDependency(request, statusCode, measurement, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogHttpDependencyWithStartDuration_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = (int) BogusGenerator.PickRandom<HttpStatusCode>();
            string dependencyId = BogusGenerator.Random.Guid().ToString();
            var startTIme = BogusGenerator.Date.RecentOffset();
            var duration = BogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogHttpDependency(request, statusCode, startTIme, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }
    }
}
