﻿using System;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Filters
{
    /// <summary>
    ///     Provides capability to filter telemetry based on its type
    /// </summary>
    public class TelemetryTypeFilter : ILogEventFilter
    {
        private TelemetryTypeFilter(TelemetryType telemetryType, bool? isTrackingEnabled)
        {
            Guard.For(() => !Enum.IsDefined(typeof(TelemetryType), telemetryType),
                new ArgumentOutOfRangeException(nameof(telemetryType), telemetryType, "Requires a type of telemetry that's within the supported value range of the enumeration"));

            TelemetryType = telemetryType;
            IsTrackingEnabled = isTrackingEnabled;
        }

        /// <summary>
        ///     Current telemetry type that should be filtered
        /// </summary>
        protected TelemetryType TelemetryType { get; }

        /// <summary>
        ///     Indication whether or not this telemetry type should be tracked
        /// </summary>
        protected bool? IsTrackingEnabled { get; }

        /// <summary>
        ///     Provides a telemetry filter based on its type
        /// </summary>
        /// <param name="telemetryType">Current telemetry type that should be filtered</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="telemetryType"/> is outside the bounds of the enumeration.</exception>
        public static TelemetryTypeFilter On(TelemetryType telemetryType)
        {
            Guard.For(() => !Enum.IsDefined(typeof(TelemetryType), telemetryType),
                new ArgumentOutOfRangeException(nameof(telemetryType), telemetryType, "Requires a type of telemetry that's within the supported value range of the enumeration"));

            // We cannot identify traces properly, so we do not allow Trace filters
            Guard.For<ArgumentException>(() => telemetryType == TelemetryType.Trace, "Filtering out traces is not supported");

            return new TelemetryTypeFilter(telemetryType, isTrackingEnabled: null);
        }

        /// <summary>
        ///     Provides a telemetry filter based on its type
        /// </summary>
        /// <param name="telemetryType">Current telemetry type that should be filtered</param>
        /// <param name="isTrackingEnabled">Indication whether or not this telemetry type should be tracked</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="telemetryType"/> is outside the bounds of the enumeration.</exception>
        public static TelemetryTypeFilter On(TelemetryType telemetryType, bool isTrackingEnabled)
        {
            Guard.For(() => !Enum.IsDefined(typeof(TelemetryType), telemetryType),
                new ArgumentOutOfRangeException(nameof(telemetryType), telemetryType, "Requires a type of telemetry that's within the supported value range of the enumeration"));

            // We cannot identify traces properly, so we do not allow Trace filters
            Guard.For<ArgumentException>(() => telemetryType == TelemetryType.Trace, "Filtering out traces is not supported");

            return new TelemetryTypeFilter(telemetryType, isTrackingEnabled);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogEvent logEvent)
        {
            if (IsTrackingEnabled is true)
            {
                return true;
            }

            switch (TelemetryType)
            {
                case TelemetryType.Dependency:
                    return IsFilteringRequired(ContextProperties.DependencyTracking.DependencyLogEntry, logEvent);
                case TelemetryType.Request:
                    return IsFilteringRequired(ContextProperties.RequestTracking.RequestLogEntry, logEvent);
                case TelemetryType.Events:
                    return IsFilteringRequired(ContextProperties.EventTracking.EventLogEntry, logEvent);
                case TelemetryType.Metrics:
                    return IsFilteringRequired(ContextProperties.MetricTracking.MetricLogEntry, logEvent);
                case TelemetryType.Trace:
                    // We cannot identify traces properly, so we always include them
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsFilteringRequired(string keyNameToFilterOut, LogEvent logEvent)
        {
            if (logEvent.Properties.ContainsKey(keyNameToFilterOut))
            {
                // Telemetry matches the type to filter out, so we should exclude it
                return false;
            }

            // Telemetry is of different type, so we should allow tracking of it
            return true;
        }
    }
}