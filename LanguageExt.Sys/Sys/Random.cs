﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LanguageExt.ClassInstances;
using LanguageExt.ClassInstances.Pred;
using LanguageExt.Sys.Traits;
using LanguageExt.TypeClasses;
using static LanguageExt.Prelude;

namespace LanguageExt.Sys;

/// <summary>
/// Random IO
/// </summary>
/// <typeparam name="RT">runtime</typeparam>
public static class Random<RT> where RT : struct, HasRandom<RT>
{
    /// <summary>
    /// Create a new random context and run the provided Eff in that context
    /// </summary>
    /// <param name="ma">Operation to run in the next context</param>
    /// <param name="seed">optional seed for the random instance</param>
    /// <typeparam name="A">Bound value type</typeparam>
    /// <returns>An effect that captures the operation running in context</returns>
    public static Eff<RT, A> localRandom<A>(Eff<RT, A> ma, int? seed = default) => 
        EffMaybe<RT, A>(rt => ma.Run(rt.LocalRandom(seed)));
    
    [Pure]
    static (T min, T max) ensureBounds<T, TNum>(T min, T max) where TNum : struct, Ord<T> 
    {
        min = default(TNum).Compare(min, max) == -1 ? min : max;
        max = default(TNum).Compare(max, min) == 1 ? max : min;
        return ( min, max);
    }

    /// <summary>
    /// Returns a non-negative int
    /// </summary>
    /// <param name="min">minimum int to return</param>
    /// <param name="max">maximum int to return</param>
    /// <returns>int</returns>
    [Pure]
    public static Eff<RT, int> nextInt(int? min = default, int? max = default) =>
        default(RT).RandomEff.Map(io => io.NextInt(min, max));
    
    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers
    /// </summary>
    /// <param name="length">number of bytes to fill</param>
    [Pure]
    public static Eff<RT, byte[]> nextByteArray(long length) =>
        default(RT).RandomEff.Map(io => io.NextByteArray(length));
    
    /// <summary>
    /// Returns a non-negative double
    /// </summary>
    /// <param name="min">minimum double to return</param>
    /// <param name="max">maximum double to return</param>
    /// <returns>double</returns>
    [Pure]
    public static Eff<RT, double> nextDouble(double? min = default, double? max = default)
    {
        var (mi, me) = ensureBounds<double, TDouble>(min ?? 0.0, max ?? 1.0);
        return default(RT).RandomEff.Map(static io => io.NextDouble())
            .Map(n =>
            {
                var result = n * (me - mi) + mi;
                return result < me ? result : MathExt.NextAfter(me, double.NegativeInfinity);
            });
    }

    /// <summary>
    /// Returns a non-negative long
    /// </summary>
    /// <param name="min">minimum long to return</param>
    /// <param name="max">maximum long to return</param>
    /// <returns>long</returns>
    [Pure]
    public static Eff<RT, long> nextLong(long? min = default, long? max = default)
    {
        var (mi, me) = ensureBounds<long, TLong>(min ?? 0L, max ?? long.MaxValue);
        return default(RT).RandomEff.Map(io => io.NextLong() % (me - mi) + mi);
    }

    /// <summary>
    /// Returns a non-negative float
    /// </summary>
    /// <param name="min">minimum float to return</param>
    /// <param name="max">maximum float to return</param>
    /// <returns>float</returns>
    [Pure]
    public static Eff<RT, float> nextFloat(float? min = default, float? max = default)
    {
        var (mi, me) = ensureBounds<float, TFloat>(min ?? 0.0f, max ?? float.MaxValue);
        return default(RT).RandomEff.Map(static io => io.NextFloat())
            .Map(n =>
            {
                var result = n * (me - mi) + me;
                return result < me ? result : MathExt.NextAfter(me, float.NegativeInfinity);
            });
    }

    /// <summary>
    /// Returns a non-negative guid
    /// </summary>
    /// <returns>guid</returns>
    [Pure]
    public static Eff<RT, Guid> nextGuid() =>
        default(RT).RandomEff.Map(static io => io.NextGuid());

    /// <summary>
    /// Returns a random character
    /// </summary>
    /// <param name="min">min char</param>
    /// <param name="max">max char</param>
    /// <returns>char</returns>
    public static Eff<RT, char> nextChar(char? min = default, char? max = default) =>
        nextInt(min ?? 32, max ?? 126).Map(static i => (char)i);

    /// <summary>
    /// Random duration
    /// </summary>
    /// <param name="min">min duration</param>
    /// <param name="max">max duration</param>
    /// <returns>random duration</returns>
    [Pure]
    public static Eff<RT, Duration> nextDuration(Duration? min = default, Duration? max = default) =>
        nextDouble(min, max).Map(static d => (Duration)d);

    /// <summary>
    /// Random time span
    /// </summary>
    /// <param name="min">min duration</param>
    /// <param name="max">max duration</param>
    /// <returns>random time span</returns>
    [Pure]
    public static Eff<RT, TimeSpan> nextTimespan(Duration? min = default, Duration? max = default) =>
        nextDuration(min, max).Map(static d => (TimeSpan)d);

    /// <summary>
    /// Random date time with offset
    /// </summary>
    /// <param name="min">min duration</param>
    /// <param name="max">max duration</param>
    /// <returns>random date time offset</returns>
    [Pure]
    public static Eff<RT, DateTimeOffset> nextDateTimeOffset(
        DateTimeOffset? min = default,
        DateTimeOffset? max = default) =>
        nextLong(
                (min ?? DateTimeOffset.MinValue).ToUnixTimeMilliseconds(),
                (max ?? DateTimeOffset.MaxValue).ToUnixTimeMilliseconds())
            .Map(static ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

    /// <summary>
    /// Random date time
    /// </summary>
    /// <param name="min">min duration</param>
    /// <param name="max">max duration</param>
    /// <returns>random date time</returns>
    [Pure]
    public static Eff<RT, DateTime> nextDateTime(DateTime? min = default, DateTime? max = default) =>
        nextDateTimeOffset(min, max).Map(static dto => dto.UtcDateTime);

    /// <summary>
    /// Random length enumerable T
    /// </summary>
    /// <param name="effect">effect T</param>
    /// <param name="min">min length</param>
    /// <param name="max">max length</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>enumerable of T</returns>
    public static Eff<RT, IEnumerable<T>> nextRange<T>(
        Eff<RT, T> effect,
        int? min = default,
        int? max = default) =>
        nextInt(Math.Abs(min ?? 0), max ?? 1000)
            .Bind(length => Range(0, length).Select(_ => effect).Sequence());

    /// <summary>
    /// Generates a random string
    /// </summary>
    /// <param name="min">min length</param>
    /// <param name="max">max length</param>
    /// <param name="minChar">min char</param>
    /// <param name="maxChar">max char</param>
    /// <returns>string</returns>
    public static Eff<RT, string> nextString(
        int? min = default,
        int? max = default,
        char? minChar = default,
        char? maxChar = default) =>
        nextRange(nextChar(minChar, maxChar), min, max ?? 100)
            .Map(x => new string(x.ToArray()));

    /// <summary>
    /// Returns a random element with an equal probability
    /// </summary>
    /// <param name="head">head of the list</param>
    /// <param name="tail">rest of the list or empty</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>random T</returns>
    public static Eff<RT, T> uniform<T>(T head, params T[] tail) =>
        uniform<T>(List(head,tail));
    
    /// <summary>
    /// Returns a random element with an equal probability
    /// </summary>
    /// <param name="elements">list of elements</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>random T</returns>
    public static Eff<RT, T> uniform<T>(Lst<NonEmpty, T> elements) =>
        nextInt(0, elements.Count - 1).Map(i => elements[i]);

    /// <summary>
    /// Returns a random element with a weighted probability
    /// </summary>
    /// <param name="head">head of the list</param>
    /// <param name="tail">rest of the list or empty</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>random T</returns>
    public static Eff<RT, T> weighted<T>((int weight, T element) head, params(int weight, T element)[] tail) =>
        weighted<T>(List(head,tail));
    
    /// <summary>
    /// Returns a random element with a weighted probability
    /// </summary>
    /// <param name="elements">weighted list of elements</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>random T</returns>
    public static Eff<RT, T> weighted<T>(Lst<NonEmpty, (int weight, T element)> elements)
    {
        var el = elements.Map(x => x with { weight = Math.Abs(x.weight) });
        return nextInt(0, el.Sum(x => x.weight))
            .Map(rand =>
                {
                    var sum = 0;
                    return el.First(x =>
                            {
                                sum += x.weight;
                                return rand < sum;
                            }).element;
                });
    }

    /// <summary>
    /// Randomly shuffles the specified list
    /// </summary>
    /// <param name="head">head of the list</param>
    /// <param name="tail">rest of the list or empty</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>shuffled list</returns>
    [Pure]
    public static Eff<RT, Lst<NonEmpty, T>> shuffle<T>(T head, params T[] tail) =>
        shuffle(List<NonEmpty, T>(head, tail));
    
    /// <summary>
    /// Randomly shuffles the specified list
    /// </summary>
    /// <param name="elements">non empty list to shuffle</param>
    /// <typeparam name="T">some T</typeparam>
    /// <returns>shuffled list</returns>
    [Pure]
    public static Eff<RT, Lst<NonEmpty, T>> shuffle<T>(Lst<NonEmpty, T> elements)
    {
        var mutable = elements.ToList();
        return default(RT).RandomEff.Map(io =>
            {
                for (var n = elements.Count - 1; n > 0; n--)
                {
                    var k = io.NextInt(max: n + 1);
                    (mutable[k], mutable[n]) = (mutable[n], mutable[k]);
                }

                return List<NonEmpty, T>(mutable[0], mutable.Tail().ToArray());
            });
    }

    /// <summary>
    /// Returns a random enum from the enumeration
    /// </summary>
    /// <typeparam name="E">enum</typeparam>
    /// <returns>enum</returns>
    public static Eff<RT, E> nextEnum<E>() where E : struct, Enum
    {
        var values = Enum.GetValues(typeof(E)).OfType<E>().ToArr();
        return uniform<E>(List(values[0], values.Tail().ToArray()));
    }

    /// <summary>
    /// Returns a random boolean
    /// </summary>
    /// <returns>enum</returns>
    public static Eff<RT, bool> nextBool() =>
        uniform<bool>(List(true, false));
}
