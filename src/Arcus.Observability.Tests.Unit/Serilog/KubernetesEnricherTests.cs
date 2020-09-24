using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Serilog;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    [Trait("Category", "Unit")]
    public class KubernetesEnricherTests
    {
        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithKubernetesInfo_WithBlankNodeName_Throws(string nodeNamePropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithKubernetesInfo(nodeNamePropertyName: nodeNamePropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithKubernetesInfo_WithBlankPodName_Throws(string podNamePropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithKubernetesInfo(podNamePropertyName: podNamePropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void WithKubernetesInfo_WithBlankNamespace_Throws(string namespacePropertyName)
        {
            // Arrange
            var configuration = new LoggerConfiguration();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => configuration.Enrich.WithKubernetesInfo(namespacePropertyName: namespacePropertyName));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankNodeName_Throws(string nodeNamePropertyName)
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new KubernetesEnricher(
                    nodeNamePropertyName: nodeNamePropertyName,
                    podNamePropertyName: "some valid ignored value",
                    namespacePropertyName: "some other valid ignored value"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankPodName_Throws(string podNamePropertyName)
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new KubernetesEnricher(
                    nodeNamePropertyName: "some valid ignored value",
                    podNamePropertyName: podNamePropertyName,
                    namespacePropertyName: "some other valid ignored value"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void CreateEnricher_WithBlankNamespace_Throws(string namespacePropertyName)
        {
            Assert.ThrowsAny<ArgumentException>(
                () => new KubernetesEnricher(
                    nodeNamePropertyName: "some valid ignored value",
                    podNamePropertyName: "some other valid ignored value",
                    namespacePropertyName: namespacePropertyName));
        }
    }
}
