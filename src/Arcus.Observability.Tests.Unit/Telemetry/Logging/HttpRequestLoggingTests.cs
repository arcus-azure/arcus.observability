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
    }
}
