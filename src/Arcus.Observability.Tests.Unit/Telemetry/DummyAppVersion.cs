using System;
using Arcus.Observability.Telemetry.Core;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    /// <summary>
    /// Dummy <see cref="IAppVersion"/> implementation.
    /// </summary>
    public class DummyAppVersion : IAppVersion
    {
        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public string GetVersion()
        {
            throw new NotImplementedException();
        }
    }
}
