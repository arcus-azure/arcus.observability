#pragma warning disable 1591
namespace Arcus.Observability.Telemetry.Core
{
    public static class ContextProperties
    {
        public static class DependencyTracking
        {
            public const string TargetName = "DependencyTargetName";
            public const string DependencyName = "DependencyName";
            public const string DependencyData = "DependencyData";
            public const string StartTime = "DependencyStartTime";
            public const string ResultCode = "DependencyResultCode";
            public const string Duration = "DependencyDuration";
            public const string IsSuccessful = "DependencyIsSuccessful";
        }

        public static class EventTracking
        {
            public const string EventName = "EventName";
            public const string EventContext = "EventDescription";
        }

        public static class Kubernetes
        {
            public const string Namespace = "Namespace";
            public const string NodeName = "NodeName";
            public const string PodName = "PodName";
        }

        public static class RequestTracking
        {
            public const string RequestMethod = "RequestMethod";
            public const string RequestHost = "RequestHost";
            public const string RequestUri = "RequestUri";
            public const string ResponseStatusCode = "ResponseStatusCode";
            public const string RequestDuration = "RequestDuration";
            public const string RequestTime = "RequestTime";
        }

        public static class MetricTracking
        {
            public const string MetricName = "MetricName";
            public const string MetricValue = "MetricValue";
        }
    }
}
