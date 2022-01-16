using System;
using System.Linq;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using static LanguageExt.ScheduleResult;
using Void = LanguageExt.Pipes.Void;

namespace LanguageExt
{
    /// <summary>
    /// Internal definition of a schedule.
    /// With inspiration from the Haskell Retry library https://hackage.haskell.org/package/retry
    /// A schedule is function from a given runtime, return value and current status into an schedule result.
    /// </summary>
    internal delegate IScheduleResult ScheduleInternal<in RT, A>(RT env, Fin<A> result, IScheduleStatus status);

    /// <summary>
    /// Transforms a schedule into another schedule.
    /// </summary>
    public delegate Schedule ScheduleTransformer(Schedule schedule);

    /// <summary>
    /// Transforms a schedule into another schedule.
    /// </summary>
    public delegate Schedule<A> ScheduleTransformer<A>(Schedule<A> schedule);

    /// <summary>
    /// Transforms a schedule into another schedule.
    /// </summary>
    public delegate Schedule<RT, A> ScheduleTransformer<RT, A>(Schedule<RT, A> schedule);

    /// <summary>
    /// Provides a mechanism for composing scheduled events.
    /// </summary>
    /// <remarks>
    /// Used heavily by `repeat`, `retry`, and `fold` with the `Aff` and `Eff` types.  Use the static methods to create parts
    /// of schedulers and then union them using `|` or intersect them using `&`.  Union will take the minimum of the two
    /// schedulers, intersect will take the maximum. 
    /// </remarks>
    /// <example>
    /// This example creates a schedule that repeats 5 times, with an exponential delay between each stage, starting
    /// at 10 milliseconds:
    /// 
    ///     var s = Schedule.Recurs(5) | Schedule.Exponential(10)
    /// 
    /// </example>
    /// <example>
    /// This example creates a schedule that repeats 5 times, with an exponential delay between each stage, starting
    /// at 10 milliseconds and with a maximum delay of 2000 milliseconds:
    /// 
    ///     var s = Schedule.Recurs(5) | Schedule.Exponential(10) | Schedule.Spaced(2000)
    /// </example>
    public sealed class Schedule : Schedule<Void>
    {
        private Schedule(Func<IScheduleStatus, IScheduleResult> internalSchedule) :
            base((_, status) => internalSchedule(status))
        {
        }

        public static Schedule Narrow<RT, A>(Schedule<RT, A> schedule)
            => new(status => schedule.InternalSchedule(default, default, status));

        public static Schedule Narrow<A>(Schedule<A> schedule)
            => new(status => schedule.InternalSchedule(default, default, status));

        public new Schedule Union(Schedule otherSchedule) => Narrow(Combine(otherSchedule, Math.Min));

        public new Schedule Intersect(Schedule otherSchedule) => Narrow(Combine(otherSchedule, Math.Max));

        public static Schedule operator |(Schedule x, Schedule y) => x.Union(y);
        public static Schedule operator |(Schedule x, ScheduleTransformer y) => y(x);
        public static Schedule operator |(ScheduleTransformer x, Schedule y) => x(y);

        public static Schedule operator &(Schedule x, Schedule y) => x.Intersect(y);
        public static Schedule operator &(Schedule x, ScheduleTransformer y) => y(x);
        public static Schedule operator &(ScheduleTransformer x, Schedule y) => x(y);

        /// <summary>
        /// A schedule that runs once.
        /// </summary>
        public static readonly Schedule Once = new(static _ => Complete);

        /// <summary>
        /// A schedule that recurs forever.
        /// </summary>
        public static readonly Schedule Forever = new(static _ => RunAgain);

        /// <summary>
        /// A schedule that only recurs the specified number of times.
        /// </summary>
        public static Schedule Recurs(int repetitions)
            => new(status => status.Iterations() <= repetitions ? RunAgain : Complete);

        /// <summary>
        /// A schedule that recurs continuously, each repetition spaced by the specified duration.
        /// </summary>
        public static Schedule Spaced(TimeSpan spacing) => new(_ => spacing.AsResult());

        /// <summary>
        /// A schedule that recurs continuously, each repetition spaced by the specified duration.
        /// </summary>
        public static Schedule Spaced(double spacingMilliseconds)
            => Spaced(spacingMilliseconds.AsTimeSpan());

        /// <summary>
        /// A schedule that recurs continuously using a linear backoff.
        /// </summary>
        public static Schedule Linear(TimeSpan spacing, double factor = 1)
            => Custom(spacing, spacing.Multiply(factor), static (_, sf, ad) => ad + sf);

        /// <summary>
        /// A schedule that recurs continuously using an exponential backoff.
        /// </summary>
        public static Schedule Linear(double spacingMilliseconds, double factor = 1)
            => Linear(spacingMilliseconds.AsTimeSpan(), factor);

        /// <summary>
        /// A schedule that recurs continuously using an exponential backoff.
        /// </summary>
        public static Schedule Exponential(TimeSpan spacing, double factor = 2)
            => Custom(spacing, factor, static (s, f, ad) => (ad == TimeSpan.Zero ? s : ad).Multiply(f));

        /// <summary>
        /// A schedule that recurs continuously using an exponential backoff.
        /// </summary>
        public static Schedule Exponential(double spacingMilliseconds, double factor = 2)
            => Exponential(spacingMilliseconds.AsTimeSpan(), factor);

        /// <summary>
        /// A schedule that recurs continuously using an fibonacci based backoff.
        /// </summary>
        public static Schedule Fibonacci(TimeSpan spacing)
            => new(status => status.Match(
                _ => spacing,
                x => x.AppliedDelay,
                x => x.AppliedDelay + x.PreviousDelay).AsResult());

        /// <summary>
        /// A schedule that recurs continuously using an fibonacci based backoff.
        /// </summary>
        public static Schedule Fibonacci(double spacingMilliseconds)
            => Fibonacci(spacingMilliseconds.AsTimeSpan());

        /// <summary>
        /// A schedule that recurs on a fixed interval. 
        /// </summary>
        /// <remarks>
        /// If the action run between updates takes longer than the interval, then the
        /// action will be run immediately, but re-runs will not "pile up".
        ///
        /// |-----interval-----|-----interval-----|-----interval-----|
        /// |---------action--------||action|-----|action|-----------|
        /// </remarks>
        /// <param name="spacing">spacing</param>
        /// <param name="currentTimeFn">optional current time function</param>
        public static Schedule Fixed(TimeSpan spacing, Func<DateTimeOffset> currentTimeFn = default)
        {
            var now = currentTimeFn ?? (() => DateTimeOffset.Now);
            var expectedNextRun = now() + spacing;
            return Custom(spacing, now,
                 (s, nowFn, _) =>
                {
                    var ct = nowFn();
                    var delay = ct > expectedNextRun ? TimeSpan.Zero : expectedNextRun - ct;
                    expectedNextRun = ct + (delay == TimeSpan.Zero ? s : delay);
                    return delay;
                });
        }

        /// <summary>
        /// A schedule that recurs on a fixed interval. 
        /// </summary>
        /// <remarks>
        /// If the action run between updates takes longer than the interval, then the
        /// action will be run immediately, but re-runs will not "pile up".
        ///
        /// |-----interval-----|-----interval-----|-----interval-----|
        /// |---------action--------||action|-----|action|-----------|
        /// </remarks>
        /// <param name="spacing">spacing</param>
        /// <param name="currentTimeFn">optional current time function</param>
        public static Schedule Fixed(double spacingMilliseconds, Func<DateTimeOffset> currentTimeFn = default)
            => Fixed(spacingMilliseconds.AsTimeSpan(), currentTimeFn);

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from status to optional time span</param>
        public new static Schedule Custom(Func<IScheduleStatus, IScheduleResult> func) => new(func);

        /// <summary>
        /// A custom schedule based on modifying the applied delay.
        /// </summary>
        /// <param name="seed">seed delay to apply on the first run</param>
        /// <param name="context">some context</param>
        /// <param name="modifyFn">some modify function</param>
        /// <typeparam name="C">context type</typeparam>
        public static Schedule Custom<C>(TimeSpan seed, C context, Func<TimeSpan, C, TimeSpan, TimeSpan> modifyFn)
            => new(status => status.ModifyAppliedDelay(seed, context, modifyFn));

        /// <summary>
        /// A custom schedule based on modifying the applied delay.
        /// </summary>
        /// <param name="seed">seed delay to apply on the first run</param>
        /// <param name="context">some context</param>
        /// <param name="modifyFn">some modify function</param>
        /// <typeparam name="C">context type</typeparam>
        public static Schedule Custom<C>(TimeSpan seed, C context, Func<TimeSpan, C, TimeSpan, int, TimeSpan> modifyFn)
            => new(status => status.ModifyAppliedDelay(
                seed, (status, context, modifyFn),
                static (s, ctx, ad) => ctx.modifyFn(s, ctx.context, ad, ctx.status.Iterations())));

        /// <summary>
        /// No op transformer.
        /// </summary>
        /// <returns>same schedule</returns>
        public new static ScheduleTransformer NoOp
            => schedule => schedule;

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public new static ScheduleTransformer Transform<Ctx>(
            Ctx ctx, Func<Ctx, IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => schedule => new(status => transform(ctx, status, schedule.InternalSchedule(default, default, status)));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public new static ScheduleTransformer Transform(Func<IScheduleResult, IScheduleResult> transform)
            => Transform(transform, static (fn, _, result) => fn(result));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public new static ScheduleTransformer TransformOnDelay<Ctx>(
            Ctx ctx, Func<Ctx, TimeSpan, TimeSpan> transform)
            => Transform((transform, ctx), static (t, _, result)
                => result.ModifyDelay(t,
                    static (t2, d) => t2.transform(t2.ctx, d)));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public new static ScheduleTransformer TransformOnDelay<Ctx>(
            Ctx ctx, Func<Ctx, double, double> transform)
            => Transform((transform, ctx), static (t, _, result)
                => result.ModifyDelay(t,
                    static (t2, d) => t2.transform(t2.ctx, d)));

        /// <summary>
        /// A schedule transformer that will enforce the first retry has no delay.
        /// </summary>
        public static ScheduleTransformer NoDelayOnFirstRetry()
            => static schedule => new Schedule(
                static status => status.Iterations() == 1 ? TimeSpan.Zero.AsResult() : RunAgain) | schedule;

        /// <summary>
        /// A schedule transformer that will limit the delay to no more than max delay.
        /// </summary>
        /// <param name="maxDelay">max delay to apply</param>
        public static ScheduleTransformer MaxDelay(TimeSpan maxDelay)
            => TransformOnDelay(maxDelay, static (md, delay) => delay > md ? md : delay);

        /// <summary>
        /// A schedule transformer that will limit the delay to no more than max delay.
        /// </summary>
        /// <remarks>Alias for spaced, union must be used</remarks>
        /// <param name="minDelayMilliseconds">max delay to apply in milliseconds</param>
        public static ScheduleTransformer MaxDelay(double minDelayMilliseconds)
            => MaxDelay(minDelayMilliseconds.AsTimeSpan());

        /// <summary>
        /// Limits the schedule to the max cumulative delay.
        /// </summary>
        /// <param name="maxDelay">max cumulative delay</param>
        public static ScheduleTransformer MaxCumulativeDelay(TimeSpan maxDelay)
            => Transform(maxDelay, static (md, status, result)
                => status is EffectReRunMoreThanOnce mo && mo.CumulativeDelay >= md
                    ? Complete
                    : result);

        /// <summary>
        /// Limits the schedule to the max cumulative delay.
        /// </summary>
        /// <param name="maxDelayMilliseconds">max cumulative delay in milliseconds</param>
        public static ScheduleTransformer MaxCumulativeDelay(double maxDelayMilliseconds)
            => MaxCumulativeDelay(maxDelayMilliseconds.AsTimeSpan());

        /// <summary>
        /// A schedule transformer that adds a random jitter to any returned delay.
        /// </summary>
        /// <param name="minRandom">min random milliseconds</param>
        /// <param name="maxRandom">max random milliseconds</param>
        /// <param name="seed">optional seed</param>
        public static ScheduleTransformer Jitter(double minRandom, double maxRandom, Option<int> seed = default)
            => TransformOnDelay((minRandom, maxRandom, seed),
                static (t, delay) => delay + SingletonRandom.Uniform(t.seed, t.minRandom, t.maxRandom));

        /// <summary>
        /// A schedule transformer that adds a random jitter to any returned delay.
        /// </summary>
        /// <param name="factor">jitter factor based on the returned delay</param>
        /// <param name="seed">optional seed</param>
        public static ScheduleTransformer Jitter(double factor = 0.5, Option<int> seed = default)
            => TransformOnDelay((factor, seed),
                static (t, delay) => delay + SingletonRandom.Uniform(t.seed, 0, delay * t.factor));
    }

    public class Schedule<A> : Schedule<Unit, A>
    {
        internal Schedule(Func<Fin<A>, IScheduleStatus, IScheduleResult> internalSchedule) :
            base((_, result, status) => internalSchedule(result, status))
        {
        }

        public static Schedule<A> Narrow<RT>(Schedule<RT, A> schedule)
            => new((result, status) => schedule.InternalSchedule(default, result, status));

        public new static Schedule<A> Widen(Schedule schedule)
            => new((_, status) => schedule.InternalSchedule(unit, default, status));

        public new static ScheduleTransformer<A> Widen(ScheduleTransformer schedule)
            => x => Widen(schedule(Schedule.Narrow(x)));

        public new Schedule<A> Union(Schedule<A> otherSchedule) => Narrow(Combine(otherSchedule, Math.Min));
        public new Schedule<A> Union(Schedule otherSchedule) => Union(Widen(otherSchedule));

        public new Schedule<A> Intersect(Schedule<A> otherSchedule) => Narrow(Combine(otherSchedule, Math.Max));
        public new Schedule<A> Intersect(Schedule otherSchedule) => Intersect(Widen(otherSchedule));

        public static Schedule<A> operator |(Schedule<A> x, Schedule<A> y) => x.Union(y);
        public static Schedule<A> operator |(Schedule<A> x, Schedule y) => x.Union(y);
        public static Schedule<A> operator |(Schedule<A> x, ScheduleTransformer<A> y) => y(x);
        public static Schedule<A> operator |(Schedule<A> x, ScheduleTransformer y) => Widen(y)(x);
        public static Schedule<A> operator |(Schedule x, Schedule<A> y) => y.Union(x);
        public static Schedule<A> operator |(ScheduleTransformer<A> x, Schedule<A> y) => x(y);
        public static Schedule<A> operator |(ScheduleTransformer x, Schedule<A> y) => Widen(x)(y);

        public static Schedule<A> operator &(Schedule<A> x, Schedule<A> y) => x.Intersect(y);
        public static Schedule<A> operator &(Schedule<A> x, Schedule y) => x.Intersect(y);
        public static Schedule<A> operator &(Schedule<A> x, ScheduleTransformer<A> y) => y(x);
        public static Schedule<A> operator &(Schedule<A> x, ScheduleTransformer y) => Widen(y)(x);
        public static Schedule<A> operator &(Schedule x, Schedule<A> y) => y.Intersect(x);
        public static Schedule<A> operator &(ScheduleTransformer<A> x, Schedule<A> y) => x(y);
        public static Schedule<A> operator &(ScheduleTransformer x, Schedule<A> y) => Widen(x)(y);

        /// <summary>
        /// A schedule that runs again when a provided error predicate is true.
        /// </summary>
        /// <param name="pred">error predicate</param>
        public new static Schedule<A> OnError(Func<Error, bool> pred)
            => new((result, _) => result.IsFail && pred(result.Error) ? RunAgain : Complete);

        /// <summary>
        /// A schedule that runs again on any of the provided errors.
        /// </summary>
        /// <param name="error">errors to retry</param>
        public new static Schedule<A> OnError(params Error[] errors)
            => OnError(errors.Contains);

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from fin of A and status to optional time span</param>
        public new static Schedule<A> Custom(Func<Fin<A>, IScheduleStatus, IScheduleResult> func) => new(func);

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from status to optional time span</param>
        public new static Schedule<A> Custom(Func<IScheduleStatus, IScheduleResult> func)
            => new((_, status) => func(status));

        /// <summary>
        /// No op transformer.
        /// </summary>
        /// <returns>same schedule</returns>
        public new static ScheduleTransformer<A> NoOp
            => schedule => schedule;

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public new static ScheduleTransformer<A> Transform(
            Func<Fin<A>, IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => schedule => new((value, status)
                => transform(value, status, schedule.InternalSchedule(default, value, status)));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public new static ScheduleTransformer<A> Transform(
            Func<IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => Transform((_, status, result) => transform(status, result));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public new static ScheduleTransformer<A> Transform(Func<IScheduleResult, IScheduleResult> transform)
            => Transform((_, result) => transform(result));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public new static ScheduleTransformer<A> TransformOnDelay(Func<TimeSpan, TimeSpan> transform)
            => Transform((_, result) => result.ModifyDelay(transform));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public new static ScheduleTransformer<A> TransformOnDelay(Func<double, double> transform)
            => Transform((_, result) => result.ModifyDelay(transform));
    }

    public class Schedule<RT, A>
    {
        internal readonly ScheduleInternal<RT, A> InternalSchedule;

        internal Schedule(ScheduleInternal<RT, A> internalSchedule)
            => InternalSchedule = internalSchedule;

        public static Schedule<RT, A> Widen(Schedule<A> schedule)
            => new((_, result, status) => schedule.InternalSchedule(unit, result, status));

        public static Schedule<RT, A> Widen(Schedule schedule)
            => new((_, _, status) => schedule.InternalSchedule(unit, default, status));

        public static ScheduleTransformer<RT, A> Widen(ScheduleTransformer<A> schedule)
            => x => Widen(schedule(Schedule<A>.Narrow(x)));

        public static ScheduleTransformer<RT, A> Widen(ScheduleTransformer schedule)
            => x => Widen(schedule(Schedule.Narrow(x)));

        protected Schedule<RT, A> Combine(
            Schedule<RT, A> otherSchedule,
            Func<double, double, double> aggregateFn)
            => new((env, result, status) =>
                InternalSchedule(env, result, status)
                    .Combine(otherSchedule.InternalSchedule(env, result, status), aggregateFn));

        public Schedule<RT, A> Union(Schedule<RT, A> otherSchedule) => Combine(otherSchedule, Math.Min);
        public Schedule<RT, A> Union(Schedule<A> otherSchedule) => Union(Widen(otherSchedule));
        public Schedule<RT, A> Union(Schedule otherSchedule) => Union(Widen(otherSchedule));

        public Schedule<RT, A> Intersect(Schedule<RT, A> otherSchedule) => Combine(otherSchedule, Math.Max);
        public Schedule<RT, A> Intersect(Schedule<A> otherSchedule) => Intersect(Widen(otherSchedule));
        public Schedule<RT, A> Intersect(Schedule otherSchedule) => Intersect(Widen(otherSchedule));

        public static Schedule<RT, A> operator |(Schedule<RT, A> x, Schedule<RT, A> y) => x.Union(y);
        public static Schedule<RT, A> operator |(Schedule<RT, A> x, Schedule<A> y) => x.Union(y);
        public static Schedule<RT, A> operator |(Schedule<RT, A> x, Schedule y) => x.Union(y);
        public static Schedule<RT, A> operator |(Schedule<RT, A> x, ScheduleTransformer<RT, A> y) => y(x);
        public static Schedule<RT, A> operator |(Schedule<RT, A> x, ScheduleTransformer<A> y) => Widen(y)(x);
        public static Schedule<RT, A> operator |(Schedule<RT, A> x, ScheduleTransformer y) => Widen(y)(x);
        public static Schedule<RT, A> operator |(Schedule<A> x, Schedule<RT, A> y) => y.Union(x);
        public static Schedule<RT, A> operator |(Schedule x, Schedule<RT, A> y) => y.Union(x);
        public static Schedule<RT, A> operator |(ScheduleTransformer<RT, A> x, Schedule<RT, A> y) => x(y);
        public static Schedule<RT, A> operator |(ScheduleTransformer<A> x, Schedule<RT, A> y) => Widen(x)(y);
        public static Schedule<RT, A> operator |(ScheduleTransformer x, Schedule<RT, A> y) => Widen(x)(y);

        public static Schedule<RT, A> operator &(Schedule<RT, A> x, Schedule<RT, A> y) => x.Intersect(y);
        public static Schedule<RT, A> operator &(Schedule<RT, A> x, Schedule<A> y) => x.Intersect(y);
        public static Schedule<RT, A> operator &(Schedule<RT, A> x, Schedule y) => x.Intersect(y);
        public static Schedule<RT, A> operator &(Schedule<RT, A> x, ScheduleTransformer<RT, A> y) => y(x);
        public static Schedule<RT, A> operator &(Schedule<RT, A> x, ScheduleTransformer<A> y) => Widen(y)(x);
        public static Schedule<RT, A> operator &(Schedule<RT, A> x, ScheduleTransformer y) => Widen(y)(x);
        public static Schedule<RT, A> operator &(Schedule<A> x, Schedule<RT, A> y) => y.Intersect(x);
        public static Schedule<RT, A> operator &(Schedule x, Schedule<RT, A> y) => y.Intersect(x);
        public static Schedule<RT, A> operator &(ScheduleTransformer<RT, A> x, Schedule<RT, A> y) => x(y);
        public static Schedule<RT, A> operator &(ScheduleTransformer<A> x, Schedule<RT, A> y) => Widen(x)(y);
        public static Schedule<RT, A> operator &(ScheduleTransformer x, Schedule<RT, A> y) => Widen(x)(y);

        /// <summary>
        /// A schedule that runs again when a provided error predicate is true.
        /// </summary>
        /// <param name="pred">error predicate</param>
        public static Schedule<RT, A> OnError(Func<Error, bool> pred)
            => new((_, result, _) => result.IsFail && pred(result.Error) ? RunAgain : Complete);

        /// <summary>
        /// A schedule that runs again on any of the provided errors.
        /// </summary>
        /// <param name="error">error to retry</param>
        public static Schedule<RT, A> OnError(params Error[] errors)
            => OnError(errors.Contains);

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from runtime of RT and fin of A and status to optional time span</param>
        public static Schedule<RT, A> Custom(Func<RT, Fin<A>, IScheduleStatus, IScheduleResult> func)
            => new((env, result, status) => func(env, result, status));

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from fin of A and status to optional time span</param>
        public static Schedule<RT, A> Custom(Func<Fin<A>, IScheduleStatus, IScheduleResult> func)
            => new((_, result, status) => func(result, status));

        /// <summary>
        /// A custom schedule.
        /// </summary>
        /// <param name="func">function from status to optional time span</param>
        public static Schedule<RT, A> Custom(Func<IScheduleStatus, IScheduleResult> func)
            => new((_, _, status) => func(status));

        /// <summary>
        /// No op transformer.
        /// </summary>
        /// <returns>same schedule</returns>
        public static ScheduleTransformer<RT, A> NoOp
            => schedule => schedule;

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public static ScheduleTransformer<RT, A> Transform(
            Func<RT, Fin<A>, IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => schedule => new((env, value, status)
                => transform(env, value, status, schedule.InternalSchedule(env, value, status)));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public static ScheduleTransformer<RT, A> Transform(
            Func<Fin<A>, IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => Transform((_, value, status, result) => transform(value, status, result));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public static ScheduleTransformer<RT, A> Transform(
            Func<IScheduleStatus, IScheduleResult, IScheduleResult> transform)
            => Transform((_, status, result) => transform(status, result));

        /// <summary>
        /// Returns a custom schedule transformer.
        /// </summary>
        public static ScheduleTransformer<RT, A> Transform(Func<IScheduleResult, IScheduleResult> transform)
            => Transform((_, result) => transform(result));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public static ScheduleTransformer<RT, A> TransformOnDelay(Func<TimeSpan, TimeSpan> transform)
            => Transform((_, result) => result.ModifyDelay(transform));

        /// <summary>
        /// Returns a custom schedule transformer that operates on delayed results only.
        /// </summary>
        public static ScheduleTransformer<RT, A> TransformOnDelay(Func<double, double> transform)
            => Transform((_, result) => result.ModifyDelay(transform));
    }
}
