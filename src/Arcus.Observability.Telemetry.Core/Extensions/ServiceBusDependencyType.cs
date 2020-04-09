// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging 
{
    /// <summary>
    /// Corresponds with the type of the dependency type when logging an Azure Service Bus dependency.
    /// </summary>
    public enum ServiceBusDependencyType
    {
        /// <summary>
        /// Sets the dependency type to a topic.
        /// </summary>
        Topic, 
        
        /// <summary>
        /// Sets the dependency type to a queue.
        /// </summary>
        Queue
    }
}