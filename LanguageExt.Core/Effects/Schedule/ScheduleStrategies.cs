using System;

namespace LanguageExt
{
    /// <summary>
    /// Some common schedule strategies and details on when to use them.
    /// </summary>
    /// <remarks>
    /// Most are sourced from Polly Contrib https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry
    /// </remarks>
    public static class ScheduleStrategies
    {
        /// <summary>
        /// Schedule that will retry five times and pause 200ms between each call.
        ///
        /// Simple strategy for dealing with transient failures that are not susceptible to
        /// overload from fast retries. 
        /// </summary>
        /// <remarks>
        /// Source https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-constant-back-off
        /// </remarks>
        /// <param name="retry">number of retry attempts, default 5</param>
        /// <param name="space">constant space between each retry, default 200 ms</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Schedule FixedConstantBackoff(
            int retry = 5,
            Option<TimeSpan> space = default,
            bool fastFirst = false)
            => Schedule.Recurs(retry)
               | Schedule.Spaced(space.IfNone(() => TimeSpan.FromMilliseconds(200)))
               | (fastFirst ? Schedule.NoDelayOnFirstRetry() : Schedule.NoOp);

        /// <summary>
        /// Schedule that will retry five times and pause based on a linear backoff starting at 100 ms.
        ///
        /// Simple strategy for dealing with transient failures that are susceptible to
        /// overload from fast retries. In this scenario, we want to give the effect some time to stabilize before trying again.
        /// </summary>
        /// <remarks>
        /// Source https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-linear-back-off
        /// </remarks>
        /// <param name="retry">number of retry attempts, default 5</param>
        /// <param name="space">initial space, default 100 ms</param>
        /// <param name="factor">linear factor to apply, default 1</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Schedule LinearBackoff(
            int retry = 5,
            Option<TimeSpan> space = default,
            double factor = 1,
            bool fastFirst = false)
            => Schedule.Recurs(retry)
               | Schedule.Linear(space.IfNone(() => TimeSpan.FromMilliseconds(100)), factor)
               | (fastFirst ? Schedule.NoDelayOnFirstRetry() : Schedule.NoOp);

        /// <summary>
        /// Schedule that will retry five times and pause based on a exponential backoff starting at 100 ms.
        ///
        /// Because of the exponential nature, this is best used with a low starting delay or in out-of-band communication,
        /// such as a service worker polling for information from a remote endpoint.
        /// Due to the potential for rapidly increasing times, care should be taken if an exponential retry is used in the code path for servicing a user request.
        ///
        /// If the overall amount of time that an exponential-backoff retry policy could take is a concern,
        /// consider combining it with a max cumulative delay,
        ///
        ///     ScheduleStrategies.ExponentialBackoff() | Schedule.MaxCumulativeDelay(TimeSpan.FromSeconds(45))
        /// 
        /// </summary>
        /// <remarks>
        /// Source https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-exponential-back-off
        /// </remarks>
        /// <param name="retry">maximum number of retries to use, default 5</param>
        /// <param name="space">initial space, default 100 ms</param>
        /// <param name="factor">linear factor to apply, default 2</param>
        /// <param name="fastFirst">flag to indicate the first retry is immediate, default false</param>
        public static Schedule ExponentialBackoff(
            int retry = 5,
            Option<TimeSpan> space = default,
            double factor = 2,
            bool fastFirst = false)
            => Schedule.Recurs(retry)
               | Schedule.Exponential(space.IfNone(() => TimeSpan.FromMilliseconds(100)), factor)
               | (fastFirst ? Schedule.NoDelayOnFirstRetry() : Schedule.NoOp);

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
        public static Schedule AwsDecorrelatedJitterBackoff(
            Option<TimeSpan> minDelay = default,
            Option<TimeSpan> maxDelay = default,
            int retry = 5,
            Option<int> seed = default,
            bool fastFirst = false)
        {
            var min = minDelay.IfNone(() => TimeSpan.FromMilliseconds(10));
            var max = maxDelay.IfNone(() => TimeSpan.FromMilliseconds(100));
            return Schedule.Recurs(retry)
                   | Schedule.Custom(min, (Max: max, Seed: seed),
                       static (min, t, ms) =>
                       {
                           var (max, seed) = t;
                           var ceiling = Math.Min(max.TotalMilliseconds, ms.Multiply(3).TotalMilliseconds);
                           return TimeSpan.FromMilliseconds(
                               SingletonRandom.Uniform(seed, min.TotalMilliseconds, ceiling));
                       })
                   | (fastFirst ? Schedule.NoDelayOnFirstRetry() : Schedule.NoOp);
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
        public static Schedule PollyDecorrelatedJitterBackoff(
            Option<TimeSpan> medianFirstRetryDelay = default,
            int retry = 5,
            Option<int> seed = default,
            bool fastFirst = false)
        {
            var mrd = medianFirstRetryDelay.IfNone(() => TimeSpan.FromMilliseconds(100));

            // A factor used within the formula to help smooth the first calculated delay.
            const double pFactor = 4.0;

            // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid user comprehension.
            // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
            const double rpScalingFactor = 1 / 1.4d;

            // Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
            // (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
            // is avoided by the arbitrary subtraction of 1000.
            var maxTimeSpanDouble = (double)TimeSpan.MaxValue.Ticks - 1000;

            return Schedule.Recurs(retry)
                   | Schedule.Custom(mrd, new { seed, maxTimeSpanDouble },
                       static (s, ctx, prev, i) =>
                       {
                           var t = i + SingletonRandom.NextDouble(ctx.seed);
                           var next = Math.Pow(2, t) * Math.Tanh(Math.Sqrt(pFactor * t));
                           var formulaIntrinsicValue = next - prev.TotalMilliseconds;
                           return TimeSpan.FromTicks((long)Math.Min(
                               formulaIntrinsicValue * rpScalingFactor * s.Ticks,
                               ctx.maxTimeSpanDouble));
                       })
                   | (fastFirst ? Schedule.NoDelayOnFirstRetry() : Schedule.NoOp);
        }
    }
}
