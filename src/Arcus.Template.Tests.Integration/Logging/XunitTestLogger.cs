using System;
using GuardNet;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Arcus.Template.Tests.Integration.Logging
{
    public class XunitTestLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutput;

        public XunitTestLogger(ITestOutputHelper testOutput)
        {
            Guard.NotNull(testOutput, nameof(testOutput));

            _testOutput = testOutput;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            _testOutput.WriteLine($"{DateTimeOffset.UtcNow:s} {logLevel} > {message}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
