namespace Arcus.Observability.Correlation 
{
    /// <summary>
    /// Default <see cref="ICorrelationInfoAccessor"/> implementation to access the <see cref="CorrelationInfo"/> in the current context.
    /// </summary>
    public class DefaultCorrelationInfoAccessor : DefaultCorrelationInfoAccessor<CorrelationInfo>, ICorrelationInfoAccessor
    {
    }
}