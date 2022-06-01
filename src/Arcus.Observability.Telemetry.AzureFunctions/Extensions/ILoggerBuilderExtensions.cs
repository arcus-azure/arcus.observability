using System;
using System.Linq;
using GuardNet;
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
        /// Clears the <see cref="ILoggerProvider"/> registrations from the given <paramref name="loggingBuilder"/>,
        /// except the specific Azure Functions registrations.
        /// </summary>
        /// <param name="loggingBuilder">The builder containing the <see cref="ILoggerProvider"/> registrations.</param>
        [Obsolete("Calling this method causes issues with correctly writing log-information to Application Insights. It is advised to no longer use it.")]
        public static ILoggingBuilder ClearProvidersExceptFunctionProviders(this ILoggingBuilder loggingBuilder)
        {
            Guard.NotNull(loggingBuilder, nameof(loggingBuilder));

            // Kudos to katrash: https://stackoverflow.com/questions/45986517/remove-console-and-debug-loggers-in-asp-net-core-2-0-when-in-production-mode
            foreach (ServiceDescriptor serviceDescriptor in loggingBuilder.Services)
            {
                if (serviceDescriptor.ServiceType == typeof(ILoggerProvider))
                {
                    if (serviceDescriptor.ImplementationType.FullName != "Microsoft.Azure.WebJobs.Script.Diagnostics.HostFileLoggerProvider"
                        && serviceDescriptor.ImplementationType.FullName != "Microsoft.Azure.WebJobs.Script.Diagnostics.FunctionFileLoggerProvider")
                    {
                        loggingBuilder.Services.Remove(serviceDescriptor);
                        break;
                    }
                }
            }

            return loggingBuilder;
        }

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
            Guard.NotNull(builder, nameof(builder), "Requires an instance of the logging builder to remove Microsoft's 'ApplicationInsightsLoggerProvider'");

            ServiceDescriptor descriptor = 
                builder.Services.FirstOrDefault(service => service.ImplementationType?.Name == "ApplicationInsightsLoggerProvider");
            
            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    "Cannot find the type 'ApplicationInsightsLoggerProvider' in the registered logging providers " 
                    + "so can't guarantee a correct Arcus implementation that sinks telemetry to Microsoft Application Insights , "
                    + "please remove this logger provider before sending telemetry via the Arcus Application Insights logging");
            }

            builder.Services.Remove(descriptor);
            return builder;
        }
    }
}
