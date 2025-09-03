using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcus.Observability.Correlation;
using Arcus.Observability.Tests.Integration.Fixture;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Serilog.Sinks.ApplicationInsights
{
    public class ExceptionTests : ApplicationInsightsSinkTests
    {
        public ExceptionTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
        }

        [Fact]
        public async Task LogException_SinksToApplicationInsights_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };

            // Act
            Logger.LogCritical(exception, exception.Message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.DoesNotContain($"Exception-{nameof(TestException.SpyProperty)}", result.CustomDimensions.Keys);
                });
            });
        }

        [Fact]
        public async Task LogException_SinksToApplicationInsightsWithIncludedProperties_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };
            ApplicationInsightsSinkOptions.Exception.IncludeProperties = true;

            // Act
            Logger.LogCritical(exception, exception.Message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.Equal(expectedProperty, Assert.Contains($"Exception-{nameof(TestException.SpyProperty)}", result.CustomDimensions));
                });
            });
        }

        [Fact]
        public async Task LogExceptionWithCustomPropertyFormat_SinksToApplicationInsights_ResultsInExceptionTelemetry()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string expectedProperty = BogusGenerator.Lorem.Word();
            var exception = new TestException(message) { SpyProperty = expectedProperty };
            string propertyFormat = "Exception.{0}";
            ApplicationInsightsSinkOptions.Exception.IncludeProperties = true;
            ApplicationInsightsSinkOptions.Exception.PropertyFormat = propertyFormat;
            TestLocation = TestLocation.Remote;

            // Act
            Logger.LogCritical(exception, exception.Message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    string propertyName = string.Format(propertyFormat, nameof(TestException.SpyProperty));

                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.Equal(expectedProperty, Assert.Contains(propertyName, result.CustomDimensions));
                });
            });
        }

        [Fact]
        public async Task LogExceptionWithComponentName_SinksToApplicationInsights_ResultsInTelemetryWithComponentName()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            string componentName = BogusGenerator.Commerce.ProductName();
            var exception = new PlatformNotSupportedException(message);
            LoggerConfiguration.Enrich.WithComponentName(componentName);

            // Act
            Logger.LogCritical(exception, exception.Message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.Equal(componentName, result.Cloud.RoleName);
                });
            });
        }

        [Fact]
        public async Task LogExceptionWithCorrelationInfo_SinksToApplicationInsights_ResultsInTelemetryWithCorrelationInfo()
        {
            // Arrange
            string message = BogusGenerator.Lorem.Sentence();
            var exception = new PlatformNotSupportedException(message);

            string operationId = $"operation-{Guid.NewGuid()}";
            string transactionId = $"transaction-{Guid.NewGuid()}";
            string operationParentId = $"operation-parent-{Guid.NewGuid()}";

            var correlationInfoAccessor = new DefaultCorrelationInfoAccessor();
            correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(operationId, transactionId, operationParentId));
            LoggerConfiguration.Enrich.WithCorrelationInfo(correlationInfoAccessor);

            // Act
            Logger.LogCritical(exception, exception.Message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.Equal(transactionId, result.Operation.Id);
                    Assert.Equal(operationId, result.Operation.ParentId);
                });
            });
        }

        [Fact]
        public async Task LogWarning_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            Exception exception = BogusGenerator.System.Exception();
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogWarning(exception, "Exception message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }

        [Fact]
        public async Task LogError_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            Exception exception = BogusGenerator.System.Exception();
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogError(exception, "Exception message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }

        [Fact]
        public async Task LogCritical_SinksToApplicationInsights_ResultsInTraceTelemetry()
        {
            // Arrange
            Exception exception = BogusGenerator.System.Exception();
            string message = BogusGenerator.Lorem.Sentence();
            Dictionary<string, object> telemetryContext = CreateTestTelemetryContext();
            string key = BogusGenerator.Lorem.Word();
            string expected = BogusGenerator.Lorem.Word();
            telemetryContext[key] = expected;

            // Act
            Logger.LogCritical(exception, "Exception message '{Sentence}'", telemetryContext, message);

            // Assert
            await RetryAssertUntilTelemetryShouldBeAvailableAsync(async client =>
            {
                EventsExceptionResult[] results = await client.GetExceptionsAsync();
                AssertX.Any(results, result =>
                {
                    Assert.Equal(exception.Message, result.Exception.OuterMessage);
                    Assert.True(result.CustomDimensions.TryGetValue(key, out string actual), "Should contain custom dimension property");
                    Assert.Equal(expected, actual);
                });
            });
        }
    }
}