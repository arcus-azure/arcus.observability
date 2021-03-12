using System;
using System.Text.RegularExpressions;
using GuardNet;

namespace Arcus.Observability.Telemetry.Serilog.Sinks.ApplicationInsights.Configuration
{
    /// <summary>
    /// User-defined configuration options to influence the behavior of the Azure Application Insights Serilog sink while tracking exceptions.
    /// </summary>
    public class ApplicationInsightsSinkExceptionOptions
    {
        private static readonly Regex PropertyFormatRegex = new Regex(@"[\{\{0\}\}]", RegexOptions.Compiled);
        private string _propertyFormat = "Exception-{0}";

        /// <summary>
        /// Gets or sets the flag indicating whether or not the properties of the exception should be included as custom dimensions in the exception tracking.
        /// </summary>
        /// <remarks>
        ///     Only the current-level properties are considered during the exception tracking; inherited properties will be excluded.
        /// </remarks>
        public bool IncludeProperties { get; set; } = false;
        
        /// <summary>
        /// <para>Gets or sets the string format to track (public) exception properties.</para>
        /// <para>Default: <code>Exception-{0}</code></para>
        /// </summary>
        /// <remarks>
        ///     Make sure that you pass along a valid string format where there's room for a single string argument (`{0}` should be included in the format),
        ///     otherwise an <see cref="FormatException"/> will be thrown further up the way.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        /// <exception cref="FormatException">
        ///     Thrown when the <paramref name="value"/> is not in the correct format to be used as a string format to track (public) exception properties.
        /// </exception>
        public string PropertyFormat
        {
            get => _propertyFormat;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Requires a non-blank string format to track (public) exception properties");
                Guard.For<FormatException>(() => PropertyFormatRegex.Matches(value).Count != 3, "Requires a single occurrence of the substring '{0}' in the exception property format");

                _propertyFormat = value;
            }
        }
    }
}