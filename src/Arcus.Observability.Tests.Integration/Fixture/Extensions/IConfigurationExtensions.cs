using System;
using System.Collections.Generic;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extensions on the <see cref="IConfiguration"/> to use reliable test input values.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Gets an required integration test configuration value with a given <paramref name="keyName"/>
        /// from the current integration test configuration instance.
        /// </summary>
        /// <param name="config">The current loaded integration test instance.</param>
        /// <param name="keyName">The name of the integration test key value within the loaded integration test instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="config"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="keyName"/> is blank.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="config"/> does not have a valid entry for the <paramref name="keyName"/> loaded.</exception>
        public static string GetRequiredValue(this IConfiguration config, string keyName)
        {
            Guard.NotNull(config, nameof(config));
            Guard.NotNullOrWhitespace(keyName, nameof(keyName));

            var keyValue = config.GetValue<string>(keyName);
            if (string.IsNullOrWhiteSpace(keyValue))
            {
                throw new KeyNotFoundException(
                    $"Cannot find an integration test configuration value with the key name: '{keyName}' because the value is blank, " +
                    $"please make sure to add one in your 'appsettings.json' and add a local variant in the 'appsettings.local.json'");
            }

            if (keyValue.StartsWith("#{") && keyValue.EndsWith("}#"))
            {
                throw new KeyNotFoundException(
                    $"Cannot find an integration test value with the key name: '{keyName}' because the value still contains the tokens '#{{ ... }}#', " +
                    $"please make sure to add a local variant in the 'appsettings.local.json'");
            }

            return keyValue;
        }
    }
}
