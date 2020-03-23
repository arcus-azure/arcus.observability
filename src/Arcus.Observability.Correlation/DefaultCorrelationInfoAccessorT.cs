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
        /// Prevents a new instance of the <see cref="DefaultCorrelationInfoAccessor"/> class from being created.
        /// </summary>
        private protected DefaultCorrelationInfoAccessor()
        {
        }

        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        public TCorrelationInfo GetCorrelationInfo()
        {
            return CorrelationInfoLocalData.Value;
        }

        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        public void SetCorrelationInfo(TCorrelationInfo correlationInfo)
        {
            CorrelationInfoLocalData.Value = correlationInfo;
        }

        /// <summary>
        /// Gets the default instance for the <see cref="DefaultCorrelationInfoAccessor{TCorrelation}"/> class.
        /// </summary>
        public static DefaultCorrelationInfoAccessor<TCorrelationInfo> Instance { get; } = new DefaultCorrelationInfoAccessor<TCorrelationInfo>();
    }
}
