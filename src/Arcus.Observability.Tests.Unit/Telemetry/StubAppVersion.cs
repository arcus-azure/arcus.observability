using Arcus.Observability.Telemetry.Core;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    /// <summary>
    /// Stubbed implementation of the <see cref="IAppVersion"/>.
    /// </summary>
    public class StubAppVersion : IAppVersion
    {
        private readonly string _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubAppVersion"/> class.
        /// </summary>
        /// <param name="version">The current stubbed application version.</param>
        public StubAppVersion(string version)
        {
            _version = version;
        }

        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public string GetVersion()
        {
            return _version;
        }
    }
}