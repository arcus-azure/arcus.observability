using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GuardNet;

// ReSharper disable once CheckNamespace
namespace Serilog.Events
{
    /// <summary>
    /// Extensions on the Serilog <see cref="StructureValue.Properties"/> to have more easily access to typed property values.
    /// </summary>
    internal static class LogEventPropertyExtensions
    {
        private static readonly IDictionary<string, string> EmptyContext = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets a raw string representation for a property value in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="properties">The properties containing an property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property value is associated with.</param>
        /// <returns>
        ///     An raw string representation of the property value when the property was found; <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        internal static string GetAsRawString(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property value as a raw string representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property key to retrieve a Serilog event property value as a raw string representation");
            
            LogEventProperty propertyValue = properties.FirstOrDefault(prop => prop.Name == propertyKey);
            if (propertyValue?.Value is null 
                || (propertyValue.Value is ScalarValue scalarValue 
                    && scalarValue.Value is null))
            {
                return null;
            }
            
            return propertyValue.Value.ToDecentString();
        }
        
        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representation for a property value in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="properties">The properties containing a property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property value is associated with.</param>
        /// <returns>
        ///     An <see cref="TimeSpan"/> representation of the property value when the property was found; <see cref="TimeSpan.Zero"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the property value cannot be parsed as a <see cref="TimeSpan"/> due of its format.</exception>
        /// <exception cref="OverflowException">Thrown when the property value is lesser or greater than the supported <see cref="TimeSpan"/> duration.</exception>
        internal static TimeSpan GetAsTimeSpan(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as a TimeSpan representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property key to retrieve a Serilog event property value as a TimeSpan representation");
            
            string propertyValue = properties.GetAsRawString(propertyKey);
            if (propertyValue is null)
            {
                return TimeSpan.Zero;
            }
            
            TimeSpan value = TimeSpan.Parse(propertyValue);
            return value;
        }
        
        /// <summary>
        /// Gets a <see cref="DateTimeOffset"/> representation of a property value in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="properties">The properties containing a property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property value is associated with.</param>
        /// <returns>
        ///     An <see cref="DateTimeOffset"/> representation with <see cref="CultureInfo.InvariantCulture"/> of the property value when the property was found;
        ///     <c>default(<see cref="DateTimeOffset"/>)</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        /// <exception cref="FormatException">Throw when the property value cannot be parsed as a <see cref="DateTimeOffset"/> due to its format.</exception>
        internal static DateTimeOffset GetAsDateTimeOffset(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as a DateTimeOffset representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property key to retrieve a Serilog event property value as a DateTimeOffset representation");
            
            string propertyValue = properties.GetAsRawString(propertyKey);
            if (propertyValue is null)
            {
                return default(DateTimeOffset);
            }
            
            DateTimeOffset value = DateTimeOffset.Parse(propertyValue, CultureInfo.InvariantCulture);
            return value;
        }
        
        /// <summary>
        /// Gets a <see cref="IDictionary{TKey,TValue}"/> representation of a property value in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the string with quotes</remarks>
        /// <param name="properties">The properties containing the property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property is associated with.</param>
        /// <returns>
        ///     An <see cref="IDictionary{TKey,TValue}"/> representation with raw string representations of name-value pairs when the property was found;
        ///     an empty dictionary otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        internal static IDictionary<string, string> GetAsDictionary(
            this IReadOnlyList<LogEventProperty> properties, 
            string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as a IDictionary<string, string> representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property key to retrieve a Serilog event property value as a IDictionary<string, string> represenation");
            
            LogEventProperty property = properties.FirstOrDefault(prop => prop.Name == propertyKey);
            if (property?.Value is DictionaryValue dictionary)
            {
                return dictionary.Elements.ToDictionary(
                    item => item.Key.ToDecentString(),
                    item => item.Value.ToDecentString());
            }

            return EmptyContext;
        }
        
        /// <summary>
        /// Gets a <see cref="Double"/> representation of a property value in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <remarks>The built-in <c>ToString</c> wraps the <c>string</c> with quotes.</remarks>
        /// <param name="properties">The properties containing the property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property is associated with.</param>
        /// <returns>
        ///     An <see cref="Double"/> representation with <see cref="CultureInfo.InvariantCulture"/> when the property was found; <see cref="Double.NaN"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the property value cannot be parsed as a <see cref="Double"/> due to its format.</exception>
        /// <exception cref="OverflowException">Thrown when the property value is lesser or greater than the supported <see cref="Double"/> range.</exception>
        internal static double GetAsDouble(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as a Double representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property key to retrieve a Serilog event property as a Double representation");
            
            LogEventProperty logEventPropertyValue = properties.FirstOrDefault(prop => prop.Name == propertyKey);
            if (logEventPropertyValue is null)
            {
                return double.NaN;
            }

            if (logEventPropertyValue.Value is ScalarValue scalarValue 
                && scalarValue.Value is double value)
            {
                return value;
            }

            return double.NaN;
        }
        
        /// <summary>
        /// Gets a <see cref="Boolean"/> representation of a property in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <param name="properties">The properties containing the property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property is associated with.</param>
        /// <returns>
        ///     An <see cref="Boolean"/> representation when the property was found; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        /// <exception cref="FormatException">Thrown when the property value cannot be parsed as a <see cref="Boolean"/> due to its format.</exception>
        internal static bool GetAsBool(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as a Boolean representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property to retrieve a Serilog event property as a Boolean representation");
            
            string propertyValue = properties.GetAsRawString(propertyKey);
            if (propertyValue is null)
            {
                return false;
            }
            
            bool value = bool.Parse(propertyValue);
            return value;
        }

        /// <summary>
        /// Gets a <typeparamref name="TValue"/> representation of a property in the <paramref name="properties"/> associated with the <paramref name="propertyKey"/>.
        /// </summary>
        /// <typeparam name="TValue">The custom type to cast to.</typeparam>
        /// <param name="properties">The properties containing the property value associated with the <paramref name="propertyKey"/>.</param>
        /// <param name="propertyKey">The key the property is associated with.</param>
        /// <returns>
        ///     An <typeparamref name="TValue"/> representation when the property was found; <c>default</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="propertyKey"/> is blank.</exception>
        internal static TValue GetAsObject<TValue>(this IReadOnlyList<LogEventProperty> properties, string propertyKey)
        {
            Guard.NotNull(properties, nameof(properties), "Requires a series of event properties to retrieve a Serilog event property as an enumeration representation");
            Guard.NotNullOrWhitespace(propertyKey, nameof(propertyKey), "Requires a non-blank property to retrieve a Serilog event property as an enumeration representation");

            LogEventProperty property = properties.FirstOrDefault(prop => prop.Name == propertyKey);
            if (property != null 
                && property.Value is ScalarValue scalarValue 
                && scalarValue.Value is TValue value)
            {
                return value;
            }

            return default(TValue);
        }
    }
}
