using System;
using GuardNet;

namespace Arcus.Observability.Tests.Unit 
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