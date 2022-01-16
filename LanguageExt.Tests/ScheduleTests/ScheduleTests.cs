using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.Common;
using LanguageExt.Core.Extensions;
using LanguageExt.Sys.Test;
using Xunit;
using static LanguageExt.ScheduleResult;
using static LanguageExt.Prelude;
using static LanguageExt.ScheduleStatus;

namespace LanguageExt.Tests.ScheduleTests
{
    public static class ScheduleTests
    {
        [Fact]
        public static void OnceTest()
        {
            var schedule = Schedule.Once;
            var results = schedule.DryRun();
            results.Should()
                .HaveCount(1)
                .And
                .Contain((RunOnce, Complete));
        }

        [Fact]
        public static void ForeverTest()
        {
            var schedule = Schedule.Forever;
            var results = schedule.DryRun().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Contain(x => x.Result == RunAgain);
        }

        [Fact]
        public static void UnionTest()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Spaced(100);
            var results = schedule.DryRun().AppliedDelays();
            var ts = TimeSpan.FromMilliseconds(100);
            results
                .Should()
                .HaveCount(5)
                .And
                .Equal(ts, ts, ts, ts, ts);
        }

        [Fact]
        public static void UnionExponentialTest()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Exponential(2000);
            var results = schedule.DryRun();
            results
                .Should()
                .HaveCount(6)
                .And
                .Equal(
                    (
                        RunOnce,
                        TimeSpan.FromSeconds(2).AsResult()),
                    (
                        ReRunOnce(TimeSpan.FromSeconds(2)),
                        TimeSpan.FromSeconds(4).AsResult()),
                    (
                        ReRunMoreThanOnce(
                            3,
                            TimeSpan.FromSeconds(6),
                            TimeSpan.FromSeconds(4),
                            TimeSpan.FromSeconds(2)),
                        TimeSpan.FromSeconds(8).AsResult()),
                    (
                        ReRunMoreThanOnce(
                            4,
                            TimeSpan.FromSeconds(14),
                            TimeSpan.FromSeconds(8),
                            TimeSpan.FromSeconds(4)),
                        TimeSpan.FromSeconds(16).AsResult()),
                    (
                        ReRunMoreThanOnce(
                            5,
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromSeconds(16),
                            TimeSpan.FromSeconds(8)),
                        TimeSpan.FromSeconds(32).AsResult()),
                    (
                        ReRunMoreThanOnce(
                            6,
                            TimeSpan.FromSeconds(62),
                            TimeSpan.FromSeconds(32),
                            TimeSpan.FromSeconds(16)),
                        Complete)
                );
        }

        [Fact]
        public static void MaxDelayTest()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Exponential(2000) | Schedule.MaxDelay(7000);
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(7),
                    TimeSpan.FromSeconds(7),
                    TimeSpan.FromSeconds(7));
        }

        [Fact]
        public static void UnionFibonacciTest()
        {
            var schedule = Schedule.Recurs(7) | Schedule.Fibonacci(1000);
            var results = schedule.DryRun();
            results.Should()
                .HaveCount(8)
                .And
                .Equal(
                    (
                        RunOnce,
                        TimeSpan.FromSeconds(1).AsResult()),
                    (
                        ReRunOnce(TimeSpan.FromSeconds(1)),
                        TimeSpan.FromSeconds(1).AsResult()),
                    (
                        ReRunMoreThanOnce(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)),
                        TimeSpan.FromSeconds(2).AsResult()),
                    (
                        ReRunMoreThanOnce(4, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1)),
                        TimeSpan.FromSeconds(3).AsResult()),
                    (
                        ReRunMoreThanOnce(5, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2)),
                        TimeSpan.FromSeconds(5).AsResult()),
                    (
                        ReRunMoreThanOnce(6, TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(3)),
                        TimeSpan.FromSeconds(8).AsResult()),
                    (
                        ReRunMoreThanOnce(7, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(8),
                            TimeSpan.FromSeconds(5)),
                        TimeSpan.FromSeconds(13).AsResult()),
                    (
                        ReRunMoreThanOnce(8, TimeSpan.FromSeconds(33), TimeSpan.FromSeconds(13),
                            TimeSpan.FromSeconds(8)),
                        Complete)
                );
        }

        [Fact(Skip = "Not ready yet")]
        public static void FixedTest()
        {
            var i = 0;
            var ct = new DateTimeOffset(2022, 01, 15, 0, 0, 0, TimeSpan.Zero);
            var schedule = Schedule.Recurs(5) | Schedule.Fixed(1000, () =>
            {
                ct += i % 2 == 0 ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(0.5);
                i++;
                return ct;
            });
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5);
        }

        [Fact]
        public static void OnErrorTest1()
        {
            var error = Error.New(1, "Some Custom Error");
            var schedule = Schedule<int>.OnError(error) | Schedule.Fibonacci(TimeSpan.FromSeconds(1));
            var results = schedule.DryRun(status => status.Iterations() < 5 ? error : 1).AppliedDelays();
            results.Should()
                .HaveCount(4)
                .And
                .Equal(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3));
        }

        [Fact]
        public static void OnErrorTest2()
        {
            var error = Error.New(2, "Some Custom Error");
            var schedule = Schedule<Runtime, int>.OnError(error) | Schedule.Fibonacci(TimeSpan.FromSeconds(1));
            var results = schedule.DryRun(Runtime.New(), status => status.Iterations() < 5 ? error : 1).AppliedDelays();
            results.Should()
                .HaveCount(4)
                .And
                .Equal(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3));
        }

        [Fact]
        public static void JitterTest1()
        {
            const int seed = 897654321;
            var schedule = Schedule.Recurs(5) | Schedule.Spaced(2000) | Schedule.Jitter(seed: seed);
            var maxAppliedDelay = schedule.DryRun().AppliedDelays().Sum(x => x.TotalMilliseconds);
            maxAppliedDelay.Should().BeGreaterThan(10000);
        }

        [Fact]
        public static void JitterTest2()
        {
            const int seed = 897654323;
            var schedule = Schedule.Recurs(5) & Schedule.Spaced(2000) | Schedule.Jitter(50, 250, seed: seed);
            var maxAppliedDelay = schedule.DryRun().AppliedDelays().Sum(x => x.TotalMilliseconds);
            maxAppliedDelay.Should().BeGreaterThan(10000);
        }

        [Fact]
        public static void MaxCumulativeDelayTest()
        {
            var schedule = Schedule.Spaced(2000) | Schedule.MaxCumulativeDelay(10000);
            var results = schedule.DryRun().ToSeq();
            var maxAppliedDelay = results.AppliedDelays().Sum(x => x.TotalMilliseconds);
            maxAppliedDelay.Should().BeLessOrEqualTo(10000);
            results.Should()
                .HaveCount(6)
                .And
                .Equal(
                    (
                        RunOnce,
                        DelayAndRunAgain(TimeSpan.FromSeconds(2))),
                    (
                        ReRunOnce(TimeSpan.FromSeconds(2)),
                        DelayAndRunAgain(TimeSpan.FromSeconds(2))),
                    (
                        ReRunMoreThanOnce(3, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)),
                        DelayAndRunAgain(TimeSpan.FromSeconds(2))),
                    (
                        ReRunMoreThanOnce(4, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)),
                        DelayAndRunAgain(TimeSpan.FromSeconds(2))),
                    (
                        ReRunMoreThanOnce(5, TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)),
                        DelayAndRunAgain(TimeSpan.FromSeconds(2))),
                    (
                        ReRunMoreThanOnce(6, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(2)),
                        Complete));
        }

        [Fact]
        public static void NoDelayOnFirstRetryTest1()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Spaced(2000) | Schedule.NoDelayOnFirstRetry();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2));
        }

        [Fact]
        public static void NoDelayOnFirstRetryTest2()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Exponential(1000) | Schedule.NoDelayOnFirstRetry();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(16));
        }

        [Fact]
        public static void NoDelayOnFirstRetryTest3()
        {
            var schedule = Schedule.Recurs(5) | Schedule.Linear(1000) | Schedule.NoDelayOnFirstRetry();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(4));
        }

        [Fact]
        public static void EffectfulScheduleTest1()
        {
            var schedule =
                Schedule<int>.Custom(static (result, _) =>
                    result.Exists(i => i % 2 == 0)
                        ? TimeSpan.FromSeconds(1).AsResult()
                        : TimeSpan.FromSeconds(2).AsResult())
                | Schedule.Recurs(5);
            var counter = 0;
            var delays = schedule.DryRun(_ => counter++).AppliedDelays();
            delays.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(1));
        }

        [Fact]
        public static void EffectfulScheduleTest2()
        {
            var schedule =
                Schedule<int>.Custom(static (result, _) => TimeSpan.FromSeconds((int)result).AsResult())
                | Schedule<int>.Transform(static (result, _, sr) => result.Exists(i => i > 0) ? sr : Complete);
            var counter = 10;
            var delays = schedule.DryRun(_ => counter--).AppliedDelays();
            delays.Should()
                .HaveCount(10)
                .And
                .Equal(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(9),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(7),
                    TimeSpan.FromSeconds(6),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(1));
        }

        private static void RunHasExpectedResult(
            IEnumerable<(Fin<int> Result, IScheduleResult Status)> results)
            => results.Should()
                .HaveCount(6)
                .And
                .Equal(
                    (Fin<int>.Succ(1), DelayAndRunAgain(TimeSpan.FromMilliseconds(1))),
                    (Fin<int>.Succ(1), DelayAndRunAgain(TimeSpan.FromMilliseconds(1))),
                    (Fin<int>.Succ(1), DelayAndRunAgain(TimeSpan.FromMilliseconds(1))),
                    (Fin<int>.Succ(1), DelayAndRunAgain(TimeSpan.FromMilliseconds(1))),
                    (Fin<int>.Succ(1), DelayAndRunAgain(TimeSpan.FromMilliseconds(1))),
                    (Fin<int>.Succ(1), Complete));

        [Fact]
        public static async Task RunTest1()
        {
            var effect = SuccessAff(1);
            var schedule = (Schedule.Recurs(5) | Schedule.Spaced(1)).WithRuntimeAndValue<Runtime, int>();
            var results = await schedule.Run(Runtime.New(), effect).ToEnumerable();
            RunHasExpectedResult(results);
        }

        [Fact]
        public static async Task RunTest2()
        {
            var effect = SuccessAff(1);
            var schedule = (Schedule.Recurs(5) | Schedule.Spaced(1)).WithValue<int>();
            var results = await schedule.Run(effect).ToEnumerable();
            RunHasExpectedResult(results);
        }

        [Fact]
        public static void RunTest3()
        {
            var effect = SuccessEff(1);
            var schedule = (Schedule.Recurs(5) | Schedule.Spaced(1) | Schedule<int>.Custom(_ => RunAgain))
                .WithRuntime<Runtime, int>();
            var results = schedule.Run(Runtime.New(), effect);
            RunHasExpectedResult(results);
        }

        [Fact]
        public static void RunTest4()
        {
            var effect = SuccessEff(1);
            var schedule = (Schedule.Recurs(5) | Schedule.Spaced(1)).WithValue<int>();
            var results = schedule.Run(effect);
            RunHasExpectedResult(results);
        }
    }
}
