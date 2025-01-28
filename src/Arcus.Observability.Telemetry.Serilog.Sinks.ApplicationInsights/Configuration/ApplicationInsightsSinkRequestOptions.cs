using System;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// User-defined configuration options to influence the behavior of the Azure Application Insights Serilog sink while tracking requests.
    /// </summary>
    public class ApplicationInsightsSinkRequestOptions
    {
        private Func<string> _generatedId = Guid.NewGuid().ToString;

        /// <summary>
        /// <para>Gets or sets the function to generate the request ID of the telemetry model while tracking requests.</para>
        /// <para>Default: GUID generation.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is <c>null</c>.</exception>
        public Func<string> GenerateId
        {
            get => _generatedId;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value), "Requires a function to generate the request ID of the telemetry model while tracking requests");
                }

                _generatedId = value;
            }
        }
    }
}
