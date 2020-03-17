using GuardNet;

namespace Arcus.Observability.Correlation
{
    internal class CorrelationInfoAccessorProxy<TCorrelationInfo> : ICorrelationInfoAccessor
        where TCorrelationInfo : CorrelationInfo
    {
        private readonly ICorrelationInfoAccessor<TCorrelationInfo> _correlationInfoAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoAccessorProxy{TCorrelationInfo}"/> class.
        /// </summary>
        internal CorrelationInfoAccessorProxy(ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor)
        {
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));

            _correlationInfoAccessor = correlationInfoAccessor;
        }

        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        public CorrelationInfo CorrelationInfo
        {
            get => _correlationInfoAccessor.CorrelationInfo;
            set
            {
                if (value is TCorrelationInfo correlationInfo)
                {
                    _correlationInfoAccessor.CorrelationInfo = correlationInfo;
                }
            }
        }
    }
}
