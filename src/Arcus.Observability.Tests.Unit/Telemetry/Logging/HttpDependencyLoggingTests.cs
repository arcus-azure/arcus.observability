using System;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
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
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
        public void LogHttpDependencyWithRequest_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var requestUri = new Uri("https://arcus.test/unit");
            HttpRequest request = CreateStubRequest(HttpMethod.Get, requestUri.Host, requestUri.AbsolutePath, requestUri.Scheme);

            var statusCode = HttpStatusCode.BadRequest;
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act
            logger.LogHttpDependency(request, statusCode, startTime, duration);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Contains("Http", dependency.DependencyType);
            Assert.Contains(request.Method, logger.WrittenMessage);
            Assert.Contains(request.Path, dependency.DependencyName);
            Assert.Equal(request.Host.Host, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.False(dependency.IsSuccessful);
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var requestUri = new Uri("https://arcus.test/unit");
            HttpRequest request = CreateStubRequest(HttpMethod.Get, requestUri.Host, requestUri.AbsolutePath, requestUri.Scheme);

            var statusCode = HttpStatusCode.OK;
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            logger.LogHttpDependency(request, statusCode, startTime, duration, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Contains("Http", dependency.DependencyType);
            Assert.Contains(request.Method, logger.WrittenMessage);
            Assert.Contains(request.Path, dependency.DependencyName);
            Assert.Equal(request.Host.Host, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.True(dependency.IsSuccessful);
            Assert.Equal(dependencyId, dependency.DependencyId);
        }

        [Fact]
        public void LogHttpDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.Url());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = BogusGenerator.Date.PastOffset();
            var duration = BogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => logger.LogHttpDependency(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogHttpDependencyWithDependencyMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
        public void LogHttpDependencyWithRequestWithDurationMeasurement_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var requestUri = new Uri("https://arcus.test/unit");
            HttpRequest request = CreateStubRequest(HttpMethod.Get, requestUri.Host, requestUri.AbsolutePath, requestUri.Scheme);

            var statusCode = HttpStatusCode.BadRequest;
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;

            // Act
            logger.LogHttpDependency(request, statusCode, measurement);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Contains("Http", dependency.DependencyType);
            Assert.Contains(request.Method, logger.WrittenMessage);
            Assert.EndsWith(request.Path, dependency.DependencyName);
            Assert.Equal(request.Host.Host, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.False(dependency.IsSuccessful);
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDurationMeasurementWithDependencyId_ValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var requestUri = new Uri("https://arcus.test/unit");
            HttpRequest request = CreateStubRequest(HttpMethod.Get, requestUri.Host, requestUri.AbsolutePath, requestUri.Scheme);

            var statusCode = HttpStatusCode.BadRequest;
            var measurement = DurationMeasurement.Start();
            DateTimeOffset startTime = measurement.StartTime;
            measurement.Dispose();
            TimeSpan duration = measurement.Elapsed;
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act
            logger.LogHttpDependency(request, statusCode, measurement, dependencyId);

            // Assert
            DependencyLogEntry dependency = logger.GetMessageAsDependency();
            Assert.Contains("Http", dependency.DependencyType);
            Assert.Equal(dependencyId, dependency.DependencyId);
            Assert.Contains(request.Method, logger.WrittenMessage);
            Assert.Contains(request.Path, dependency.DependencyName);
            Assert.Equal(request.Host.Host, dependency.TargetName);
            Assert.Equal(duration, dependency.Duration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), dependency.StartTime);
            Assert.False(dependency.IsSuccessful);
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequestMessage) null, statusCode, measurement));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurementWithDependencyId_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequestMessage) null, statusCode, measurement, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequest) null, statusCode, measurement));
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDurationMeasurementWithDependencyId_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request: (HttpRequest) null, statusCode, measurement, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestMessageWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new HttpRequestMessage(HttpMethod.Get, BogusGenerator.Internet.UrlWithPath());
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
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
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: null, dependencyId));
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            HttpRequest request = CreateStubRequest(HttpMethod.Get, "host", "/path", "http");
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: (DurationMeasurement)null));
        }

        [Fact]
        public void LogHttpDependencyWithRequestWithDurationMeasurementWithDependencyId_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            HttpRequest request = CreateStubRequest(HttpMethod.Get, "host", "/path", "http");
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string dependencyId = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogHttpDependency(request, statusCode, measurement: null, dependencyId));
        }

        private static HttpRequest CreateStubRequest(HttpMethod method, string host, string path, string scheme)
        {
            var stubRequest = new Mock<HttpRequest>();
            stubRequest.Setup(request => request.Method).Returns(method.ToString());
            stubRequest.Setup(request => request.Host).Returns(new HostString(host));
            stubRequest.Setup(request => request.Path).Returns(path);
            stubRequest.Setup(req => req.Scheme).Returns(scheme);

            return stubRequest.Object;
        }
    }
}
