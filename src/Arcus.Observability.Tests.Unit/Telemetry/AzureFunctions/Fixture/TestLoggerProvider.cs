using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Tests.Unit.Telemetry.AzureFunctions.Fixture
{
    /// <summary>
    /// <see cref="ILoggerProvider"/> implementation to verify if the provider gets removed during the <see cref="ILoggerBuilderExtensionsTests"/>.
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider
    {
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The instance of <see cref="T:Microsoft.Extensions.Logging.ILogger" /> that was created.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }
    }
}
