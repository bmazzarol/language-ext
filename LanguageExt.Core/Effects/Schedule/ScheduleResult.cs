using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LanguageExt
{
    /// <summary>
    /// Defines an ADT for the result of a schedule iteration.
    /// </summary>
    public interface IScheduleResult
    {
    }

    /// <summary>
    /// Indicates that the effect should be completed and the schedule is complete.
    /// </summary>
    public sealed class CompleteEffect : IScheduleResult
    {
        public static readonly CompleteEffect Default = new();

        private CompleteEffect()
        {
        }
    }

    /// <summary>
    /// Indicates that the effect should be run again.
    /// </summary>
    public sealed class ReRunEffect : IScheduleResult
    {
        public static readonly ReRunEffect Default = new();

        private ReRunEffect()
        {
        }
    }

    /// <summary>
    /// Indicates that the effect should be run again after a provided delay.
    /// </summary>
    public readonly struct ReRunEffectAfterDelay : IScheduleResult
    {
        public readonly TimeSpan Delay;

        public ReRunEffectAfterDelay(TimeSpan delay) => Delay = delay.Duration();

        public static implicit operator double(ReRunEffectAfterDelay x) => x.Delay.TotalMilliseconds;

        public override string ToString() => $"{nameof(ReRunEffectAfterDelay)}({nameof(Delay)}: {Delay})";
    }

    public static class ScheduleResult
    {
        /// <summary>
        /// Complete the schedule.
        /// </summary>
        public static IScheduleResult Complete => CompleteEffect.Default;

        /// <summary>
        /// Runs the effect again.
        /// </summary>
        public static IScheduleResult RunAgain => ReRunEffect.Default;

        /// <summary>
        /// Runs the effect again after the delay.
        /// </summary>
        /// <param name="delay">absolute delay to apply before re-running the effect</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult DelayAndRunAgain(TimeSpan delay) => new ReRunEffectAfterDelay(delay);

        /// <summary>
        /// Timespan to schedule result.
        /// </summary>
        /// <param name="delay">absolute delay to apply before re-running the effect</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult AsResult(this TimeSpan delay) => DelayAndRunAgain(delay);

        /// <summary>
        /// Runs the effect again after the delay.
        /// </summary>
        /// <param name="delayMilliseconds">absolute delay to apply before re-running the effect in milliseconds</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult DelayAndRunAgain(double delayMilliseconds)
            => DelayAndRunAgain(delayMilliseconds.AsTimeSpan());

        /// <summary>
        /// Converts a schedule result to a timespan.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ToTimeSpan(this IScheduleResult result)
            => result switch
            {
                ReRunEffectAfterDelay x => x.Delay,
                _ => default
            };

        /// <summary>
        /// Modifies the delay of the schedule result.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult ModifyDelay<Ctx>(
            this IScheduleResult result, Ctx ctx, Func<Ctx, TimeSpan, TimeSpan> modifyFn)
            => result switch
            {
                ReRunEffectAfterDelay x => DelayAndRunAgain(modifyFn(ctx, x.Delay)),
                _ => result
            };

        /// <summary>
        /// Modifies the delay of the schedule result.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult ModifyDelay(this IScheduleResult result, Func<TimeSpan, TimeSpan> modifyFn)
            => result.ModifyDelay(modifyFn, static (fn, delay) => fn(delay));

        /// <summary>
        /// Modifies the delay of the schedule result.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult ModifyDelay<Ctx>(
            this IScheduleResult result, Ctx ctx, Func<Ctx, double, double> modifyFn)
            => result.ModifyDelay((ctx, modifyFn),
                static (t, delay) => t.modifyFn(t.ctx, delay.TotalMilliseconds).AsTimeSpan());

        /// <summary>
        /// Modifies the delay of the schedule result.
        /// </summary>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult ModifyDelay(this IScheduleResult result, Func<double, double> modifyFn)
            => result.ModifyDelay(modifyFn, static (fn, delay) => fn(delay.TotalMilliseconds).AsTimeSpan());

        /// <summary>
        /// Combines 2 schedule results into one, with a provided aggregate function for delays.
        /// </summary>
        /// <param name="a">result a</param>
        /// <param name="b">result b</param>
        /// <param name="aggregateFn">some aggregation function for delay values</param>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult Combine(
            this IScheduleResult a,
            IScheduleResult b,
            Func<double, double, double> aggregateFn)
#pragma warning disable CS8509
            => (a, b) switch
#pragma warning restore CS8509
            {
                (CompleteEffect, CompleteEffect) => Complete,
                (CompleteEffect, ReRunEffect) => Complete,
                (CompleteEffect, ReRunEffectAfterDelay) => Complete,
                (ReRunEffect, CompleteEffect) => Complete,
                (ReRunEffect, ReRunEffect) => RunAgain,
                (ReRunEffect, ReRunEffectAfterDelay xb) => DelayAndRunAgain(xb.Delay),
                (ReRunEffectAfterDelay, CompleteEffect) => Complete,
                (ReRunEffectAfterDelay xa, ReRunEffect) => DelayAndRunAgain(xa.Delay),
                (ReRunEffectAfterDelay xa, ReRunEffectAfterDelay xb) => DelayAndRunAgain(aggregateFn(xa, xb))
            };
    }
}
