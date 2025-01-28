using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    /// <summary>
    /// Extension class on the xUnit <see cref="Assert"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static class AssertX
    {
        /// <summary>
        /// Verifies that a <paramref name="collection"/> has at least one element that matches the given <paramref name="assertion"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The collection to set through.</param>
        /// <param name="assertion">The element assertion to find at least a single element that matches.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="collection"/> or <paramref name="assertion"/> is <c>null</c>.</exception>
        public static void Any<T>(IEnumerable<T> collection, Action<T> assertion)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection), "Requires collection of elements to find a single element that matches the assertion");
            }
            if (assertion is null)
            {
                throw new ArgumentNullException(nameof(assertion), "Requires an element assertion to verify if an single element in the collection matches");
            }

            Assert.NotEmpty(collection);

            T[] array = collection.ToArray();
            var stack = new Stack<Tuple<int, object, Exception>>();

            for (var index = 0; index < array.Length; index++)
            {
                T item = array[index];
                try
                {
                    assertion(item);
                    return;
                }
                catch (Exception exception)
                {
                    stack.Push(new Tuple<int, object, Exception>(index, item, exception));
                }
            }

            if (stack.Count > 0)
            {
                throw new ContainsException(array.Length, stack.ToArray());
            }
        }
    }
}
