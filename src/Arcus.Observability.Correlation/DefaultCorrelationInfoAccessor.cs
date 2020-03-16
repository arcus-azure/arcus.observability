using System.Threading;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Default <see cref="ICorrelationInfoAccessor"/> implementation to access the <see cref="Correlation.CorrelationInfo"/> in the current context.
    /// </summary>
    public class DefaultCorrelationInfoAccessor : ICorrelationInfoAccessor
    {
        private static readonly AsyncLocal<CorrelationInfo> CorrelationInfoLocalData = new AsyncLocal<CorrelationInfo>();

        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        public CorrelationInfo CorrelationInfo
        {
            get => CorrelationInfoLocalData.Value;
            set => CorrelationInfoLocalData.Value = value;
        }
    }
}
