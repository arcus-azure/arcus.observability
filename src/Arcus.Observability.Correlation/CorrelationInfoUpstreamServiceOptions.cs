using System;
using GuardNet;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Correlation options specific to the upstream services, used in the <see cref="CorrelationInfoOptions"/>.
    /// </summary>
    public class CorrelationInfoUpstreamServiceOptions
    {
        private string _operationParentIdHeaderName = "Request-Id";
        private Func<string> _generateId = () => Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the flag indicating whether or not the upstream service information should be extracted from the <see cref="OperationParentIdHeaderName"/> following the W3C Trace-Context standard. 
        /// </summary>
        public bool ExtractFromRequest { get; set; } = true;

        /// <summary>
        /// Gets or sets the request header name where te operation parent ID is located (default: <c>"Request-Id"</c>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is blank.</exception>
        public string OperationParentIdHeaderName
        {
            get => _operationParentIdHeaderName;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Requires a non-blank value for the operation parent ID request header name");
                _operationParentIdHeaderName = value;
            }
        }

        /// <summary>
        /// Gets or sets the function to generate the operation parent ID without extracting from the request.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is <c>null</c>.</exception>
        public Func<string> GenerateId
        {
            get => _generateId;
            set
            {
                Guard.NotNull(value, nameof(value), "Requires a function to generate the operation parent ID");
                _generateId = value;
            }
        }
    }
}