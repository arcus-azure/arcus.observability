using System;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights.Fixture;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsSinkExtensionTests : ApplicationInsightsSinkTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsSinkExtensionTests" /> class.
        /// </summary>
        public ApplicationInsightsSinkExtensionTests(ITestOutputHelper outputWriter)
            : base(outputWriter)
        {
        }

        [Fact]
        public async Task Sink_WithConnectionString_WritesTelemetry()
        {
            // Arrange
            string connectionString = $"InstrumentationKey={InstrumentationKey}";
            var configuration = new LoggerConfiguration()
               .WriteTo.AzureApplicationInsightsWithConnectionString(connectionString);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Message);
                });
            });
        }

        [Fact]
        public async Task Sink_WithConnectionStringWithServiceProvider_WritesTelemetry()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<TelemetryClient>();
            IServiceProvider provider = services.BuildServiceProvider();

            string connectionString = $"InstrumentationKey={InstrumentationKey}";
            var configuration = new LoggerConfiguration()
                .WriteTo.AzureApplicationInsightsWithConnectionString(provider, connectionString);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Message);
                });
            });
        }

        [Fact]
        public async Task Sink_WithInstrumentationKey_WritesTelemetry()
        {
            // Arrange
            var configuration = new LoggerConfiguration()
                .WriteTo.AzureApplicationInsightsWithInstrumentationKey(InstrumentationKey);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Message);
                });
            });
        }

          [Fact]
        public async Task Sink_WithInstrumentationKeyWithServiceProvider_WritesTelemetry()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<TelemetryClient>();
            IServiceProvider provider = services.BuildServiceProvider();
            var configuration = new LoggerConfiguration()
                .WriteTo.AzureApplicationInsightsWithInstrumentationKey(provider, InstrumentationKey);

            var message = "Something to log with connection string";
            ILogger logger = CreateLogger(configuration);

            // Act
            logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] traces = await client.GetTracesAsync();
                AssertX.Any(traces, trace =>
                {
                    Assert.Equal(message, trace.Message);
                });
            });
        }

        [Fact]
        public async Task SinkWithCustomTransactionIdPropertyName_TrackNonRequest_EnrichCorrelationCorrectly()
        {
            // Arrange
            string transactionIdPropertyName = "My-Transaction-Id";
            ApplicationInsightsSinkOptions.Correlation.TransactionIdPropertyName = transactionIdPropertyName;

            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor, options => options.TransactionIdPropertyName = transactionIdPropertyName);

            var message = "Something to log with custom transaction ID property name";

            // Act
            Logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] result = await client.GetTracesAsync();
                AssertX.Any(result, trace =>
                {
                    Assert.Equal(message, trace.Message);
                    Assert.Equal(correlation.TransactionId, trace.Operation.Id);
                    Assert.Equal(correlation.OperationId, trace.Operation.ParentId);
                });
            });
        }

        [Fact]
        public async Task SinkWithCustomOperationIdPropertyName_TrackNonRequest_EnrichCorrelationCorrectly()
        {
            // Arrange
            string operationIdPropertyName = "My-Operation-Id";
            ApplicationInsightsSinkOptions.Correlation.OperationIdPropertyName = operationIdPropertyName;

            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor, options => options.OperationIdPropertyName = operationIdPropertyName);

            var message = "Something to log with custom transaction ID property name";

            // Act
            Logger.LogInformation(message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsTraceResult[] result = await client.GetTracesAsync();
                AssertX.Any(result, trace =>
                {
                    Assert.Equal(message, trace.Message);
                    Assert.Equal(correlation.TransactionId, trace.Operation.Id);
                    Assert.Equal(correlation.OperationId, trace.Operation.ParentId);
                });
            });
        }

         [Fact]
        public async Task SinkWithCustomTransactionIdPropertyName_TrackRequest_EnrichCorrelationCorrectly()
        {
            // Arrange
            string transactionIdPropertyName = "My-Transaction-Id";
            ApplicationInsightsSinkOptions.Correlation.TransactionIdPropertyName = transactionIdPropertyName;

            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor, options => options.TransactionIdPropertyName = transactionIdPropertyName);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = new PathString("/service-b");

            using (var measurement = DurationMeasurement.Start())
            {
                // Act
                Logger.LogRequest(context.Request, context.Response, measurement);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] result = await client.GetRequestsAsync();
                AssertX.Any(result, request =>
                {
                    Assert.Equal(correlation.TransactionId, request.Operation.Id);
                    Assert.Equal(correlation.OperationId, request.Id);
                    Assert.Equal(correlation.OperationParentId, request.Operation.ParentId);
                });
            });
        }

        [Fact]
        public async Task SinkWithCustomOperationIdPropertyName_TrackRequest_EnrichCorrelationCorrectly()
        {
            // Arrange
            string operationIdPropertyName = "My-Operation-Id";
            ApplicationInsightsSinkOptions.Correlation.OperationIdPropertyName = operationIdPropertyName;

            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor, options => options.OperationIdPropertyName = operationIdPropertyName);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = new PathString("/service-b");

            using (var measurement = DurationMeasurement.Start())
            {
                // Act
                Logger.LogRequest(context.Request, context.Response, measurement);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] result = await client.GetRequestsAsync();
                AssertX.Any(result, request =>
                {
                    Assert.Equal(correlation.TransactionId, request.Operation.Id);
                    Assert.Equal(correlation.OperationId, request.Id);
                    Assert.Equal(correlation.OperationParentId, request.Operation.ParentId);
                });
            });
        }

        [Fact]
        public async Task SinkWithCustomOperationParentIdPropertyName_TrackRequest_EnrichCorrelationCorrectly()
        {
            // Arrange
            string operationIdPropertyName = "My-OperationParent-Id";
            ApplicationInsightsSinkOptions.Correlation.OperationParentIdPropertyName = operationIdPropertyName;

            var correlation = new CorrelationInfo($"operation-{Guid.NewGuid()}", $"transaction-{Guid.NewGuid()}", $"parent-{Guid.NewGuid()}");
            var accessor = new DefaultCorrelationInfoAccessor();
            accessor.SetCorrelationInfo(correlation);
            LoggerConfiguration.Enrich.WithCorrelationInfo(accessor, options => options.OperationParentIdPropertyName = operationIdPropertyName);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = new PathString("/service-b");

            using (var measurement = DurationMeasurement.Start())
            {
                // Act
                Logger.LogRequest(context.Request, context.Response, measurement);
            }

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsRequestResult[] result = await client.GetRequestsAsync();
                AssertX.Any(result, request =>
                {
                    Assert.Equal(correlation.TransactionId, request.Operation.Id);
                    Assert.Equal(correlation.OperationId, request.Id);
                    Assert.Equal(correlation.OperationParentId, request.Operation.ParentId);
                });
            });
        }
    }
}
