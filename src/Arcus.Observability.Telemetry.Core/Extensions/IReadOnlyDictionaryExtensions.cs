using GuardNet;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
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
