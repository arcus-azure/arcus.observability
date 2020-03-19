using Arcus.Observability.Telemetry.Core;
using Serilog.Core;
using Serilog.Events;

namespace Arcus.Observability.Telemetry.Serilog.Filters
{
    /// <summary>
    ///     Provides capability to filter telemetry based on its type
    /// </summary>
    public class TelemetryTypeFilter : ILogEventFilter
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="telemetryType">Current telemetry type that should be filtered</param>
        /// <param name="isTrackingEnabled">Indication whether or not this telemetry type should be tracked</param>
        private TelemetryTypeFilter(TelemetryType telemetryType, bool? isTrackingEnabled)
        {
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
        public static TelemetryTypeFilter On(TelemetryType telemetryType)
        {
            return new TelemetryTypeFilter(telemetryType, isTrackingEnabled: null);
        }

        /// <summary>
        ///     Provides a telemetry filter based on its type
        /// </summary>
        /// <param name="telemetryType">Current telemetry type that should be filtered</param>
        /// <param name="isTrackingEnabled">Indication whether or not this telemetry type should be tracked</param>
        public static TelemetryTypeFilter On(TelemetryType telemetryType, bool isTrackingEnabled)
        {
            return new TelemetryTypeFilter(telemetryType, isTrackingEnabled);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogEvent logEvent)
        {
            if (IsTrackingEnabled == true)
            {
                return true;
            }

            var telemetryType = logEvent.Properties.GetAsEnum<TelemetryType>(ContextProperties.General.TelemetryType);
            if (telemetryType == null)
            {
                return true;
            }

            // Tracking is disabled, so we want everything except dependencies
            return telemetryType != TelemetryType;
        }
    }
}