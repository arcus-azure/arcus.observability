using System;
using GuardNet;

namespace Arcus.Observability.Tests.Unit.Telemetry 
{
    /// <summary>
    /// Represents a temporary environment variable that gets removed when the model gets disposed.
    /// </summary>
    public class TemporaryEnvironmentVariable : IDisposable
    {
        private readonly string _name;

        private TemporaryEnvironmentVariable(string name)
        {
            Guard.NotNullOrWhitespace(name, nameof(name));

            _name = name;
        }

        /// <summary>
        /// Creates a <see cref="TemporaryEnvironmentVariable"/> instance that creates and removes the environment variable
        /// with <paramref name="name"/> and <paramref name="value"/> during its lifetime.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <param name="value">The value of the environment variable.</param>
        public static TemporaryEnvironmentVariable Create(string name, string value)
        {
            Guard.NotNullOrWhitespace(name, nameof(name));
            Guard.NotNullOrWhitespace(value, nameof(value));

            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            return new TemporaryEnvironmentVariable(name);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_name, value: null, target: EnvironmentVariableTarget.Process);
        }
    }
}