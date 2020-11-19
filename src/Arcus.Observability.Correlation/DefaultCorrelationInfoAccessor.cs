using System;

namespace Arcus.Observability.Correlation 
{
    /// <summary>
    /// Default <see cref="ICorrelationInfoAccessor"/> implementation to access the <see cref="CorrelationInfo"/> in the current context.
    /// </summary>
    public class DefaultCorrelationInfoAccessor : DefaultCorrelationInfoAccessor<CorrelationInfo>, ICorrelationInfoAccessor
    {
        /// <summary>
        /// Prevents a new instance of the <see cref="DefaultCorrelationInfoAccessor"/> class from being created.
        /// </summary>
        public DefaultCorrelationInfoAccessor()
        {
        }

        /// <summary>
        /// Gets the default instance for the <see cref="DefaultCorrelationInfoAccessor"/> class.
        /// </summary>
        [Obsolete("Create a new instance instead of using this static value")]
        public new static DefaultCorrelationInfoAccessor Instance { get; } = new DefaultCorrelationInfoAccessor();
    }
}