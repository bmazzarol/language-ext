using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static LanguageExt.Prelude;

namespace LanguageExt.Next
{
    /// <summary>
    /// Provides the current execution context for a schedule.
    /// </summary>
    public readonly struct ScheduleContext : IEquatable<ScheduleContext>
    {
        public static readonly ScheduleContext Initial = new(None, 0, 0);

        public readonly Option<ScheduleResult> LastResult;

        public PositiveDuration AppliedDelay => LastResult;

        public readonly PositiveDuration CumulativeAppliedDelay;

        public readonly int Iteration;
        public bool HasNotStarted => Iteration == 0;
        public bool HasStarted => !HasNotStarted;

        private ScheduleContext(
            Option<ScheduleResult> lastResult,
            PositiveDuration cumulativeAppliedDelay,
            int iteration)
        {
            LastResult = lastResult;
            Iteration = iteration;
            CumulativeAppliedDelay = cumulativeAppliedDelay;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScheduleContext New(
            int iterations,
            PositiveDuration cumulativeDelay,
            Option<ScheduleResult> lastResult)
            => new(lastResult, cumulativeDelay, iterations);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScheduleContext Next(
            Option<ScheduleResult> scheduleResult, Option<ScheduleContext> lastContext)
            => lastContext.Map(x
                => new ScheduleContext(
                    scheduleResult,
                    x.AppliedDelay + (PositiveDuration)scheduleResult,
                    x.Iteration + 1)).IfNone(Initial);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ScheduleContext a, ScheduleContext b)
            => a.AppliedDelay == b.AppliedDelay
               && a.CumulativeAppliedDelay == b.CumulativeAppliedDelay
               && a.Iteration == b.Iteration;


        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ScheduleContext a, ScheduleContext b) => !(a == b);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => HasNotStarted
                ? $"{nameof(ScheduleContext)}.{nameof(Initial)}"
                : $"{nameof(ScheduleContext)}(" +
                  $"{nameof(Iteration)}:{Iteration}, " +
                  $"{nameof(LastResult)}:{LastResult}, " +
                  $"{nameof(CumulativeAppliedDelay)}:{CumulativeAppliedDelay})";

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ScheduleContext other) =>
            LastResult.Equals(other.LastResult)
            && Equals(CumulativeAppliedDelay, other.CumulativeAppliedDelay)
            && Iteration == other.Iteration;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) =>
            obj is ScheduleContext other && Equals(other);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LastResult.GetHashCode();
                hashCode = (hashCode * 397) ^
                           CumulativeAppliedDelay.GetHashCode();
                hashCode = (hashCode * 397) ^ Iteration;
                return hashCode;
            }
        }
    }
}
