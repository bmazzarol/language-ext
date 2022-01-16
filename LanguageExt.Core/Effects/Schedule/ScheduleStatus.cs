using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LanguageExt
{
    /// <summary>
    /// Defines a ADT for all the possible states a running schedule can be in.
    /// </summary>
    public interface IScheduleStatus
    {
    }

    /// <summary>
    /// First time the effect has been run.
    /// </summary>
    public class EffectRunOnce : IScheduleStatus
    {
        public static readonly EffectRunOnce Default = new();

        private EffectRunOnce()
        {
        }
    }

    /// <summary>
    /// First time the effect has be re-run.
    /// </summary>
    public readonly struct EffectReRunOnce : IScheduleStatus
    {
        /// <summary>
        /// Applied delay. Can be zero.
        /// </summary>
        public readonly TimeSpan AppliedDelay;

        public EffectReRunOnce(TimeSpan appliedDelay) => AppliedDelay = appliedDelay.Duration();

        public override string ToString() => $"{nameof(EffectReRunOnce)}({nameof(AppliedDelay)}: {AppliedDelay})";
    }

    /// <summary>
    /// Subsequent times the effect has been re-run.
    /// </summary>
    public readonly struct EffectReRunMoreThanOnce : IScheduleStatus
    {
        /// <summary>
        /// Current number of iterations, some integer greater than 2.
        /// </summary>
        public readonly int Iteration;

        /// <summary>
        /// Total cumulative delay. Can be zero.
        /// </summary>
        public readonly TimeSpan CumulativeDelay;

        /// <summary>
        /// Current applied delay. Can be zero.
        /// </summary>
        public readonly TimeSpan AppliedDelay;

        /// <summary>
        /// Previous delay. Can be zero.
        /// </summary>
        public readonly TimeSpan PreviousDelay;

        public EffectReRunMoreThanOnce(int iteration, TimeSpan cumulativeDelay, TimeSpan appliedDelay,
            TimeSpan previousDelay)
        {
            Iteration = iteration;
            CumulativeDelay = cumulativeDelay;
            AppliedDelay = appliedDelay;
            PreviousDelay = previousDelay;
        }

        public override string ToString()
            => $"{nameof(EffectReRunMoreThanOnce)}({nameof(Iteration)}: {Iteration}, {nameof(CumulativeDelay)}: {CumulativeDelay}, {nameof(AppliedDelay)}: {AppliedDelay}, {nameof(PreviousDelay)}: {PreviousDelay})";
    }

    public static class ScheduleStatus
    {
        /// <summary>
        /// The effect has been run once.
        /// </summary>
        [Pure]
        public static IScheduleStatus RunOnce => EffectRunOnce.Default;

        /// <summary>
        /// The effect has been re-run once.
        /// </summary>
        /// <param name="appliedDelay">applied delay, can be zero</param>
        [Pure]
        public static IScheduleStatus ReRunOnce(TimeSpan appliedDelay) => new EffectReRunOnce(appliedDelay);

        /// <summary>
        /// The effect has been re-run once.
        /// </summary>
        /// <param name="appliedDelayMs">applied delay in milliseconds</param>
        [Pure]
        public static IScheduleStatus ReRunOnce(double appliedDelayMs)
            => ReRunOnce(TimeSpan.FromMilliseconds(appliedDelayMs));

        /// <summary>
        /// The effect has been re-run more than once.
        /// </summary>
        /// <param name="iteration">current iteration</param>
        /// <param name="cumulativeDelay">cumulative delay applied, can be zero</param>
        /// <param name="appliedDelay">current applied delay, can be zero</param>
        /// <param name="previousDelay">previous delay, can be zero</param>
        /// <returns></returns>
        [Pure]
        public static IScheduleStatus ReRunMoreThanOnce(
            int iteration, TimeSpan cumulativeDelay, TimeSpan appliedDelay, TimeSpan previousDelay)
            => new EffectReRunMoreThanOnce(iteration, cumulativeDelay, appliedDelay, previousDelay);

        /// <summary>
        /// The effect has been re-run more than once.
        /// </summary>
        /// <param name="iteration">current iteration</param>
        /// <param name="cumulativeDelayMs">cumulative delay applied in milliseconds</param>
        /// <param name="appliedDelayMs">current applied delay in milliseconds</param>
        /// <param name="previousDelayMs">previous delay in milliseconds</param>
        /// <returns></returns>
        [Pure]
        public static IScheduleStatus ReRunMoreThanOnce(
            int iteration, double cumulativeDelayMs, double appliedDelayMs, double previousDelayMs)
            => ReRunMoreThanOnce(
                iteration,
                TimeSpan.FromMilliseconds(cumulativeDelayMs),
                TimeSpan.FromMilliseconds(appliedDelayMs),
                TimeSpan.FromMilliseconds(previousDelayMs));

        /// <summary>
        /// Matches the schedule status states.
        /// </summary>
        /// <param name="status">schedule status</param>
        /// <param name="once">matches the effect run state</param>
        /// <param name="reRunOnce">matches the effect re run state</param>
        /// <param name="moreThanOnce">matches the effect re run more than once state</param>
        /// <typeparam name="A">some type A</typeparam>
        /// <returns>returns some value of type A</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static A Match<A>(
            this IScheduleStatus status,
            Func<EffectRunOnce, A> once,
            Func<EffectReRunOnce, A> reRunOnce,
            Func<EffectReRunMoreThanOnce, A> reRunMoreThanOnce)
            => status switch
            {
                EffectRunOnce x => once(x),
                EffectReRunOnce x => reRunOnce(x),
                EffectReRunMoreThanOnce x => reRunMoreThanOnce(x),
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };

        /// <summary>
        /// Returns the number of times the effect has run.
        /// </summary>
        /// <param name="status">schedule status</param>
        /// <returns>number of iterations</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Iterations(this IScheduleStatus status)
            => status.Match(
                _ => 1,
                _ => 2,
                x => x.Iteration);

        /// <summary>
        /// Modifies the current applied delay and returns a new delay result.
        /// </summary>
        /// <param name="status">current status</param>
        /// <param name="seed">seed to apply on first iteration</param>
        /// <param name="ctx">some outer context to provide to the modify function, avoid capture</param>
        /// <param name="modifyFn">function to modify the applied delay</param>
        /// <returns>delay result</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IScheduleResult ModifyAppliedDelay<A>(
            this IScheduleStatus status,
            TimeSpan seed,
            A ctx,
            Func<TimeSpan, A, TimeSpan, TimeSpan> modifyFn)
            => status.Match(
                    _ => seed,
                    x => modifyFn(seed, ctx, x.AppliedDelay),
                    x => modifyFn(seed, ctx, x.AppliedDelay))
                .AsResult();

        /// <summary>
        /// Increments the schedule status.
        /// </summary>
        /// <param name="status">current status</param>
        /// <param name="delay">current delay</param>
        /// <returns>new schedule status</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IScheduleStatus RegisterIteration(this IScheduleStatus status, TimeSpan delay)
            => status.Match(
                _ => ReRunOnce(delay),
                x => ReRunMoreThanOnce(3, x.AppliedDelay + delay, delay, x.AppliedDelay),
                x => ReRunMoreThanOnce(x.Iteration + 1, x.CumulativeDelay + delay, delay, x.AppliedDelay));

        /// <summary>
        /// Increments the schedule status.
        /// </summary>
        /// <param name="status">current status</param>
        /// <param name="result">current schedule result</param>
        /// <returns>new schedule status</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IScheduleStatus RegisterIteration(this IScheduleStatus status, IScheduleResult result)
            => status.RegisterIteration(result.ToTimeSpan());
    }
}
