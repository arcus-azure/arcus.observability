using System;
using GuardNet;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Correlation options specific for the operation ID.
    /// </summary>
    [Obsolete("Use HTTP or messaging-specific correlation options instead")]
    public class CorrelationInfoOperationOptions
    {
        private string _headerName = "RequestId";
        private Func<string> _generateId = () => Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets whether to include the operation ID in the response.
        /// </summary>
        /// <remarks>
        ///     A common use case is to disable tracing info in edge services, so that such details are not exposed to the outside world.
        /// </remarks>
        public bool IncludeInResponse { get; set; } = true;

        /// <summary>
        /// Gets or sets the header that will contain the response operation ID.
        /// </summary>
        public string HeaderName
        {
            get => _headerName;
            set
            {
                Guard.NotNullOrWhitespace(value, nameof(value), "Correlation operation header cannot be blank");
                _headerName = value;
            }
        }

        /// <summary>
        /// Gets or sets the function to generate the operation ID when the <see cref="IncludeInResponse"/> is set to <c>true</c> (default: <c>true</c>).
        /// </summary>
        public Func<string> GenerateId
        {
            get => _generateId;
            set
            {
                Guard.NotNull(value, nameof(value), "Correlation function to generate an operation ID cannot be 'null'");
                _generateId = value;
            }
        }
    }
}
