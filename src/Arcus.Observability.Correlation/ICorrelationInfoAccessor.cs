namespace Arcus.Observability.Correlation 
{
    /// <summary>
    /// Represents a contract to access the <see cref="Correlation.CorrelationInfo"/> model.
    /// </summary>
    public interface ICorrelationInfoAccessor
    {
        /// <summary>
        /// Gets or sets the current correlation information initialized in this context.
        /// </summary>
        CorrelationInfo CorrelationInfo { get; set; }
    }
}