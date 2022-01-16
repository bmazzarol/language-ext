using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LanguageExt
{
    internal static class TimeSpanExtensions
    {
        /// <summary>
        /// Converts a double to a timespan.
        /// </summary>
        /// <param name="milliseconds">milliseconds</param>
        /// <returns>timespan</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan AsTimeSpan(this double milliseconds)
            => TimeSpan.FromMilliseconds(milliseconds);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TimeSpan Multiply(this TimeSpan a, double b)
            => (a.TotalMilliseconds * b).AsTimeSpan();
    }
}
