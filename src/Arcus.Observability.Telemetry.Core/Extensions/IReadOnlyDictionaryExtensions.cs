using GuardNet;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Extensions on the <see cref="IReadOnlyDictionary{TKey,TValue}"/> to get more easily access to the the items' values.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IReadOnlyDictionaryExtensions
    {
        /// <summary>
        ///     Provides value or default for a given dictionary entry
        /// </summary>
        /// <param name="dictionary">Dictionary containing data</param>
        /// <param name="propertyKey">Key of the dictionary entry to return</param>
        public static TValue GetValueOrDefault<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string propertyKey)
        {
            Guard.NotNull(dictionary, nameof(dictionary));

            if (dictionary.TryGetValue(propertyKey, out TValue foundValue))
            {
                return foundValue;
            }

            return default;
        }
    }
}
