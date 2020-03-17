namespace Arcus.Observability.Correlation
{
    /// <summary>
    /// Represents a contract to access the <typeparamref name="TCorrelationInfo"/> model.
    /// </summary>
    public interface ICorrelationInfoAccessor<TCorrelationInfo> 
        where TCorrelationInfo : CorrelationInfo
    {
        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        TCorrelationInfo CorrelationInfo { get; set; }
    }
}
