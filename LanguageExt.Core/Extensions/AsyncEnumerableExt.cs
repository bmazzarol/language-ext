using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LanguageExt.Core.Extensions
{
    /// <summary>
    /// Some extension methods for working with async enumerable.
    /// </summary>
    public static class AsyncEnumerableExt
    {
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
            => (await list.FoldWhile(Seq.empty<A>(), (seq, a) => seq.Add(a), _ => true)).AsEnumerable();
    }
}
