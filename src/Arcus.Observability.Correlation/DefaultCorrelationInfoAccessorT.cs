using System.Threading;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Default <see cref="ICorrelationInfoAccessor"/> implementation to access the <typeparamref name="TCorrelationInfo"/> in the current context.
    /// </summary>
    public class DefaultCorrelationInfoAccessor<TCorrelationInfo> : ICorrelationInfoAccessor<TCorrelationInfo> 
        where TCorrelationInfo : CorrelationInfo 
    {
        private TCorrelationInfo _correlationInfo;

        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        public TCorrelationInfo GetCorrelationInfo()
        {
            return _correlationInfo;
        }

        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        public void SetCorrelationInfo(TCorrelationInfo correlationInfo)
        {
            Interlocked.Exchange(ref _correlationInfo, correlationInfo);
        }
    }
}
