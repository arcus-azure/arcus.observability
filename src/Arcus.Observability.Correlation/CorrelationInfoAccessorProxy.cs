using System;

namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Internal proxy implementation to register an <see cref="ICorrelationInfoAccessor{TCorrelationInfo}"/> as an <see cref="ICorrelationInfoAccessor"/>.
    /// </summary>
    /// <typeparam name="TCorrelationInfo">The custom <see cref="Correlation.CorrelationInfo"/> model.</typeparam>
    internal class CorrelationInfoAccessorProxy<TCorrelationInfo> : ICorrelationInfoAccessor
        where TCorrelationInfo : CorrelationInfo
    {
        private readonly ICorrelationInfoAccessor<TCorrelationInfo> _correlationInfoAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfoAccessorProxy{TCorrelationInfo}"/> class.
        /// </summary>
        internal CorrelationInfoAccessorProxy(ICorrelationInfoAccessor<TCorrelationInfo> correlationInfoAccessor)
        {
            if (correlationInfoAccessor is null)
            {
                throw new ArgumentNullException(nameof(correlationInfoAccessor));
            }

            _correlationInfoAccessor = correlationInfoAccessor;
        }

        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        public CorrelationInfo GetCorrelationInfo()
        {
            return _correlationInfoAccessor.GetCorrelationInfo();
        }

        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        public void SetCorrelationInfo(CorrelationInfo correlationInfo)
        {
            if (correlationInfo is TCorrelationInfo typedCorrelationInfo)
            {
                _correlationInfoAccessor.SetCorrelationInfo(typedCorrelationInfo);
            }
        }
    }
}
