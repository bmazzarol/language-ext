using System;
using System.Diagnostics.Contracts;
using static LanguageExt.Prelude;

namespace LanguageExt.Next
{
    /// <summary>
    /// Defines a schedule result, either complete or a delay to apply.
    /// </summary>
    public readonly struct ScheduleResult
    {
        private readonly Option<PositiveDuration> _delayToApply;
        public bool IsComplete => _delayToApply.IsNone;
        public PositiveDuration DelayUnsafe => _delayToApply.IfNone(PositiveDuration.Zero);
        private ScheduleResult(Option<PositiveDuration> delayToApply) => _delayToApply = delayToApply;

        /// <summary>
        /// Completed schedule.
        /// </summary>
        [Pure]
        public static ScheduleResult Complete => new(None);

        /// <summary>
        /// Re run effect without delay.
        /// </summary>
        [Pure]
        public static ScheduleResult RunAgain => new(PositiveDuration.Zero);

        /// <summary>
        /// Delay to apply and run the schedule again.
        /// </summary>
        /// <param name="delayToApply">delay to apply</param>
        [Pure]
        public static ScheduleResult DelayAndRunAgain(PositiveDuration delayToApply) => new(delayToApply);

        [Pure]
        public static explicit operator PositiveDuration(ScheduleResult result)
            => result.IsComplete
                ? throw new InvalidCastException("Result is complete.")
                : (PositiveDuration)result._delayToApply;

        [Pure]
        public static bool operator ==(ScheduleResult a, ScheduleResult b)
            => a._delayToApply == b._delayToApply;

        [Pure]
        public static bool operator !=(ScheduleResult a, ScheduleResult b)
            => !(a == b);

        [Pure]
        public static ScheduleResult operator +(ScheduleResult result, PositiveDuration delay)
            => result.IsComplete ? result : DelayAndRunAgain(result.DelayUnsafe + delay);

        /// <summary>
        /// Union of 2 schedule results.
        /// </summary>
        [Pure]
        public static ScheduleResult operator |(ScheduleResult a, ScheduleResult b) =>
            a.IsComplete && b.IsComplete ? Complete :
            a.IsComplete && !b.IsComplete ? b :
            !a.IsComplete && b.IsComplete ? a :
            DelayAndRunAgain(Math.Min(a.DelayUnsafe, b.DelayUnsafe));

        /// <summary>
        /// Intersection of 2 schedule results.
        /// </summary>
        [Pure]
        public static ScheduleResult operator &(ScheduleResult a, ScheduleResult b) =>
            a.IsComplete && b.IsComplete ? Complete :
            a.IsComplete && !b.IsComplete ? Complete :
            !a.IsComplete && b.IsComplete ? Complete :
            DelayAndRunAgain(Math.Max(a.DelayUnsafe, b.DelayUnsafe));

        public bool Equals(ScheduleResult other) => _delayToApply.Equals(other._delayToApply);

        public override bool Equals(object obj) => obj is ScheduleResult other && Equals(other);

        public override int GetHashCode() => _delayToApply.GetHashCode();

        public override string ToString()
            => IsComplete
                ? $"{nameof(ScheduleResult)}.{nameof(Complete)}"
                : _delayToApply
                    .Where(delay => delay != PositiveDuration.Zero)
                    .Map(delay => $"{nameof(ScheduleResult)}.{nameof(DelayAndRunAgain)}({delay})")
                    .IfNone(() => $"{nameof(ScheduleResult)}.{nameof(RunAgain)}");
    }

    /// <summary>
    /// Transforms a schedule result.
    /// </summary>
    public delegate ScheduleResult ScheduleResultTransformer(ScheduleContext context, ScheduleResult currentResult);
}
