using System;
using System.Collections.Generic;
using System.Text;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Converters;
using Bogus;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class CloudContextConverterTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Fact]
        public void EnrichWithAppInfo_TraceWithComponentNameAndPodName_CloudContextRoleNameAndRoleInstanceSet()
        {
            // Arrange
            string roleName = _bogusGenerator.Name.JobArea();
            string roleInstance = _bogusGenerator.Name.JobType() + _bogusGenerator.Random.Number();

            LogEvent logEvent = CreateLogEvent(
                new LogEventProperty(ContextProperties.General.ComponentName, new ScalarValue(roleName)),
                new LogEventProperty(ContextProperties.Kubernetes.PodName, new ScalarValue(roleInstance)));
            
            var telemetry = new TraceTelemetry();
            var converter = new CloudContextConverter();

            // Act
            converter.EnrichWithAppInfo(logEvent, telemetry);

            // Assert
            Assert.Equal(roleName, telemetry.Context.Cloud.RoleName);
            Assert.Equal(roleInstance, telemetry.Context.Cloud.RoleInstance);
        }

        [Fact]
        public void EnrichWithAppInfo_EventWithComponentNameAndMachineName_CloudContextRoleNameAndRoleInstanceSet()
        {
            // Arrange
            string roleName = _bogusGenerator.Name.JobArea();
            string roleInstance = _bogusGenerator.Name.JobType() + _bogusGenerator.Random.Number();

            LogEvent logEvent = CreateLogEvent(
                new LogEventProperty(ContextProperties.General.ComponentName, new ScalarValue(roleName)),
                new LogEventProperty(ContextProperties.General.MachineName, new ScalarValue(roleInstance)));
            
            var telemetry = new EventTelemetry();
            var converter = new CloudContextConverter();

            // Act
            converter.EnrichWithAppInfo(logEvent, telemetry);

            // Assert
            Assert.Equal(roleName, telemetry.Context.Cloud.RoleName);
            Assert.Equal(roleInstance, telemetry.Context.Cloud.RoleInstance);
        }

        [Fact]
        public void EnrichWithAppInfo_EventWithComponentNameAndPodNameAndMachineName_CloudContextRoleNameAndPodNameRoleInstanceSet()
        {
            // Arrange
            string roleName = _bogusGenerator.Name.JobArea();
            string podRoleInstance = _bogusGenerator.Name.JobType() + _bogusGenerator.Random.Number();
            string machineRoleInstance = _bogusGenerator.Name.JobType() + _bogusGenerator.Random.Number();

            LogEvent logEvent = CreateLogEvent(
                new LogEventProperty(ContextProperties.General.ComponentName, new ScalarValue(roleName)),
                new LogEventProperty(ContextProperties.Kubernetes.PodName, new ScalarValue(podRoleInstance)),
                new LogEventProperty(ContextProperties.General.MachineName, new ScalarValue(machineRoleInstance)));
            
            var telemetry = new EventTelemetry();
            var converter = new CloudContextConverter();

            // Act
            converter.EnrichWithAppInfo(logEvent, telemetry);

            // Assert
            Assert.Equal(roleName, telemetry.Context.Cloud.RoleName);
            Assert.Equal(podRoleInstance, telemetry.Context.Cloud.RoleInstance);
        }

        [Fact]
        public void EnrichWithAppInfo_DependencyWithoutComponentNameAndPodName_CloudContextRoleNameAndRoleInstanceSet()
        {
            // Arrange
            LogEvent logEvent = CreateLogEvent();
            
            var telemetry = new TraceTelemetry();
            var converter = new CloudContextConverter();

            // Act
            converter.EnrichWithAppInfo(logEvent, telemetry);

            // Assert
            Assert.Null(telemetry.Context.Cloud.RoleName);
            Assert.Null(telemetry.Context.Cloud.RoleInstance);
        }

        private LogEvent CreateLogEvent(params LogEventProperty[] properties)
        {
            DateTimeOffset timestamp = _bogusGenerator.Date.PastOffset();
            var message = new MessageTemplate(_bogusGenerator.Lorem.Sentence(), new List<MessageTemplateToken>());

            LogEventLevel successfulLevel = _bogusGenerator.PickRandom(LogEventLevel.Information, LogEventLevel.Debug, LogEventLevel.Verbose);
            var successfulLogEvent = new LogEvent(timestamp, successfulLevel, exception: null, message, properties);
            
            Exception exception = _bogusGenerator.System.Exception();
            LogEventLevel failureLevel = _bogusGenerator.PickRandom(LogEventLevel.Error, LogEventLevel.Fatal, LogEventLevel.Warning);
            var failureLogEvent = new LogEvent(timestamp, failureLevel, exception, message, properties);

            LogEvent logEvent = _bogusGenerator.PickRandom(successfulLogEvent, failureLogEvent);
            return logEvent;
        }
    }
}
