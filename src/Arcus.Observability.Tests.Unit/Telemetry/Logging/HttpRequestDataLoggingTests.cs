using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
#if NET6_0
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http; 
#endif
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
#if NET6_0
    public class HttpRequestDataDataLoggingTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, statusCode, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal($"{method} {path}", entry.OperationName);
            Assert.Equal((int) statusCode, entry.ResponseStatusCode);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)BogusGenerator.PickRandom<HttpStatusCode>();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, statusCode, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, measurement: null));
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(999, int.MaxValue)]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithInvalidStatusCode_Fails(int min, int max)
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode) BogusGenerator.Random.Int(min, max);
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithValidArgumentsWithResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, statusCode, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal($"{method} {path}", entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal((int) statusCode, entry.ResponseStatusCode);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestData) null, statusCode, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, startTime, duration, context));
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(999, int.MaxValue)]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithInvalidStatusCode_Fails(int min, int max)
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode) BogusGenerator.Random.Int(min, max);
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = BogusGenerator.Lorem.Word();

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, statusCode, operationName, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameWithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)BogusGenerator.PickRandom<HttpStatusCode>();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string operationName = BogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, statusCode, operationName, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameWithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = BogusGenerator.Lorem.Word();

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, measurement: null));
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(999, int.MaxValue)]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameWithInvalidStatusCode_Fails(int min, int max)
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode) BogusGenerator.Random.Int(min, max);
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);

            string operationName = BogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithOperationNameWithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            string operationName = BogusGenerator.Lorem.Word();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, statusCode, operationName, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal((int) statusCode, entry.ResponseStatusCode);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithOperationNameWithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();
            string operationName = BogusGenerator.Lorem.Word();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestData) null, statusCode, operationName, startTime, duration, context));
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(999, int.MaxValue)]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithOperationNameWithInvalidStatusCode_Fails(int min, int max)
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = BogusGenerator.Lorem.Word();

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            var statusCode = (HttpStatusCode) BogusGenerator.Random.Int(min, max);
            TimeSpan duration = BogusGenerator.Date.Timespan();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeDurationStartTime_WithOperationNameWithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = BogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{BogusGenerator.Lorem.Word()}";
            string host = BogusGenerator.Lorem.Word();
            var scheme = "https";
            string operationName = BogusGenerator.Lorem.Word();
            HttpMethod method = HttpMethod.Post;

            HttpRequestData stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = BogusGenerator.Date.RecentOffset();

            string key = BogusGenerator.Lorem.Word();
            string value = BogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, startTime, duration, context));
        }

        private static HttpRequestData CreateStubRequest(HttpMethod method, string host, string path, string scheme)
        {
            var context = Mock.Of<FunctionContext>();

            var stub = new Mock<HttpRequestData>(context);
            stub.Setup(s => s.Method).Returns(method.Method);
            stub.Setup(s => s.Url).Returns(new Uri(scheme + "://" + host + path));

            return stub.Object;
        }
    }
#endif
}
