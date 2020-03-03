using System;
using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Tests.Unit
{
    public class TestLogger : ILogger
    {
        /// <summary>
        ///     Last log message that was written
        /// </summary>
        public string WrittenMessage { get; private set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            WrittenMessage = state.ToString();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}