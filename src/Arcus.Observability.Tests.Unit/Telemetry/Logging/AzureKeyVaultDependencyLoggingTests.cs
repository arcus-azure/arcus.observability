using System;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class AzureKeyVaultDependencyLoggingTests
    {
        private readonly Faker _bogusGenerator = new Faker();

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretName_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(5), "dependency ID"));
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithIdWithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", "MySecret", isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration, "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5)));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretName_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: TimeSpan.FromSeconds(value: 5)));
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new TestLogger();
            TimeSpan duration = TimeSpanGenerator.GeneratePositiveDuration().Negate();

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", "MySecret", isSuccessful: true, startTime: DateTimeOffset.UtcNow, duration: duration));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDependencyMeasurement_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DependencyMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretNameDependencyMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DependencyMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDependencyMeasurement_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DependencyMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretNameDependencyMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DependencyMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDurationMeasurement_WithIdWithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DurationMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithIdWithoutSecretNameDurationMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DurationMeasurement.Start(), "dependency ID"));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependencyDurationMeasurement_WithoutVaultUri_Fails(string vaultUri)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency(vaultUri, "MySecret", isSuccessful: true, measurement: DurationMeasurement.Start()));
        }

        [Theory]
        [ClassData(typeof(Blanks))]
        public void LogAzureKeyVaultDependency_WithoutSecretNameDurationMeasurement_Fails(string secretName)
        {
            // Arrange
            var logger = new TestLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogAzureKeyVaultDependency("https://my-vault.vault.azure.net", secretName, isSuccessful: true, measurement: DurationMeasurement.Start()));
        }
    }
}
