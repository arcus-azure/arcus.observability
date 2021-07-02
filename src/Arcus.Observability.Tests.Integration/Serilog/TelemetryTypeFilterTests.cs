using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Filters;
using Arcus.Observability.Tests.Core;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog
{
    [Trait("Category", "Integration")]
    public class TelemetryTypeFilterTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        public static IEnumerable<object[]> TelemetryTypesWithoutEvent => GetTelemetryTypesWithout(TelemetryType.Events);
        public static IEnumerable<object[]> TelemetryTypesWithoutMetric => GetTelemetryTypesWithout(TelemetryType.Metrics);
        public static IEnumerable<object[]> TelemetryTypesWithoutDependency => GetTelemetryTypesWithout(TelemetryType.Dependency);
        public static IEnumerable<object[]> TelemetryTypesWithoutRequest => GetTelemetryTypesWithout(TelemetryType.Request);
        
        [Fact]
        public void LogEvent_WithTelemetryTypeFilter_IgnoresLogEvent()
        {
            // Arrange
            string eventName = _bogusGenerator.Random.Word();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogEvent(eventName, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutEvent))]
        public void LogEvent_WithTelemetryTypeFilterOnOtherType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string eventName = _bogusGenerator.Random.Word();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType, isTrackingEnabled: false))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogEvent(eventName, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
                Assert.Contains(TelemetryType.Events.ToString(), writtenMessage);
            }
        }
        
        [Fact]
        public void LogSecurityEvent_WithTelemetryTypeFilter_IgnoresLogEvent()
        {
            // Arrange
            string eventName = _bogusGenerator.Random.Word();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Events))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogSecurityEvent(eventName, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutEvent))]
        public void LogSecurityEvent_WithTelemetryTypeFilterOnOtherType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string eventName = _bogusGenerator.Random.Word();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType, isTrackingEnabled: false))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogSecurityEvent(eventName, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
                Assert.Contains(TelemetryType.Events.ToString(), writtenMessage);
            }
        }

        [Fact]
        public void LogMetric_WithTelemetryTypeFilter_IgnoresMetric()
        {
            // Arrange
            string metricName = _bogusGenerator.Random.Word();
            double metricValue = _bogusGenerator.Random.Double();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Metrics))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogMetric(metricName, metricValue, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutMetric))]
        public void LogMetric_WithTelemetryTypeFilterOnDifferentTyp_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string metricName = _bogusGenerator.Random.Word();
            double metricValue = _bogusGenerator.Random.Double();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogMetric(metricName, metricValue, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(metricName, writtenMessage);
                Assert.Contains(metricValue.ToString(CultureInfo.InvariantCulture), writtenMessage);
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
                Assert.Contains(TelemetryType.Metrics.ToString(), writtenMessage);
            }
        }

        [Fact]
        public void LogRequest_WithTelemetryTypeFilter_FilersInRequest()
        {
            // Arrange
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            HttpMethod method = HttpMethod.Head;
            var stubbedRequest = new Mock<HttpRequest>();
            stubbedRequest.Setup(request => request.Method).Returns(method.ToString());
            stubbedRequest.Setup(request => request.Host).Returns(new HostString(host));
            stubbedRequest.Setup(request => request.Path).Returns(path);
            var stubbedResponse = new Mock<HttpResponse>();
            stubbedResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Request))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogRequest(stubbedRequest.Object, stubbedResponse.Object, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
         [Theory]
         [MemberData(nameof(TelemetryTypesWithoutRequest))]
        public void LogRequest_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            var statusCode = (int)_bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Lorem.Word()}";
            string host = _bogusGenerator.Lorem.Word();
            HttpMethod method = HttpMethod.Head;
            var stubbedRequest = new Mock<HttpRequest>();
            stubbedRequest.Setup(request => request.Method).Returns(method.ToString());
            stubbedRequest.Setup(request => request.Host).Returns(new HostString(host));
            stubbedRequest.Setup(request => request.Path).Returns(path);
            var stubbedResponse = new Mock<HttpResponse>();
            stubbedResponse.Setup(response => response.StatusCode).Returns(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogRequest(stubbedRequest.Object, stubbedResponse.Object, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(path, writtenMessage);
                Assert.Contains(host, writtenMessage);
                Assert.Contains(statusCode.ToString(), writtenMessage);
                Assert.Contains(method.ToString(), writtenMessage);
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
                Assert.Contains(TelemetryType.Request.ToString(), writtenMessage);
            }
        }
        
        [Fact]
        public void LogRequestMessage_WithTelemetryTypeFilter_IgnoresRequestMessage()
        {
            // Arrange
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            string host = _bogusGenerator.Name.FirstName().ToLower();
            HttpMethod method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var response = new HttpResponseMessage(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};

             var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Request))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogRequest(request, response, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutRequest))]
        public void LogRequestMessage_WithTelemetryTypeFilter_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            var path = $"/{_bogusGenerator.Name.FirstName().ToLower()}";
            string host = _bogusGenerator.Name.FirstName().ToLower();
            HttpMethod method = HttpMethod.Head;
            var request = new HttpRequestMessage(method, new Uri("https://" + host + path));
            var response = new HttpResponseMessage(statusCode);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};

             var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogRequest(request, response, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Request.ToString(), writtenMessage);
                Assert.Contains(path, writtenMessage);
                Assert.Contains(host, writtenMessage);
                Assert.Contains(((int)statusCode).ToString(), writtenMessage);
                Assert.Contains(method.ToString(), writtenMessage);
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
                Assert.Contains(TelemetryType.Request.ToString(), writtenMessage);
            }
        }
        
        [Fact]
        public void LogHttpDependency_WithTelemetryTypeFilter_IgnoresHttpDependency()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> { [propertyName] = propertyValue };

            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogHttpDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, _bogusGenerator.Internet.UrlWithPath());
            var statusCode = _bogusGenerator.PickRandom<HttpStatusCode>();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> { [propertyName] = propertyValue };

            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogHttpDependency(request, statusCode, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string writtenMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), writtenMessage);
                Assert.Contains(request.RequestUri.Host, writtenMessage);
                Assert.Contains(request.RequestUri.PathAndQuery, writtenMessage);
                Assert.Contains(request.Method.ToString(), writtenMessage);
                Assert.Contains(((int)statusCode).ToString(), writtenMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), writtenMessage);
                bool isSuccessful = (int)statusCode >= 200 && (int)statusCode < 300;
                Assert.Contains($"Successful: {isSuccessful}", writtenMessage);
                Assert.Contains(propertyName, writtenMessage);
                Assert.Contains(propertyValue, writtenMessage);
            }
        }
        
        [Fact]
        public void LogSqlDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};

            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogSqlDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string serverName = _bogusGenerator.Name.FullName();
            string databaseName = _bogusGenerator.Name.FullName();
            string tableName = _bogusGenerator.Name.FullName();
            string operationName = _bogusGenerator.Name.FullName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};

            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogSqlDependency(serverName, databaseName, tableName, operationName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(serverName, logMessage);
                Assert.Contains(databaseName, logMessage);
                Assert.Contains(tableName, logMessage);
                Assert.Contains(operationName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogServiceBusTopicDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogServiceBusTopicDependency(topicName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogServiceBusTopicDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string topicName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogServiceBusTopicDependency(topicName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(ServiceBusEntityType.Topic.ToString(), logMessage);
                Assert.Contains(topicName, logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogServiceBusQueueDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
             var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogServiceBusQueueDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string queueName = _bogusGenerator.Commerce.Product();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogServiceBusQueueDependency(queueName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(ServiceBusEntityType.Queue.ToString(), logMessage);
                Assert.Contains(queueName, logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogAzureSearchDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogAzureSearchDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string searchServiceName = _bogusGenerator.Commerce.Product();
            string operationName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogAzureSearchDependency(searchServiceName, operationName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(searchServiceName, logMessage);
                Assert.Contains(operationName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogAzureKeyVaultDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            var vaultUri = "https://myvault.vault.azure.net";
            string secretName = "MySecret";
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogAzureKeyVaultDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            var vaultUri = "https://myvault.vault.azure.net";
            string secretName = "MySecret";
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.RecentOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(vaultUri, logMessage);
                Assert.Contains(secretName, logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogDependencyTarget_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogDependencyTarget_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string dependencyType = _bogusGenerator.Name.FullName();
            var dependencyData = _bogusGenerator.Finance.Amount().ToString("F");
            string targetName = _bogusGenerator.Lorem.Word();
            bool isSuccessful = _bogusGenerator.PickRandom(true, false);
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogDependency(dependencyType, dependencyData, targetName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(dependencyType, logMessage);
                Assert.Contains(dependencyData, logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogCosmosSqlDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogCosmosSqlDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string container = _bogusGenerator.Commerce.ProductName();
            string database = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogCosmosSqlDependency(accountName, database, container, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(container, logMessage);
                Assert.Contains(database, logMessage);
                Assert.Contains(accountName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogIotHubDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, startTime: startTime, duration: duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogIotHubDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string iotHubName = _bogusGenerator.Commerce.ProductName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogIotHubDependency(iotHubName: iotHubName, isSuccessful: isSuccessful, startTime: startTime, duration: duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(iotHubName, logMessage);
                Assert.Contains(iotHubName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogEventHubsDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogEventHubsDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string eventHubName = _bogusGenerator.Commerce.ProductName();
            string namespaceName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();
            
            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();
                
                // Act
                logger.LogEventHubsDependency(namespaceName, eventHubName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(namespaceName, logMessage);
                Assert.Contains(eventHubName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogTableStorageDependency_WithTelemetryTypeFilter_FiltersOutDependency()
        {
            // Arrange
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();

                // Act
                logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogTableStorageDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string tableName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();

                // Act
                logger.LogTableStorageDependency(accountName, tableName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(tableName, logMessage);
                Assert.Contains(accountName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }
        
        [Fact]
        public void LogBlobStorageDependency_WithTelemetryTypeFilter_IgnoresDependency()
        {
            // Arrange
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
             string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(TelemetryType.Dependency))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();

                // Act
                logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, startTime, duration, properties);

                // Assert
                Assert.Empty(spySink.CurrentLogEmits);
            }
        }
        
        [Theory]
        [MemberData(nameof(TelemetryTypesWithoutDependency))]
        public void LogBlobStorageDependency_WithTelemetryTypeFilterOnDifferentTelemetryType_DoesNotFilterOutEntry(TelemetryType telemetryType)
        {
            // Arrange
            string containerName = _bogusGenerator.Commerce.ProductName();
            string accountName = _bogusGenerator.Finance.AccountName();
            bool isSuccessful = _bogusGenerator.Random.Bool();
            DateTimeOffset startTime = _bogusGenerator.Date.PastOffset();
            TimeSpan duration = _bogusGenerator.Date.Timespan();
             string propertyName = _bogusGenerator.Random.Word();
            string propertyValue = _bogusGenerator.Random.Word();
            var properties = new Dictionary<string, object> {[propertyName] = propertyValue};
            
            var spySink = new InMemoryLogSink();
            Logger serilogLogger = new LoggerConfiguration()
                .Filter.With(TelemetryTypeFilter.On(telemetryType))
                .WriteTo.Sink(spySink)
                .CreateLogger();

            using (var factory = new SerilogLoggerFactory(serilogLogger))
            {
                ILogger logger = factory.CreateLogger<TelemetryTypeFilterTests>();

                // Act
                logger.LogBlobStorageDependency(accountName, containerName, isSuccessful, startTime, duration, properties);

                // Assert
                LogEvent logEvent = Assert.Single(spySink.CurrentLogEmits);
                Assert.NotNull(logEvent);
                string logMessage = logEvent.RenderMessage();
                Assert.Contains(TelemetryType.Dependency.ToString(), logMessage);
                Assert.Contains(containerName, logMessage);
                Assert.Contains(accountName, logMessage);
                Assert.Contains(isSuccessful.ToString(), logMessage);
                Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), logMessage);
                Assert.Contains(duration.ToString(), logMessage);
                Assert.Contains(propertyName, logMessage);
                Assert.Contains(propertyValue, logMessage);
            }
        }

        private static IEnumerable<object[]> GetTelemetryTypesWithout(TelemetryType telemetryType)
        {
            return Enum.GetValues(typeof(TelemetryType))
                       .OfType<TelemetryType>()
                       .Where(type => type != telemetryType)
                       .Where(type => type != TelemetryType.Trace)
                       .Select(type => new object[] {type})
                       .ToArray();
        }
    }
}