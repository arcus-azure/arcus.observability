// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging 
{
    /// <summary>
    /// Corresponds with the type of the dependency type when logging an Azure Service Bus dependency.
    /// </summary>
    public enum ServiceBusEntityType
    {
        /// <summary>
        /// Sets the dependency type to a unknown entity.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Sets the dependency type to a queue.
        /// </summary>
        Queue = 1,

        /// <summary>
        /// Sets the dependency type to a topic.
        /// </summary>
        Topic = 2,
    }
}