using System;
using System.Diagnostics.Contracts;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt.Next
{
    /// <summary>
    /// Building blocks for working with schedules.
    /// </summary>
    public static partial class Schedule
    {
        private static readonly Func<DateTime> LiveNowFn = () => DateTime.Now;

        /// <summary>
        /// Identity or noop schedule result transformer.
        /// </summary>
        [Pure]
        public static ScheduleResultTransformer Identity => (_, result) => result;

        /// <summary>
        /// Schedule that runs once.
        /// </summary>
        [Pure]
        public static Sched Once => Constant(ScheduleResult.Complete);

        /// <summary>
        /// Schedule that runs forever.
        /// </summary>
        [Pure]
        public static Sched Forever => Constant(ScheduleResult.RunAgain);

        [Pure]
        private static Eff<RT, DateTime> Now<RT>() where RT : struct, HasCancel<RT>, HasTime<RT> =>
            from env in runtime<RT>()
            from time in env.TimeEff
            select time.Now;

        /// <summary>
        /// Schedule that runs for a given duration.
        /// </summary>
        /// <param name="max">max duration to run the schedule for</param>
        /// <param name="currentTime">current time function</param>
        [Pure]
        public static Sched UpTo(PositiveDuration max, Func<DateTime> currentTime = null)
        {
            var ctFn = currentTime ?? LiveNowFn;
            return New((startTime: ctFn(), currentTime: ctFn, max), static (t, _) =>
                (PositiveDuration)(t.startTime - t.currentTime()) > t.max
                    ? ScheduleResult.Complete
                    : ScheduleResult.RunAgain);
        }

        /// <summary>
        /// Schedule that runs for a given duration.
        /// </summary>
        /// <param name="max">max duration to run the schedule for</param>
        [Pure]
        public static Sched<RT, A> UpTo<RT, A>(PositiveDuration max)
            where RT : struct, HasCancel<RT>, HasTime<RT>
            => New<(Eff<RT, DateTime> startTime, PositiveDuration max), RT, A>(
                (startTime: Now<RT>().Memo(), max), static (t, _, _) =>
                    from env in runtime<RT>()
                    let startTime = t.startTime.Run(env).IfFail(_ => DateTime.Now)
                    from currentTime in Now<RT>()
                    select (PositiveDuration)(startTime - currentTime) > t.max
                        ? ScheduleResult.Complete
                        : ScheduleResult.RunAgain);

        /// <summary>
        /// Schedule that recurs for the specified fixed durations.
        /// </summary>
        /// <param name="durations">durations to apply</param>
        [Pure]
        public static Sched FromDurations(params PositiveDuration[] durations)
            => New(durations.ToSeq(), static (waits, _)
                => (Result: !waits.IsEmpty ? ScheduleResult.DelayAndRunAgain(waits.Head) : ScheduleResult.Complete,
                    State: waits.Tail));

        /// <summary>
        /// Schedule that recurs the specified number of times.
        /// </summary>
        /// <param name="times">number of times</param>
        [Pure]
        public static Sched Recurs(int times)
            => New(times, static (times, ctx)
                => ctx.Iteration < times ? ScheduleResult.RunAgain : ScheduleResult.Complete);

        /// <summary>
        /// Schedule that recurs continuously with the given spacing.
        /// </summary>
        /// <param name="space">space</param>
        [Pure]
        public static Sched Spaced(PositiveDuration space) => Constant(ScheduleResult.DelayAndRunAgain(space));

        /// <summary>
        /// Schedule that will retry five times and pause 200ms between each call.
        ///
        /// Simple strategy for dealing with transient failures that are not susceptible to
        /// overload from fast retries. 
        /// </summary>
        /// <param name="retry">number of retry attempts, default 5</param>
        /// <param name="space">constant space between each retry, default 200 ms</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        [Pure]
        public static Sched Spaced(int retry = 5, Option<PositiveDuration> space = default, bool fastFirst = false)
            => Recurs(retry) & Spaced(space.IfNone(() => 200)) & (fastFirst ? NoDelayOnFirstRetry : Identity);

        /// <summary>
        /// Schedule that recurs continuously using a linear backoff.
        /// </summary>
        /// <param name="seed">seed</param>
        /// <param name="factor">optional factor to apply, default 1</param>
        [Pure]
        public static Sched Linear(PositiveDuration seed, double factor = 1)
            => New(seed * factor, seed, static (extraDelay, _, ctx)
                => ctx.AppliedDelay + extraDelay);

        /// <summary>
        /// Schedule that will retry five times and pause based on a linear backoff starting at 100 ms.
        ///
        /// Simple strategy for dealing with transient failures that are susceptible to
        /// overload from fast retries. In this scenario, we want to give the effect some time
        /// to stabilize before trying again.
        /// </summary>
        /// <param name="retry">number of retry attempts, default 5</param>
        /// <param name="space">initial space, default 100 ms</param>
        /// <param name="factor">linear factor to apply, default 1</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Sched Linear(
            int retry = 5, Option<PositiveDuration> space = default, double factor = 1, bool fastFirst = false)
            => Recurs(retry) & Linear(space.IfNone(() => 100), factor)
                             & (fastFirst ? NoDelayOnFirstRetry : Identity);

        /// <summary>
        /// Schedule that recurs continuously using a exponential backoff.
        /// </summary>
        /// <param name="seed">seed</param>
        /// <param name="factor">optional factor to apply, default 2</param>
        [Pure]
        public static Sched Exponential(PositiveDuration seed, double factor = 2)
            => New(factor, seed, static (factor, _, ctx)
                => ctx.AppliedDelay * factor);

        /// <summary>
        /// Schedule that will retry five times and pauses based on a exponential backoff starting at 100 ms.
        ///
        /// Because of the exponential nature, this is best used with a low starting delay or in out-of-band
        /// communication, such as a service worker polling for information from a remote endpoint.
        /// Due to the potential for rapidly increasing times, care should be taken if an exponential retry
        /// is used in the code path for servicing a user request.
        ///
        /// If the overall amount of time that an exponential-backoff retry policy could take is a concern,
        /// consider combining it with a max cumulative delay,
        ///
        ///     Schedule.Exponential() | Schedule.MaxCumulativeDelay(45*seconds)
        /// 
        /// </summary>
        /// <param name="retry">maximum number of retries to use, default 5</param>
        /// <param name="space">initial space, default 100 ms</param>
        /// <param name="factor">linear factor to apply, default 2</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Sched Exponential(
            int retry = 5, Option<PositiveDuration> space = default, double factor = 2, bool fastFirst = false)
            => Recurs(retry) & Exponential(space.IfNone(() => 100), factor)
                             & (fastFirst ? NoDelayOnFirstRetry : Identity);

        /// <summary>
        /// Schedule that recurs continuously using an fibonacci based backoff.
        /// </summary>
        /// <param name="seed">seed</param>
        [Pure]
        public static Sched Fibonacci(PositiveDuration seed)
            => New(seed, static (seed, ctx)
                => (Result: ScheduleResult.DelayAndRunAgain(ctx.AppliedDelay + seed), State: ctx.AppliedDelay));

        [Pure]
        private static PositiveDuration SecondsToIntervalStart(
            DateTime startTime, DateTime currentTime, PositiveDuration interval)
            => interval - (currentTime - startTime).TotalMilliseconds % interval;

        /// <summary>
        /// Schedule that recurs on a fixed interval.
        ///
        /// If the action run between updates takes longer than the interval, then the
        /// action will be run immediately, but re-runs will not "pile up".
        ///
        /// <pre>
        /// |-----interval-----|-----interval-----|-----interval-----|
        /// |---------action--------||action|-----|action|-----------|
        /// </pre>
        /// </summary>
        /// <param name="interval">schedule interval</param>
        /// <param name="currentTime">current time function</param>
        public static Sched Fixed(PositiveDuration interval, Func<DateTime> currentTime = null)
        {
            var ctFn = currentTime ?? LiveNowFn;
            var startTime = ctFn();
            return New((
                    startTime,
                    currentTimeFn: ctFn,
                    interval,
                    lastRunTime: startTime),
                static (t, _) =>
                {
                    var now = t.currentTimeFn();
                    var runningBehind = now > t.lastRunTime + (TimeSpan)t.interval;
                    var boundary = t.interval == PositiveDuration.Zero
                        ? t.interval
                        : SecondsToIntervalStart(t.startTime, now, t.interval);
                    var sleepTime = boundary == PositiveDuration.Zero ? t.interval : boundary;
                    var nextRun = runningBehind ? now : now + (TimeSpan)sleepTime;
                    return (
                        Result: runningBehind
                            ? ScheduleResult.RunAgain
                            : ScheduleResult.DelayAndRunAgain(sleepTime),
                        State: t with { lastRunTime = nextRun });
                });
        }

        /// <summary>
        /// Schedule that recurs on a fixed interval.
        ///
        /// If the action run between updates takes longer than the interval, then the
        /// action will be run immediately, but re-runs will not "pile up".
        ///
        /// <pre>
        /// |-----interval-----|-----interval-----|-----interval-----|
        /// |---------action--------||action|-----|action|-----------|
        /// </pre>
        /// </summary>
        /// <param name="interval">schedule interval</param>
        public static Sched<RT, A> Fixed<RT, A>(PositiveDuration interval) where RT : struct, HasCancel<RT>, HasTime<RT>
        {
            var startTime = Now<RT>().Memo();
            return New<(
                Eff<RT, DateTime> startTime,
                PositiveDuration interval,
                Eff<RT, DateTime> lastRunTime), RT, A>(
                (startTime, interval, lastRunTime: startTime),
                static (t, _, _) =>
                    from env in runtime<RT>()
                    let startTime = t.startTime.Run(env).IfFail(_ => DateTime.Now)
                    let lastRunTime = t.lastRunTime.Run(env).IfFail(_ => startTime)
                    from now in Now<RT>()
                    let runningBehind = now > lastRunTime + (TimeSpan)t.interval
                    let boundary = t.interval == PositiveDuration.Zero
                        ? t.interval
                        : SecondsToIntervalStart(startTime, now, t.interval)
                    let sleepTime = boundary == PositiveDuration.Zero ? t.interval : boundary
                    let nextRun = runningBehind ? now : now + (TimeSpan)sleepTime
                    select (
                        Result: runningBehind
                            ? ScheduleResult.RunAgain
                            : ScheduleResult.DelayAndRunAgain(sleepTime),
                        State: t with { lastRunTime = Eff<RT, DateTime>.Success(nextRun) }));
        }

        ///<summary>
        /// A schedule that divides the timeline to `interval`-long windows, and sleeps
        /// until the nearest window boundary every time it recurs.
        ///
        /// For example, `Windowed(10*seconds)` would produce a schedule as follows:
        /// <pre>
        ///      10s        10s        10s       10s
        /// |----------|----------|----------|----------|
        /// |action------|sleep---|act|-sleep|action----|
        /// </pre>
        /// </summary>
        /// <param name="interval">schedule interval</param>
        /// <param name="currentTime">current time function</param>
        public static Sched Windowed(PositiveDuration interval, Func<DateTime> currentTime = null)
        {
            var ctFn = currentTime ?? LiveNowFn;
            return New((currentTimeFn: ctFn, startTime: ctFn(), interval),
                static (t, _)
                    => ScheduleResult.DelayAndRunAgain(
                        SecondsToIntervalStart(t.startTime, t.currentTimeFn(), t.interval)));
        }

        ///<summary>
        /// A schedule that divides the timeline to `interval`-long windows, and sleeps
        /// until the nearest window boundary every time it recurs.
        ///
        /// For example, `Windowed(10*seconds)` would produce a schedule as follows:
        /// <pre>
        ///      10s        10s        10s       10s
        /// |----------|----------|----------|----------|
        /// |action------|sleep---|act|-sleep|action----|
        /// </pre>
        /// </summary>
        /// <param name="interval">schedule interval</param>
        public static Sched<RT, A> Windowed<RT, A>(PositiveDuration interval)
            where RT : struct, HasCancel<RT>, HasTime<RT>
            => New<(Eff<RT, DateTime> startTime, PositiveDuration interval), RT, A>(
                (startTime: Now<RT>().Memo(), interval),
                static (t, _, _) =>
                    from env in runtime<RT>()
                    let startTime = t.startTime.Run(env).IfFail(_ => DateTime.Now)
                    from now in Now<RT>()
                    select ScheduleResult.DelayAndRunAgain(SecondsToIntervalStart(startTime, now, t.interval)));

        [Pure]
        private static int DurationToIntervalStart(
            int intervalStart, int currentIntervalPosition, int intervalWidth)
        {
            var steps = intervalStart - currentIntervalPosition;
            return steps > 0 ? steps : steps + intervalWidth;
        }

        [Pure]
        private static int RoundBetween(int value, int min, int max)
            => value > max ? max : value < min ? min : value;

        /// <summary>
        /// Cron-like schedule that recurs every specified `second` of each minute.
        /// </summary>
        /// <param name="second">second of the minute, will be rounded to fit between 0 and 59</param>
        /// <param name="currentTime">current time function</param>
        public static Sched SecondOfMinute(int second, Func<DateTime> currentTime = null)
            => New((currentTimeFn: currentTime ?? LiveNowFn, second: RoundBetween(second, 0, 59)),
                static (t, _) => ScheduleResult.DelayAndRunAgain(
                    DurationToIntervalStart(t.second, t.currentTimeFn().Second, 60) * seconds));

        /// <summary>
        /// Cron-like schedule that recurs every specified `second` of each minute.
        /// </summary>
        /// <param name="second">second of the minute, will be rounded to fit between 0 and 59</param>
        public static Sched<RT, A> SecondOfMinute<RT, A>(int second) where RT : struct, HasCancel<RT>, HasTime<RT> =>
            New<int, RT, A>(RoundBetween(second, 0, 59), static (second, _, _) =>
                from now in Now<RT>()
                select ScheduleResult.DelayAndRunAgain(DurationToIntervalStart(second, now.Second, 60) * seconds));

        /// <summary>
        /// Cron-like schedule that recurs every specified `minute` of each hour.
        /// </summary>
        /// <param name="minute">minute of the hour, will be rounded to fit between 0 and 59</param>
        /// <param name="currentTime">current time function</param>
        public static Sched MinuteOfHour(int minute, Func<DateTime> currentTime = null)
            => New((currentTimeFn: currentTime ?? LiveNowFn, minute: RoundBetween(minute, 0, 59)),
                static (t, _) => ScheduleResult.DelayAndRunAgain(
                    DurationToIntervalStart(t.minute, t.currentTimeFn().Minute, 60) * minutes));

        /// <summary>
        /// Cron-like schedule that recurs every specified `minute` of each hour.
        /// </summary>
        /// <param name="minute">minute of the hour, will be rounded to fit between 0 and 59</param>
        public static Sched<RT, A> MinuteOfHour<RT, A>(int minute) where RT : struct, HasCancel<RT>, HasTime<RT> =>
            New<int, RT, A>(RoundBetween(minute, 0, 59), static (minute, _, _) =>
                from now in Now<RT>()
                select ScheduleResult.DelayAndRunAgain(DurationToIntervalStart(minute, now.Minute, 60) * minutes));

        /// <summary>
        /// Cron-like schedule that recurs every specified `hour` of each day.
        /// </summary>
        /// <param name="hour">hour of the day, will be rounded to fit between 0 and 23</param>
        /// <param name="currentTime">current time function</param>
        public static Sched HourOfDay(int hour, Func<DateTime> currentTime = null)
            => New((currentTimeFn: currentTime ?? LiveNowFn, hour: RoundBetween(hour, 0, 23)),
                static (t, _) => ScheduleResult.DelayAndRunAgain(
                    DurationToIntervalStart(t.hour, t.currentTimeFn().Hour, 24) * hours));

        /// <summary>
        /// Cron-like schedule that recurs every specified `hour` of each day.
        /// </summary>
        /// <param name="hour">hour of the day, will be rounded to fit between 0 and 23</param>
        public static Sched<RT, A> HourOfDay<RT, A>(int hour) where RT : struct, HasCancel<RT>, HasTime<RT> =>
            New<int, RT, A>(RoundBetween(hour, 0, 23), static (hour, _, _) =>
                from now in Now<RT>()
                select ScheduleResult.DelayAndRunAgain(DurationToIntervalStart(hour, now.Hour, 24) * hours));

        /// <summary>
        /// Cron-like schedule that recurs every specified `day` of each week.
        /// </summary>
        /// <param name="day">day of the week</param>
        /// <param name="currentTime">current time function</param>
        public static Sched DayOfWeek(DayOfWeek day, Func<DateTime> currentTime = null)
            => New((currentTimeFn: currentTime ?? LiveNowFn, day),
                static (t, _) => ScheduleResult.DelayAndRunAgain(
                    DurationToIntervalStart((int)t.day + 1, (int)t.currentTimeFn().DayOfWeek + 1, 7) * days));

        /// <summary>
        /// Cron-like schedule that recurs every specified `day` of each week.
        /// </summary>
        /// <param name="day">day of the week</param>
        public static Sched<RT, A> DayOfWeek<RT, A>(DayOfWeek day) where RT : struct, HasCancel<RT>, HasTime<RT> =>
            New<DayOfWeek, RT, A>(day, static (day, _, _) =>
                from now in Now<RT>()
                select ScheduleResult.DelayAndRunAgain(
                    DurationToIntervalStart((int)day, (int)now.DayOfWeek, 7) * days));

        /// <summary>
        /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
        /// For example: 117ms, 236ms, 141ms, 424ms, ...
        /// Per the formula from https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
        ///
        /// Sudden issues affecting performance, combined with a fixed-progression wait-and-retry,
        /// can lead to subsequent retries being highly correlated. For example, if there are 50 concurrent failures,
        /// and all 50 requests enter a wait-and-retry for 10ms, then all 50 requests will hit the service again in 10ms;
        /// potentially overwhelming the service again.
        /// 
        /// One way to address this is to add some randomness to the wait delay.
        /// This will cause each request to vary slightly on retry, which decorrelates the retries from each other.
        /// </summary>
        /// <remarks>
        /// Source https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#earlier-jitter-recommendations
        /// </remarks>
        /// <param name="minDelay">>minimum delay before each retry, default 10 milliseconds</param>
        /// <param name="maxDelay">maximum delay before each retry, default 100 milliseconds</param>
        /// <param name="retry">maximum number of retries to use, default 5</param>
        /// <param name="seed">optional seed to use</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Sched AwsDecorrelated(
            Option<PositiveDuration> minDelay = default,
            Option<PositiveDuration> maxDelay = default,
            int retry = 5,
            Option<int> seed = default,
            bool fastFirst = false)
        {
            var min = minDelay.IfNone(() => 10);
            var max = maxDelay.IfNone(() => 100);
            return Recurs(retry)
                   & New((min, max, seed), min,
                       static (t, _, ms) =>
                       {
                           var (min, max, seed) = t;
                           var ceiling = Math.Min(max, (double)ms.AppliedDelay * 3);
                           return SingletonRandom.Uniform(min, ceiling, seed);
                       })
                   & (fastFirst ? NoDelayOnFirstRetry : Identity);
        }

        /// <summary>
        /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
        /// For example: 850ms, 1455ms, 3060ms.
        ///
        /// Sudden issues affecting performance, combined with a fixed-progression wait-and-retry,
        /// can lead to subsequent retries being highly correlated. For example, if there are 50 concurrent failures,
        /// and all 50 requests enter a wait-and-retry for 10ms, then all 50 requests will hit the service again in 10ms;
        /// potentially overwhelming the service again.
        /// 
        /// One way to address this is to add some randomness to the wait delay.
        /// This will cause each request to vary slightly on retry, which decorrelates the retries from each other.
        /// </summary>
        /// <remarks>
        /// Source https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation
        /// </remarks>
        /// <param name="medianFirstRetryDelay">median delay to target before the first retry, call it f (= f * 2^0).
        /// Choose this value both to approximate the first delay, and to scale the remainder of the series.
        /// Subsequent retries will (over a large sample size) have a median approximating retries at time f * 2^1, f * 2^2 ... f * 2^t etc for try t.
        /// The actual amount of delay-before-retry for try t may be distributed between 0 and f * (2^(t+1) - 2^(t-1)) for t >= 2;
        /// or between 0 and f * 2^(t+1), for t is 0 or 1, default 100 milliseconds</param>
        /// <param name="retry">The maximum number of retries to use, default 5</param>
        /// <param name="seed">An optional seed to use</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Sched PollyDecorrelated(
            Option<PositiveDuration> medianFirstRetryDelay = default,
            int retry = 5,
            Option<int> seed = default,
            bool fastFirst = false)
        {
            var mrd = medianFirstRetryDelay.IfNone(() => 100);

            // A factor used within the formula to help smooth the first calculated delay.
            const double pFactor = 4.0;

            // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid user comprehension.
            // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
            const double rpScalingFactor = 1 / 1.4d;

            // Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
            // (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
            // is avoided by the arbitrary subtraction of 1000.
            var maxTimeSpanDouble = (double)TimeSpan.MaxValue.Ticks - 1000;

            return Recurs(retry)
                   & New((seed, maxTimeSpanDouble), mrd,
                       static (s, seed, ctx) =>
                       {
                           var t = ctx.Iteration + SingletonRandom.NextDouble(s.seed);
                           var next = Math.Pow(2, t) * Math.Tanh(Math.Sqrt(pFactor * t));
                           var formulaIntrinsicValue = next - (double)ctx.AppliedDelay;
                           return TimeSpan.FromTicks((long)Math.Min(
                               formulaIntrinsicValue * rpScalingFactor * ((TimeSpan)seed).Ticks,
                               s.maxTimeSpanDouble));
                       })
                   & (fastFirst ? NoDelayOnFirstRetry : Identity);
        }

        /// <summary>
        /// A schedule transformer that will enforce the first retry has no delay.
        /// </summary>
        [Pure]
        public static ScheduleResultTransformer NoDelayOnFirstRetry
            => (context, result) => context.HasNotStarted ? ScheduleResult.RunAgain : result;

        /// <summary>
        /// A schedule transformer that limits the returned delays to max delay.
        /// </summary>
        /// <param name="max">max delay to return</param>
        [Pure]
        public static ScheduleResultTransformer MaxDelay(PositiveDuration max)
            => (_, result) => result.DelayUnsafe > max ? ScheduleResult.DelayAndRunAgain(max) : result;

        /// <summary>
        /// Limits the schedule to the max cumulative delay.
        /// </summary>
        /// <param name="max">max delay to stop schedule at</param>
        [Pure]
        public static ScheduleResultTransformer MaxCumulativeDelay(PositiveDuration max)
            => (context, result) => context.CumulativeAppliedDelay >= max ? ScheduleResult.Complete : result;

        /// <summary>
        /// A schedule transformer that adds a random jitter to any returned delay.
        /// </summary>
        /// <param name="minRandom">min random milliseconds</param>
        /// <param name="maxRandom">max random milliseconds</param>
        /// <param name="seed">optional seed</param>
        [Pure]
        public static ScheduleResultTransformer Jitter(
            PositiveDuration minRandom, PositiveDuration maxRandom, Option<int> seed = default)
            => (_, result) => result + SingletonRandom.Uniform(minRandom, maxRandom, seed);

        /// <summary>
        /// A schedule transformer that adds a random jitter to any returned delay.
        /// </summary>
        /// <param name="factor">jitter factor based on the returned delay</param>
        /// <param name="seed">optional seed</param>
        [Pure]
        public static ScheduleResultTransformer Jitter(double factor = 0.5, Option<int> seed = default)
            => (_, result) => result + SingletonRandom.Uniform(0, result.DelayUnsafe * factor, seed);
    }
}
