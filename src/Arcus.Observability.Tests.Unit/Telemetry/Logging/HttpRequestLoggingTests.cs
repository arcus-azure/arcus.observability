using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog.Data;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class HttpRequestLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void LogRequestWithDurationMeasurement_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            
            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, stubResponse, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal($"{method} {path}", entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, stubResponse, measurement));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, response: null, measurement));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, stubResponse, measurement: null));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithValidArgumentsWithResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, stubResponse, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal($"{method} {path}", entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response: stubResponse, startTime: startTime, duration: duration, context: context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, response: null, startTime: startTime, duration: duration, context: context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, stubResponse, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithOperationNameValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, stubResponse, operationName, measurement, context);

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
        public void LogRequestWithDurationMeasurement_WithOperationNameWithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string operationName = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, stubResponse, operationName, measurement));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithOperationNameWithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, response: null, operationName, measurement));
        }

        [Fact]
        public void LogRequestWithDurationMeasurement_WithOperationNameWithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, stubResponse, operationName, measurement: null));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithOperationNameWithValidArgumentsWithResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, stubResponse, operationName, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithOperationNameWithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string operationName = _bogusGenerator.Lorem.Word();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response: stubResponse, operationName: operationName, startTime: startTime, duration: duration, context: context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithOperationNameWithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, response: null, operationName: operationName, startTime: startTime, duration: duration, context: context));
        }

        [Fact]
        public void LogRequestWithDurationStartTime_WithOperationNameWithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            string operationName = _bogusGenerator.Lorem.Word();
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            HttpResponse stubResponse = CreateStubResponse(statusCode);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, stubResponse, operationName, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(stubRequest, statusCode, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.StartsWith(scheme, entry.RequestHost);
            Assert.EndsWith(host, entry.RequestHost);
            Assert.Equal(path, entry.RequestUri);
            Assert.Equal($"{method} {path}", entry.OperationName);
            Assert.Equal(statusCode, entry.ResponseStatusCode);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();

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
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

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
            var statusCode = _bogusGenerator.Random.Int(min, max);
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithValidArgumentsWithResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
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
            Assert.Equal(statusCode, entry.ResponseStatusCode);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, responseStatusCode: statusCode, startTime: startTime, duration: duration, context: context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
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
            var statusCode = _bogusGenerator.Random.Int(min, max);
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
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
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();
            string operationName = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, statusCode, operationName, measurement));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationMeasurement_WithOperationNameWithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

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
            int statusCode = _bogusGenerator.Random.Int(min, max);
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);

            string operationName = _bogusGenerator.Lorem.Word();
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
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
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
            Assert.Equal(statusCode, entry.ResponseStatusCode);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithOperationNameWithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            string operationName = _bogusGenerator.Lorem.Word();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, responseStatusCode: statusCode, operationName: operationName, startTime: startTime, duration: duration, context: context));
        }

        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(999, int.MaxValue)]
        public void LogRequestWithStatusCodeWithDurationStartTime_WithOperationNameWithInvalidStatusCode_Fails(int min, int max)
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            HttpMethod method = HttpMethod.Post;
            string operationName = _bogusGenerator.Lorem.Word();

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            int statusCode = _bogusGenerator.Random.Int(min, max);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, startTime, duration, context));
        }

        [Fact]
        public void LogRequestWithStatusCodeDurationStartTime_WithOperationNameWithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            var scheme = "https";
            string operationName = _bogusGenerator.Lorem.Word();
            HttpMethod method = HttpMethod.Post;

            HttpRequest stubRequest = CreateStubRequest(method, host, path, scheme);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(stubRequest, statusCode, operationName, startTime, duration, context));
        }

        [Fact]
        public void LogRequestMessageWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");
            var response = new HttpResponseMessage(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, response, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int) statusCode, entry.ResponseStatusCode);
            Assert.Equal(request.Method + " " + request.RequestUri.AbsolutePath, entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response, measurement));
        }

        [Fact]
        public void LogRequestMessageWithDurationMeasurement_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response: null, measurement));
        }

        [Fact]
        public void LogRequestMessageWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response, measurement: null));
        }

        [Fact]
        public void LogRequestMessageWithStartTime_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");
            var response = new HttpResponseMessage(statusCode);

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, response, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(request.Method + " " + request.RequestUri.AbsolutePath, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response: response, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithStartTime_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response: null, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response, startTime, duration));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");
            var response = new HttpResponseMessage(statusCode);

            string operationName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, response, operationName, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            string operationName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response, operationName, measurement));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithDurationMeasurement_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            string operationName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response: null, operationName, measurement));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);
            string operationName = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response, operationName, measurement: null));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithStartTime_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");
            var response = new HttpResponseMessage(statusCode);

            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, response, operationName, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);

            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: null, response: response, operationName: operationName, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithStartTime_WithoutResponse_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response: null, operationName: operationName, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithOperationNameWithStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var response = new HttpResponseMessage(statusCode);
            string operationName = _bogusGenerator.Lorem.Word();

            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, response, operationName, startTime, duration));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, statusCode, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(request.Method + " " + request.RequestUri.AbsolutePath, entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestMessage) null, statusCode, measurement));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, measurement: null));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithStartTime_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, statusCode, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(request.Method + " " + request.RequestUri.AbsolutePath, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestMessage) null, responseStatusCode: statusCode, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, startTime, duration));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithDurationMeasurement_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            string operationName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, statusCode, operationName, measurement, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int)statusCode, entry.ResponseStatusCode);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(measurement.Elapsed, entry.RequestDuration);
            Assert.Equal(measurement.StartTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithDurationMeasurement_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            string operationName = _bogusGenerator.Lorem.Word();
            var measurement = DurationMeasurement.Start();
            measurement.Dispose();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestMessage) null, statusCode, operationName, measurement));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithDurationMeasurement_WithoutMeasurement_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            string operationName = _bogusGenerator.Lorem.Word();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, operationName, measurement: null));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithStartTime_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            string key = _bogusGenerator.Lorem.Word();
            string value = _bogusGenerator.Lorem.Word();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogRequest(request, statusCode, operationName, startTime, duration, context);

            // Assert
            RequestLogEntry entry = logger.GetMessageAsRequest();
            Assert.Equal(scheme + "://" + host, entry.RequestHost);
            Assert.Equal("/" + path, entry.RequestUri);
            Assert.Equal((int) statusCode, entry.ResponseStatusCode);
            Assert.Equal(operationName, entry.OperationName);
            Assert.Equal(duration, entry.RequestDuration);
            Assert.Equal(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), entry.RequestTime);
            Assert.Equal(value, Assert.Contains(key, entry.Context));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithStartTime_WithoutRequest_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();

            string operationName = _bogusGenerator.Lorem.Word();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request: (HttpRequestMessage) null, responseStatusCode: statusCode, operationName: operationName, startTime: startTime, duration: duration));
        }

        [Fact]
        public void LogRequestMessageWithStatusCodeWithOperationNameWithStartTime_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            string operationName = _bogusGenerator.Lorem.Word();

            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, operationName, startTime, duration));
        }

        [Fact]
        public void LogRequestMessage_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            var scheme = "https";
            var host = _bogusGenerator.Lorem.Word();
            var path = _bogusGenerator.Lorem.Word();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{host}/{path}");

            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            string operationName = _bogusGenerator.Lorem.Word();

            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogRequest(request, statusCode, operationName, startTime, duration, context);

            // Assert
            Assert.Empty(context);
        }

        [Fact]
        public void LogRequestWithHttpRequest_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            HttpRequest request = CreateStubRequest(HttpMethod.Get, "host", "/path", "https");
            var statusCode = (int) _bogusGenerator.PickRandom<HttpStatusCode>();
            string operationName = _bogusGenerator.Lorem.Word();
            var startTime = _bogusGenerator.Date.RecentOffset();
            var duration = _bogusGenerator.Date.Timespan();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogRequest(request, statusCode, operationName, startTime, duration, context);

            // Assert
            Assert.Empty(context);
        }

        private static HttpResponse CreateStubResponse(int statusCode)
        {
            var stubResponse = new Mock<HttpResponse>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            
            return stubResponse.Object;
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

        [Fact]
        public void LogRequestObsolete_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(mockRequest.Object, mockResponse.Object, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(statusCode.ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequest_ValidArgumentsIncludingResponseStatusCode_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(mockRequest.Object, statusCode, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(statusCode.ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequest_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            var host = _bogusGenerator.Lorem.Word();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(mockRequest.Object, mockResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithSchemeWithWhitespaceWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("hostname"));
            saboteurRequest.Setup(r => r.Scheme).Returns("scheme with spaces");
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithoutHostWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString());
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithHostWithWhitespaceWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("host with spaces"));
            var stubResponse = new Mock<HttpResponse>();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            stubResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, stubResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_NoRequestWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpRequest request = null;
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, mockResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_OutsideResponseStatusCodeRange_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1),
                _bogusGenerator.Random.Int(min: 1000));
            HttpRequest request = null;
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(response => response.StatusCode).Returns(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, mockResponse.Object, duration));
        }

        [Fact]
        public void LogRequest_RequestWithSchemeWithWhitespaceWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("hostname"));
            saboteurRequest.Setup(r => r.Scheme).Returns("scheme with spaces");
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_RequestWithoutHostWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString());
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_RequestWithHostWithWhitespaceWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var saboteurRequest = new Mock<HttpRequest>();
            saboteurRequest.Setup(r => r.Host).Returns(new HostString("host with spaces"));
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(saboteurRequest.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_NoRequestWasSpecifiedWhenPassingResponseStatusCode_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            HttpRequest request = null;
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, duration));
        }

        [Fact]
        public void LogRequest_OutsideResponseStatusCodeRangeWhenPassingResponseStatusCode_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var request = new Mock<HttpRequest>();
            var statusCode = _bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1),
                _bogusGenerator.Random.Int(min: 1000));
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => logger.LogRequest(request.Object, statusCode, duration));
        }

        [Fact]
        public void LogRequest_NoResponseWasSpecified_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger();
            var path = $"/{_bogusGenerator.Name.FirstName()}";
            var host = _bogusGenerator.Name.FirstName();
            var method = HttpMethod.Head;
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(request => request.Method).Returns(method.ToString());
            mockRequest.Setup(request => request.Host).Returns(new HostString(host));
            mockRequest.Setup(request => request.Path).Returns(path);
            HttpResponse response = null;
            var duration = _bogusGenerator.Date.Timespan();

            // Act & Arrange
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(mockRequest.Object, response, duration));
        }

        [Fact]
        public void LogRequestMessage_ValidArgumentsIncludingResponse_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var response = new HttpResponseMessage(statusCode);
            var duration = _bogusGenerator.Date.Timespan();

            // Act
            logger.LogRequest(request, response, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequestMessage_ValidArgumentsIncludingResponseStatusCode_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var duration = _bogusGenerator.Date.Timespan();

            // Act;
            logger.LogRequest(request, statusCode, duration);

            // Assert
            var logMessage = logger.WrittenMessage;
            Assert.Contains(TelemetryType.Request.ToString(), logMessage);
            Assert.Contains(path, logMessage);
            Assert.Contains(host, logMessage);
            Assert.Contains(((int)statusCode).ToString(), logMessage);
            Assert.Contains(method.ToString(), logMessage);
        }

        [Fact]
        public void LogRequestMessage_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, duration));
        }

        [Fact]
        public void LogRequestMessage_OutsideResponseStatusCodeRange_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            var statusCode = (HttpStatusCode)_bogusGenerator.PickRandom(
                _bogusGenerator.Random.Int(max: -1),
                _bogusGenerator.Random.Int(min: 1000));
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            var host = _bogusGenerator.Name.FirstName().ToLower();
            var method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            TimeSpan duration = _bogusGenerator.Date.Timespan();

            // Act & Assert
            Assert.ThrowsAny<ArgumentException>(() => logger.LogRequest(request, statusCode, duration));
        }
    }
}
