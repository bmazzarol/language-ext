using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LanguageExt
{
    /// <summary>
    /// Some extension methods for working with async enumerable.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Projects the values in the async enumerable using a map function into a new async enumerable (Select in LINQ).
        /// </summary>
        /// <typeparam name="T">Async enumerable item type</typeparam>
        /// <typeparam name="R">Return async enumerable item type</typeparam>
        /// <param name="list">Async enumerable to map</param>
        /// <param name="map">Map function</param>
        /// <returns>Mapped async enumerable</returns>
        [Pure]
        public static async IAsyncEnumerable<R> Map<T, R>(this IAsyncEnumerable<T> list, Func<T, R> map)
        {
            await foreach (var t in list) yield return map(t);
        }

        /// <summary>
        /// Removes items from the async enumerable that do not match the given predicate (Where in LINQ)
        /// </summary>
        /// <typeparam name="T">Async enumerable item type</typeparam>
        /// <param name="list">Async enumerable to filter</param>
        /// <param name="predicate">Predicate function</param>
        /// <returns>Filtered async enumerable</returns>
        [Pure]
        public static async IAsyncEnumerable<T> Filter<T>(this IAsyncEnumerable<T> list, Func<T, bool> predicate)
        {
            await foreach (var t in list)
                if (predicate(t))
                    yield return t;
        }

        /// <summary>
        /// Applies a function 'folder' to each element of the collection whilst the predicate function 
        /// returns True for the item being processed, threading an aggregate state through the 
        /// computation. The fold function takes the state argument, and applies the function 'folder' 
        /// to it and the first element of the list. Then, it feeds this result into the function 'folder' 
        /// along with the second element, and so on. It returns the final result.
        /// </summary>
        /// <typeparam name="S">State type</typeparam>
        /// <typeparam name="T">Enumerable item type</typeparam>
        /// <param name="list">Async Enumerable to fold</param>
        /// <param name="state">Initial state</param>
        /// <param name="folder">Fold function</param>
        /// <param name="preditem">Predicate function</param>
        /// <returns>Aggregate value</returns>
        [Pure]
        public static async Task<S> FoldWhile<S, T>(
            this IAsyncEnumerable<T> list,
            S state,
            Func<S, T, S> folder,
            Func<T, bool> preditem)
        {
            await foreach (var t in list)
            {
                if (!preditem(t)) return state;
                state = folder(state, t);
            }

            return state;
        }

        /// <summary>
        /// Converts an async enumerable to a task of enumerable.
        /// </summary>
        /// <param name="list">async enumerable</param>
        /// <returns>task of enumerable A</returns>
        public static async Task<IEnumerable<A>> ToEnumerable<A>(
            this IAsyncEnumerable<A> list)
            => (await list.FoldWhile(Seq.empty<A>(), static (seq, a) => seq.Add(a), static _ => true)).AsEnumerable();
    }
}
