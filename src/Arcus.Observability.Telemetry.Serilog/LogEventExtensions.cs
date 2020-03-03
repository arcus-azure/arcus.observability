using System.Collections.Generic;
using System.Linq;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Serilog.Events
{
    /// <summary>
    /// Extensions on the <see cref="LogEvent"/> model to work more easily with the type.
    /// </summary>
    public static class LogEventExtensions
    {
        /// <summary>
        /// Determines if the given <paramref name="logEvent"/> contains a property with the specified <paramref name="name"/> and simple <paramref name="value"/>.
        /// </summary>
        /// <param name="logEvent">The event on which the property should be present.</param>
        /// <param name="name">The unique name of the property that should be present in the <paramref name="logEvent"/>.</param>
        /// <param name="value">The simple value of the property that should be present for the given <paramref name="name"/>.</param>
        /// <returns>
        ///     [true] if the specified <paramref name="logEvent"/> contains the log property with the specified <paramref name="name"/> and the simple <paramref name="value"/>; [false] otherwise.
        /// </returns>
        public static bool ContainsProperty(this LogEvent logEvent, string name, string value)
        {
            Guard.NotNull(logEvent, nameof(logEvent));

            KeyValuePair<string, LogEventPropertyValue> actual = 
                logEvent.Properties.SingleOrDefault(prop => prop.Key == name);

            if (actual.Key is null || actual.Value is null)
            {
                return false;
            }

            string actualValue = actual.ToString().Trim('\"');
            return value == actualValue;
        }
    }
}
