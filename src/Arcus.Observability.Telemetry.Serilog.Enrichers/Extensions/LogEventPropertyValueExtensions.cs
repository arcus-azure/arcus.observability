using System;
using System.Collections.Generic;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Serilog.Events
{
    /// <summary>
    /// Extensions for event properties
    /// </summary>
    public static class LogEventPropertyValueExtensions
    {
        private static readonly StructureValue EmptyStructureValue = new StructureValue([]);

        /// <summary>
        ///     Provide a string representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static string GetAsRawString(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            var logEventPropertyValue = eventPropertyValues.GetValueOrDefault(key: propertyKey);
            return logEventPropertyValue?.ToDecentString();
        }

        /// <summary>
        ///     Provides a <c>double</c> representation for a <paramref name="propertyKey"/>.
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the <c>string</c> with quotes.</remarks>
        /// <param name="eventPropertyValues">The Event property values to provide as a <c>string</c> representation.</param>
        /// <param name="propertyKey">The key of the property to return.</param>
        public static double GetAsDouble(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            LogEventPropertyValue logEventPropertyValue = eventPropertyValues.GetValueOrDefault(key: propertyKey);
            string rawDouble = logEventPropertyValue?.ToDecentString();

            if (rawDouble != null)
            {
                return Double.Parse(rawDouble, CultureInfo.InvariantCulture);
            }

            return Double.NaN;
        }

        /// <summary>
        ///     Provide a string representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        /// <param name="propertyDictionaryValues">Found information for the given property key</param>
        public static bool TryGetAsDictionary(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey, out IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> propertyDictionaryValues)
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            var propertyValue = eventPropertyValues.GetValueOrDefault(key: propertyKey);
            if (propertyValue == null || propertyValue is DictionaryValue == false)
            {
                propertyDictionaryValues = null;
                return false;
            }

            propertyDictionaryValues = (propertyValue as DictionaryValue).Elements;
            return true;
        }

        /// <summary>
        ///     Provide a string representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> GetAsDictionary(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            var valueFound = TryGetAsDictionary(eventPropertyValues, propertyKey, out var propertyDictionaryValues);
            if (valueFound == false)
            {
                throw new NotSupportedException($"Value for '{propertyKey}' is not a dictionary");
            }

            return propertyDictionaryValues;
        }

        /// <summary>
        ///     Provide a enum representation for a property key
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="eventPropertyValues"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        /// <exception cref="FormatException">
        ///     Thrown when the Serilog property value cannot be parsed correctly because the <typeparamref name="TEnum"/> does not present an enumeration type.
        /// </exception>
        /// <returns>
        ///     The parsed representation of the Serilog property value as the provided <typeparamref name="TEnum"/> enumeration type; <c>null</c> otherwise.
        /// </returns>
        public static TEnum? GetAsEnum<TEnum>(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
            where TEnum : struct
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyKey);

            LogEventPropertyValue logEventPropertyValue = eventPropertyValues.GetValueOrDefault(key: propertyKey);
            if (logEventPropertyValue is null)
            {
                return null;
            }

            string rawEnum = logEventPropertyValue.ToDecentString();

            try
            {
                if (Enum.TryParse(rawEnum, out TEnum enumRepresentation))
                {
                    return enumRepresentation;
                }
            }
            catch (ArgumentException exception)
            {
                throw new FormatException("Cannot correctly parse the incoming Serilog property value to an enumeration", exception);
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
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            var logEventPropertyValue = eventPropertyValues.GetAsRawString(propertyKey);
            var value = DateTimeOffset.Parse(logEventPropertyValue, CultureInfo.InvariantCulture);
            return value;
        }

        /// <summary>
        ///     Provide a <c>TimeSpan</c> representation for a property key
        /// </summary>
        /// <param name="eventPropertyValues">Event property value to provide a string representation</param>
        /// <param name="propertyKey">Key of the property to return</param>
        public static TimeSpan GetAsTimeSpan(this IReadOnlyDictionary<string, LogEventPropertyValue> eventPropertyValues, string propertyKey)
        {
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

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
            ArgumentNullException.ThrowIfNull(eventPropertyValues);

            var logEventPropertyValue = eventPropertyValues.GetAsRawString(propertyKey);
            var value = bool.Parse(logEventPropertyValue);
            return value;
        }

        /// <summary>
        /// Provide a <see cref="StructureValue"/> representation for a property value associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="properties">The series of log properties, containing a <see cref="StructureValue"/> with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key that is associated with the property value that's an <see cref="StructureValue"/>.</param>
        /// <returns>
        ///     An <see cref="StructureValue"/> that contains its own name-value pair collection of the log event; an empty collection instead.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        public static StructureValue GetAsStructureValue(this IReadOnlyDictionary<string, LogEventPropertyValue> properties, string propertyKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyKey);

            if (properties is null)
            {
                return EmptyStructureValue;
            }

            if (properties.TryGetValue(propertyKey, out LogEventPropertyValue propertyValue)
                && propertyValue is StructureValue value)
            {
                return value;
            }

            return EmptyStructureValue;
        }

        /// <summary>
        ///     Provide a decent string representation of the event property value
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="logEventPropertyValue">Event property value to provide a string representation</param>
        public static string ToDecentString(this LogEventPropertyValue logEventPropertyValue)
        {
            ArgumentNullException.ThrowIfNull(logEventPropertyValue);

            if (logEventPropertyValue is ScalarValue scalar)
            {
                var result = scalar.Value?.ToString();
                return result;
            }
            else
            {
                string propertyValueAsString = logEventPropertyValue.ToString().Trim();
                if (propertyValueAsString.StartsWith('\"'))
                {
                    propertyValueAsString = propertyValueAsString.Remove(0, 1);
                }

                if (propertyValueAsString.EndsWith('\"'))
                {
                    propertyValueAsString = propertyValueAsString.Remove(propertyValueAsString.Length - 1);
                }

                return propertyValueAsString;
            }
        }
    }
}