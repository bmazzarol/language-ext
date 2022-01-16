using System;
using FluentAssertions;
using Xunit;
using static LanguageExt.ScheduleResult;
using static LanguageExt.ScheduleStatus;

namespace LanguageExt.Tests.ScheduleTests
{
    public static class StrategyTests
    {
        [Fact]
        public static void FixedConstantBackoffTest1()
        {
            var schedule = ScheduleStrategies.FixedConstantBackoff();
            var results = schedule.DryRun().AppliedDelays();
            var ds = TimeSpan.FromMilliseconds(200);
            results.Should()
                .HaveCount(5)
                .And
                .Equal(ds, ds, ds, ds, ds);
        }

        [Fact]
        public static void FixedConstantBackoffTest2()
        {
            var schedule = ScheduleStrategies.FixedConstantBackoff(3, TimeSpan.FromMilliseconds(10), true);
            var results = schedule.DryRun().ToSeq();
            results.Should()
                .HaveCount(4)
                .And
                .Equal((
                        RunOnce,
                        DelayAndRunAgain(0)),
                    (
                        ReRunOnce(0),
                        DelayAndRunAgain(10)),
                    (
                        ReRunMoreThanOnce(3, 10, 10, 0),
                        DelayAndRunAgain(10)),
                    (
                        ReRunMoreThanOnce(4, 20, 10, 10),
                        Complete));
        }

        [Fact]
        public static void LinearBackoffTest1()
        {
            var schedule = ScheduleStrategies.LinearBackoff();
            var results = schedule.DryRun();
            results.Should()
                .HaveCount(6)
                .And
                .Equal((
                        RunOnce,
                        DelayAndRunAgain(100)),
                    (
                        ReRunOnce(100),
                        DelayAndRunAgain(200)),
                    (
                        ReRunMoreThanOnce(3, 300, 200, 100),
                        DelayAndRunAgain(300)),
                    (
                        ReRunMoreThanOnce(4, 600, 300, 200),
                        DelayAndRunAgain(400)),
                    (
                        ReRunMoreThanOnce(5, 1000, 400, 300),
                        DelayAndRunAgain(500)),
                    (
                        ReRunMoreThanOnce(6, 1500, 500, 400),
                        Complete));
        }

        [Fact]
        public static void LinearBackoffTest2()
        {
            var schedule = ScheduleStrategies.LinearBackoff(space: TimeSpan.FromMilliseconds(10), factor: 2);
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromMilliseconds(30),
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(70),
                    TimeSpan.FromMilliseconds(90));
        }

        [Fact]
        public static void ExponentialBackoffTest1()
        {
            var schedule = ScheduleStrategies.ExponentialBackoff();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(400),
                    TimeSpan.FromMilliseconds(800),
                    TimeSpan.FromMilliseconds(1600));
        }

        [Fact]
        public static void ExponentialBackoffTest2()
        {
            var schedule = ScheduleStrategies.ExponentialBackoff(factor: 2.75, fastFirst: true);
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(275),
                    TimeSpan.FromMilliseconds(756.25),
                    TimeSpan.FromMilliseconds(2079.6875),
                    TimeSpan.FromMilliseconds(5719.140625));
        }

        [Fact]
        public static void AwsDecorrelatedJitterTest1()
        {
            var schedule = ScheduleStrategies.AwsDecorrelatedJitterBackoff();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .NotContain(TimeSpan.FromMilliseconds(9))
                .And
                .NotContain(TimeSpan.FromMilliseconds(101));
        }

        [Fact]
        public static void AwsDecorrelatedJitterTest2()
        {
            var schedule = ScheduleStrategies.AwsDecorrelatedJitterBackoff(fastFirst: true);
            var results = schedule.DryRun().AppliedDelays();
            results.Should().HaveCount(5);
            results.Tail.Should().Contain(x => x != default);
        }

        [Fact]
        public static void PollyDecorrelatedJitterBackoffTest1()
        {
            var schedule = ScheduleStrategies.PollyDecorrelatedJitterBackoff();
            var results = schedule.DryRun().AppliedDelays();
            results.Should()
                .HaveCount(5)
                .And
                .Contain(x => x != default);
        }

        [Fact]
        public static void PollyDecorrelatedJitterBackoffTest2()
        {
            var schedule = ScheduleStrategies.PollyDecorrelatedJitterBackoff(fastFirst: true);
            var results = schedule.DryRun().AppliedDelays();
            results.Should().HaveCount(5);
            results.Tail.Should().Contain(x => x != default);
        }
    }
}
