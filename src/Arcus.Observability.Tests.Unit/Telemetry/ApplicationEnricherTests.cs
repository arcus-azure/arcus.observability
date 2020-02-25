using System;
using Arcus.Observability.Telemetry.Serilog;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    [Trait("Category", "Unit")]
    public class ApplicationEnricherTests
    {
        private const string ComponentName = "ComponentName",
                             InstanceName = "InstanceName";

        [Fact]
        public void LogEvent_WithApplicationEnricherOnPodName_HasApplicationInformation()
        {
            // Arrange
            string componentName = $"component-{Guid.NewGuid()}";
            string podName = $"podname-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .Enrich.With(new ApplicationEnricher(componentName, ApplicationInstance.PodName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            using (TemporaryEnvironmentVariable.Create("KUBERNETES_POD_NAME", podName))
            {
                // Act
                logger.Information("This event will be enriched with application information"); 
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ComponentName, componentName),
                "Log event should contain component name property");
            Assert.True(
                logEvent.ContainsProperty(InstanceName, podName),
                "Log event should contain instance name = Kubernetes pod name");
        }

        [Fact]
        public void LogEvent_WithApplicationEnricherOnMachineName_HasApplicationInformation()
        {
            // Arrange
            string componentName = $"component-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.With(new ApplicationEnricher(componentName, ApplicationInstance.MachineName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will be enriched with application information");

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ComponentName, componentName),
                "Log event should contain component name property");
            Assert.True(
                logEvent.ContainsProperty(InstanceName, Environment.MachineName),
                "Log event should contain instance name = machine name");
        }

        [Fact]
        public void LogEventWithComponentNameProperty_WithApplicationEnricher_DoesntAlterComponentNameProperty()
        {
            // Arrange
            string expectedComponentName = $"expected-component-{Guid.NewGuid()}";
            string ignoredComponentName = $"ignored-component-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new ApplicationEnricher(ignoredComponentName, ApplicationInstance.MachineName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will not be enriched with component name because it already has one called '{ComponentName}'", expectedComponentName);

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(ComponentName, expectedComponentName),
                "Log event should not overwrite component name property");
        }

        [Fact]
        public void LogEventWithInstanceName_WithApplicationEnricherOnPodName_DoesntAlterInstanceNameProperty()
        {
            // Arrange
            string expectedPodName = $"expected-podname-{Guid.NewGuid()}";
            string ignoredPodName = $"ignored-podname-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With<KubernetesEnricher>()
                .Enrich.With(new ApplicationEnricher("ignored-component-name", ApplicationInstance.PodName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            using (TemporaryEnvironmentVariable.Create("KUBERNETES_POD_NAME", ignoredPodName))
            {
                logger.Information("This event will not be enriched with instance name = Kubernetes pod name because it already has one called '{InstanceName}'", expectedPodName);
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(InstanceName, expectedPodName),
                "Log event should not overwrite instance name property");
        }

        [Fact]
        public void LogEventWithInstanceName_WithApplicationEnricherOnMachineName_DoesntAlterInstanceNameProperty()
        {
            // Arrange
            string expectedMachineName = $"expected-machine-{Guid.NewGuid()}";
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.With(new ApplicationEnricher("ignored-component-name", ApplicationInstance.MachineName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will not be enriched with instance name = environment machine name because it already has one called '{InstanceName}'", expectedMachineName);

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.True(
                logEvent.ContainsProperty(InstanceName, expectedMachineName),
                "Log event should not overwrite instance name property");
        }

        [Fact]
        public void LogEvent_WithApplicationEnricherOnPodNameWithoutKubernetesEnricher_DoesntAddInstanceNameProperty()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new ApplicationEnricher("ignored-component-name", ApplicationInstance.PodName))
                .WriteTo.Sink(spy)
                .CreateLogger();
            
            using (TemporaryEnvironmentVariable.Create("KUBERNETES_POD_NAME", "ignored-pod-name"))
            {
                // Act
                logger.Information("This event will not be enriched with instance name because no Kubernetes pod name could be retrieved");
            }

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == InstanceName);
        }

        [Fact]
        public void LogEvent_WithApplicationEnricherOnMachineNameWithoutEnvironmentEnricher_DoesntAddInstanceNameProperty()
        {
            // Arrange
            var spy = new InMemoryLogSink();
            ILogger logger = new LoggerConfiguration()
                .Enrich.With(new ApplicationEnricher("ignored-component-name", ApplicationInstance.MachineName))
                .WriteTo.Sink(spy)
                .CreateLogger();

            // Act
            logger.Information("This event will not be enriched with instance name because no environment machine name could be retrieved");

            // Assert
            LogEvent logEvent = Assert.Single(spy.CurrentLogEmits);
            Assert.NotNull(logEvent);
            Assert.DoesNotContain(logEvent.Properties, prop => prop.Key == InstanceName);
        }
    }
}
