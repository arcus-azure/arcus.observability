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
    }
}
