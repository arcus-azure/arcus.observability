namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Represents a contract to access the <typeparamref name="TCorrelationInfo"/> model.
    /// </summary>
    public interface ICorrelationInfoAccessor<TCorrelationInfo> 
        where TCorrelationInfo : CorrelationInfo
    {
        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        TCorrelationInfo GetCorrelationInfo();
        
        /// <summary>
        /// Sets the current correlation information for this context.
        /// </summary>
        /// <param name="correlationInfo">The correlation model to set.</param>
        void SetCorrelationInfo(TCorrelationInfo correlationInfo);
    }
}
