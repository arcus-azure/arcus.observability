// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a set of extension on the <see cref="IDictionary{TKey,TValue}"/> to more easily interact with multiple dictionaries.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static class IDictionaryExtensions
    {
        /// <summary>
        /// Adds the items from the <paramref name="additionalItems"/> to this current <paramref name="items"/>.
        /// </summary>
        /// <param name="items">The current set of items.</param>
        /// <param name="additionalItems">The additional set of items.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="items"/> or <paramref name="additionalItems"/> is <c>null</c>.</exception>
        internal static void AddRange(this IDictionary<string, string> items, IDictionary<string, string> additionalItems)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(additionalItems);

            foreach (KeyValuePair<string, string> property in additionalItems)
            {
                items.Add(property);
            }
        }
    }
}
