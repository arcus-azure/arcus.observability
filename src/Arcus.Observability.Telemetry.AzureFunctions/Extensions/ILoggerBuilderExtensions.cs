using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// <see cref="ILoggingBuilder"/> extensions related to Azure Functions projects.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggingBuilderExtensions
    {
        /// <summary>
        /// Removes Microsoft's 'ApplicationInsightsLoggerProvider' type from the registered logging providers
        /// so that Arcus' Serilog Application Insights sink is the only one place telemetry is created and sent to Microsoft Application Insights.
        /// </summary>
        /// <param name="builder">The builder instance to add and remove logging providers.</param>
        /// <returns>A <see cref="ILoggingBuilder"/> without Microsoft's 'ApplicationInsightsLoggerProvider' registration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no 'ApplicationInsightsLoggerProvider' instance can be found in the <paramref name="builder"/>.</exception>
        public static ILoggingBuilder RemoveMicrosoftApplicationInsightsLoggerProvider(this ILoggingBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder), "Requires an instance of the logging builder to remove Microsoft's 'ApplicationInsightsLoggerProvider'");
            }

            ServiceDescriptor descriptor = 
                builder.Services.FirstOrDefault(service => service.ImplementationType?.Name == "ApplicationInsightsLoggerProvider");
            
            if (descriptor != null)
            {
                builder.Services.Remove(descriptor);
            }

            return builder;
        }
    }
}
