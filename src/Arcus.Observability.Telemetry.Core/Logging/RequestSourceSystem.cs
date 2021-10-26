namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents the system from where the request came from.
    /// </summary>
    public enum RequestSourceSystem
    {
        /// <summary>
        /// Sets the request source system as an Azure Service Bus queue or topic.
        /// </summary>
        AzureServiceBus, 
        
        /// <summary>
        /// Sets the request source system as an web API endpoint.
        /// </summary>
        Http
    }
}