using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Bogus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry.Logging
{
    [Trait("Category", "Unit")]
    public class AzureKeyVaultDependencyLoggingTests
    {
        private static readonly Faker Bogus = new Faker();

        [Fact]
        public void LogAzureKeyVaultDependency_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new TestLogger();
            var vaultUri = Bogus.Internet.Url();
            var secretName = Bogus.Lorem.Word();
            bool isSuccessful = Bogus.Random.Bool();
            var startTime = Bogus.Date.RecentOffset();
            var duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();
            var key = Bogus.Lorem.Word();
            var value = Bogus.Random.Guid().ToString();
            var context = new Dictionary<string, object> { [key] = value };

            // Act
            logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            string message = logger.WrittenMessage;
            Assert.Contains(vaultUri, message);
            Assert.Contains(secretName, message);
            Assert.Contains(isSuccessful.ToString(), message);
            Assert.Contains(startTime.ToString(FormatSpecifiers.InvariantTimestampFormat), message);
            Assert.Contains(duration.ToString(), message);
            Assert.Contains(dependencyId, message);
            Assert.Contains(key, message);
            Assert.Contains(value, message);
        }

        [Fact]
        public void LogAzureKeyVaultDependency_WithContext_DoesNotAlterContext()
        {
            // Arrange
            var logger = new TestLogger();
            var vaultUri = Bogus.Internet.Url();
            var secretName = Bogus.Lorem.Word();
            bool isSuccessful = Bogus.Random.Bool();
            var startTime = Bogus.Date.RecentOffset();
            var duration = Bogus.Date.Timespan();
            var dependencyId = Bogus.Random.Guid().ToString();
            var context = new Dictionary<string, object>();

            // Act
            logger.LogAzureKeyVaultDependency(vaultUri, secretName, isSuccessful, startTime, duration, dependencyId, context);

            // Assert
            Assert.Empty(context);
        }

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
