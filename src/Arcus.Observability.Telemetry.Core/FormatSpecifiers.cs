namespace Arcus.Observability.Telemetry.Core
{
    public static class FormatSpecifiers
    {
        /// <summary>
        /// A format specifier for converting DateTimeOffset instances to a string representation
        /// using the highest precision that is available.
        /// </summary>
        public static string InvariantTimestampFormat { get; } = "yyyy-MM-ddTHH:mm:ss.fffffff zzz";
    }
}
