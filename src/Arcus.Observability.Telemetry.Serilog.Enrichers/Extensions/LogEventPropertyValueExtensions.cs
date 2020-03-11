using System;
using System.Collections.Generic;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Serilog.Events
{
    public static class LogEventPropertyValueExtensions
    {
        /// <summary>
        ///     Provide a string representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static string GetAsRawString(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetValueOrDefault(propertyKey);
            return logEventPropertyValue?.ToDecentString();
        }

        /// <summary>
        ///     Provide a string representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> GetAsDictionary(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetValueOrDefault(propertyKey);

            if (logEventPropertyValue is DictionaryValue == false)
            {
                throw new NotSupportedException($"Value for '{propertyKey}' is not a dictionary");
            }

            return (logEventPropertyValue as DictionaryValue).Elements;
        }

        /// <summary>
        ///     Provide a enum representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static TEnum? GetAsEnum<TEnum>(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
            where TEnum : struct
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetValueOrDefault(propertyKey);
            var rawEnum = logEventPropertyValue?.ToDecentString();

            if (Enum.TryParse(typeof(TEnum), rawEnum, out var enumRepresentation))
            {
                return (TEnum)enumRepresentation;
            }

            return null;
        }

        /// <summary>
        ///     Provide a <c>DateTimeOffset</c> representation for a property key
        /// </summary>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static DateTimeOffset GetAsDateTimeOffset(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetAsRawString(propertyKey);
            var value = DateTimeOffset.Parse(logEventPropertyValue);
            return value;
        }

        /// <summary>
        ///     Provide a <c>TimeSpan</c> representation for a property key
        /// </summary>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static TimeSpan GetAsTimeSpan(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetAsRawString(propertyKey);
            var value = TimeSpan.Parse(logEventPropertyValue);
            return value;
        }

        /// <summary>
        ///     Provide a <c>bool</c> representation for a property key
        /// </summary>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static bool GetAsBool(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            Guard.NotNull(eventPropertyValues, nameof(eventPropertyValues));

            var logEventPropertyValue = eventPropertyValues.GetAsRawString(propertyKey);
            var value = bool.Parse(logEventPropertyValue);
            return value;
        }

        /// <summary>
        ///     Provide a decent string representation of the event property value
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="logEventPropertyValue">Event property value to provide a string representation</param>
        public static string ToDecentString(this LogEventPropertyValue logEventPropertyValue)
        {
            Guard.NotNull(logEventPropertyValue, nameof(logEventPropertyValue));

            var propertyValueAsString = logEventPropertyValue.ToString().Trim();

            if (propertyValueAsString.StartsWith("\""))
            {
                propertyValueAsString = propertyValueAsString.Remove(0, 1);
            }

            if (propertyValueAsString.EndsWith("\""))
            {
                propertyValueAsString = propertyValueAsString.Remove(propertyValueAsString.Length - 1);
            }

            return propertyValueAsString;
        }
    }
}
