using System;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class HttpDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogHttpDependency_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.Url());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

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
        public void LogHttpDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.Url());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
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
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DependencyMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogHttpDependency(request, statusCode, measurement);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
            Assert.Contains(request.RequestUri.Host, logMessage);
            Assert.Contains(request.RequestUri.PathAndQuery, logMessage);
            Assert.Contains(request.Method.ToString(), logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
            Assert.Contains(duration.ToString(), logMessage);
            var isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
            Assert.Contains($"Successful: {isSuccessful}", logMessage);
            Uri requestUri = request.RequestUri;
            HttpMethod requestMethod = request.Method;
            string dependencyName = $"{requestMethod} {requestUri.AbsolutePath}";
            Assert.Contains("Http " + dependencyName, logMessage);
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
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
        public void LogHttpDependencyWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: null, statusCode, measurement));
        }

        [Fact]
        public void LogHttpDependencyWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: (DurationMeasurement)null));
        }
    }
}
