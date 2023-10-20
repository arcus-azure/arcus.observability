using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using Arcus.Observability.Telemetry.Core.Logging;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Key Vault dependency")]
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DurationMeasurement measurement,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="measurement">Measuring the latency to call the dependency</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        [Obsolete("Use the overload with " + nameof(DurationMeasurement) + " instead to track an Azure Key Vault dependency")]
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DependencyMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            context = context ?? new Dictionary<string, object>();
            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="measurement">The measurement of the duration to call the dependency.</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="measurement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DurationMeasurement measurement,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotNull(measurement, nameof(measurement), "Requires a dependency measurement instance to track the latency of the Azure Key Vault when tracking an Azure Key Vault dependency");

            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, measurement.StartTime, measurement.Elapsed, dependencyId, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Key Vault operation");

            LogAzureKeyVaultDependency(logger, vaultUri, secretName, isSuccessful, startTime, duration, dependencyId: null, context);
        }

        /// <summary>
        /// Logs an Azure Key Vault dependency.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="vaultUri">The URI pointing to the Azure Key Vault resource.</param>
        /// <param name="secretName">The secret that is being used within the Azure Key Vault resource.</param>
        /// <param name="isSuccessful">Indication whether or not the operation was successful</param>
        /// <param name="startTime">Point in time when the interaction with the HTTP dependency was started</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="dependencyId">The ID of the dependency to link as parent ID.</param>
        /// <param name="context">Context that provides more insights on the dependency that was measured</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="vaultUri"/> or <paramref name="secretName"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="secretName"/> is not in the correct format.</exception>
        /// <exception cref="UriFormatException">Thrown when the <paramref name="vaultUri"/> is not in the correct format.</exception>
        public static void LogAzureKeyVaultDependency(
            this ILogger logger,
            string vaultUri,
            string secretName,
            bool isSuccessful,
            DateTimeOffset startTime,
            TimeSpan duration,
            string dependencyId,
            Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the Azure Key Vault dependency");
            Guard.NotNullOrWhitespace(vaultUri, nameof(vaultUri), "Requires a non-blank URI for the Azure Key Vault");
            Guard.NotNullOrWhitespace(secretName, nameof(secretName), "Requires a non-blank secret name for the Azure Key Vault");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the Azure Key Vault operation");

            context = context is null ? new Dictionary<string, object>() : new Dictionary<string, object>(context);

            logger.LogWarning(MessageFormats.DependencyFormat, new DependencyLogEntry(
                dependencyType: "Azure key vault",
                dependencyName: vaultUri,
                dependencyData: secretName,
                targetName: vaultUri,
                duration: duration,
                dependencyId: dependencyId,
                startTime: startTime,
                resultCode: null,
                isSuccessful: isSuccessful,
                context: context));
        }
    }
}
