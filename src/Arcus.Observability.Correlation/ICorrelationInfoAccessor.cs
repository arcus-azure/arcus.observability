namespace Arcus.Observability.Correlation 
{
    /// <summary>
    /// Represents a contract to access the <see cref="CorrelationInfo"/> model.
    /// </summary>
    public interface ICorrelationInfoAccessor : ICorrelationInfoAccessor<CorrelationInfo>
    {
        /// <summary>
        /// Gets the current correlation information initialized in this context.
        /// </summary>
        CorrelationInfo GetCorrelationInfo();
    }

}