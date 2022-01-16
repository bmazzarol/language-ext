using System;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    /// <summary>
    /// Singleton source of randomness.
    /// </summary>
    public static class SingletonRandom
    {
        private static readonly Func<Option<int>, Random> Provider =
            memo((Option<int> seed) => seed.Map(x => new Random(x)).IfNone(new Random()));
        
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0,
        /// and less than 1.0.
        /// </summary>
        public static double NextDouble(Option<int> seed)
        {
            lock (Provider) return Provider(seed).NextDouble();
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to <paramref name="a"/>,
        /// and less than <paramref name="b"/>.
        /// </summary>
        public static double Uniform(Option<int> seed, double a, double b)
        {
            if (a.Equals(b)) return a;
            return a + (b - a) * NextDouble(seed);
        }
    }
}
