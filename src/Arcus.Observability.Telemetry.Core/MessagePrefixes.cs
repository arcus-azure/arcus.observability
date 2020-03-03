using System;
using System.Collections.Generic;
using System.Text;

namespace Arcus.Observability.Telemetry.Core
{
    public class MessagePrefixes
    {
        public const string RequestViaHttp = "HTTP Request";
        public const string DependencyViaHttp = "HTTP Dependency";
        public const string DependencyViaSql = "SQL Dependency";
        public const string Event = "Events";
    }
}
