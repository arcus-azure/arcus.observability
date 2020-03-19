using System.Threading;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Default <see cref="ICorrelationInfoAccessor"/> implementation to access the <typeparamref name="TCorrelationInfo"/> in the current context.
    /// </summary>
    public class DefaultCorrelationInfoAccessor<TCorrelationInfo> : ICorrelationInfoAccessor<TCorrelationInfo> where TCorrelationInfo : CorrelationInfo 
    {
        private static readonly AsyncLocal<TCorrelationInfo> CorrelationInfoLocalData = new AsyncLocal<TCorrelationInfo>();

        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        public TCorrelationInfo CorrelationInfo
        {
            get => CorrelationInfoLocalData.Value;
            set => CorrelationInfoLocalData.Value = value;
        }
    }
}
