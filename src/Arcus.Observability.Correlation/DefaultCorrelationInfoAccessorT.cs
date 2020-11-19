using System;
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
        /// Prevents a new instance of the <see cref="DefaultCorrelationInfoAccessor"/> class from being created.
        /// </summary>
        public DefaultCorrelationInfoAccessor()
        {
        }

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

        /// <summary>
        /// Gets the default instance for the <see cref="DefaultCorrelationInfoAccessor{TCorrelation}"/> class.
        /// </summary>
        [Obsolete("Create a new instance instead of using this static value")]
        public static DefaultCorrelationInfoAccessor<TCorrelationInfo> Instance { get; } = new DefaultCorrelationInfoAccessor<TCorrelationInfo>();
    }
}
