using System;

namespace Arcus.Observability.Tests.Integration.Fixture
{
    /// <summary>
    /// Represents a custom <see cref="Exception"/> to determine if the exception is tracked as expected.
    /// </summary>
    public class TestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestException" /> class.
        /// </summary>
        public TestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the exception.</param>
        public TestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        /// <summary>
        /// Gets the custom test property of this exception to determine if the property is considered while tracking the exception.
        /// </summary>
        public string SpyProperty { get; set; }
    }
}
