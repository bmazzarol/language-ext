using System;
using System.Diagnostics.Contracts;
using System.Linq;
using FluentAssertions;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt.Next.Tests.ScheduleTests
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
                .Contain((ScheduleContext.Initial, ScheduleResult.Complete));
        }

        [Fact]
        public static void ForeverTest()
        {
            var schedule = Schedule.Forever;
            var results = schedule.DryRun().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Contain(t => t.Result == ScheduleResult.RunAgain);
        }

        [Fact]
        public static void FromDurationsTest()
        {
            var schedule = Schedule.FromDurations(1 * sec, 2 * sec, 3 * sec);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(3)
                .And
                .Equal(1 * sec, 2 * sec, 3 * sec);
        }

        [Fact]
        public static void RecursTest()
        {
            var schedule = Schedule.Recurs(5);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Contain(x => x.Equals(0 * sec));
        }

        [Fact]
        public static void SpacedTest()
        {
            var schedule = Schedule.Spaced(5 * sec);
            var results = schedule.AppliedDelays().ToSeq().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Contain(x => x.Equals(5 * sec));
        }

        [Fact]
        public static void SpacedTest2()
        {
            var schedule = Schedule.Spaced();
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Contain(x => x.Equals(200 * ms));
        }

        [Fact]
        public static void SpacedTest3()
        {
            var schedule = Schedule.Spaced(2, fastFirst: true);
            var results = schedule.DryRun();
            results.Should()
                .HaveCount(3)
                .And
                .Equal(
                    (ScheduleContext.Initial, ScheduleResult.RunAgain),
                    (
                        ScheduleContext.New(1, 0, ScheduleResult.RunAgain),
                        ScheduleResult.DelayAndRunAgain(200 * ms)),
                    (
                        ScheduleContext.New(2, 200 * ms, ScheduleResult.DelayAndRunAgain(200 * ms)),
                        ScheduleResult.Complete)
                );
        }

        [Fact]
        public static void LinearTest()
        {
            var schedule = Schedule.Linear(1 * sec);
            var results = schedule.AppliedDelays().ToSeq().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Equal(1 * sec, 2 * sec, 3 * sec, 4 * sec, 5 * sec);
        }

        [Fact]
        public static void LinearTest1()
        {
            var schedule = Schedule.Linear();
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(100 * ms, 200 * ms, 300 * ms, 400 * ms, 500 * ms);
        }

        [Fact]
        public static void LinearTest2()
        {
            var schedule = Schedule.Linear(factor: 2);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(100, 300, 500, 700, 900);
        }

        [Fact]
        public static void ExponentialTest()
        {
            var schedule = Schedule.Exponential(1 * sec);
            var results = schedule.AppliedDelays().ToSeq().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Equal(1 * sec, 2 * sec, 4 * sec, 8 * sec, 16 * sec);
        }

        [Fact]
        public static void ExponentialTest2()
        {
            var schedule = Schedule.Exponential();
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(100, 200, 400, 800, 1600);
        }

        [Fact]
        public static void FibonacciTest()
        {
            var schedule = Schedule.Fibonacci(1 * sec);
            var results = schedule.AppliedDelays().ToSeq().Take(5);
            results.Should()
                .HaveCount(5)
                .And
                .Equal(1 * sec, 1 * sec, 2 * sec, 3 * sec, 5 * sec);
        }

        [Pure]
        private static Seq<DateTime> FromDuration(PositiveDuration duration)
        {
            var now = DateTime.Now;
            return Range(0, (int)((TimeSpan)duration).TotalSeconds)
                .Select(i => now + TimeSpan.FromSeconds(i))
                .ToSeq();
        }

        [Pure]
        private static Seq<DateTime> FromDurations(Seq<PositiveDuration> durations)
            => durations.Fold(Seq1(DateTime.Now), (times, duration) =>
            {
                var last = times.Head();
                return times.Add(last + (TimeSpan)duration);
            });

        [Pure]
        private static Func<DateTime> FromDates(Seq<DateTime> dates) => () =>
        {
            var date = dates.HeadOrNone().IfNone(() => DateTime.Now);
            dates = dates.Tail;
            return date;
        };

        [Fact]
        public static void UpToTest()
        {
            var schedule = Schedule.UpTo(5 * sec, FromDates(FromDuration(2 * min)));
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(0, 0, 0, 0, 0);
        }

        [Fact]
        public static void UpToTest2()
        {
            var schedule = Schedule.UpTo<ScheduleTestRuntime, int>(5 * sec);
            var results = schedule.AppliedDelays(
                    new ScheduleTestRuntime(FromDates(FromDuration(2 * min))),
                    Eff<ScheduleTestRuntime, int>.Success(0))
                .ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Equal(0, 0, 0, 0, 0);
        }

        [Fact]
        public static void FixedTest1()
        {
            var schedule = Schedule.Fixed(5 * sec, FromDates(FromDurations(Seq<PositiveDuration>(
                6 * sec,
                1 * sec,
                4 * sec
            ))));
            var results = schedule.AppliedDelays().ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(0, 4 * sec, 1 * sec);
        }

        [Fact]
        public static void FixedTest2()
        {
            var schedule = Schedule.Fixed<ScheduleTestRuntime, int>(5 * sec);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(FromDurations(Seq<PositiveDuration>(
                    6 * sec,
                    1 * sec,
                    4 * sec
                )))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(0, 4 * sec, 1 * sec);
        }

        [Fact]
        public static void WindowedTest1()
        {
            var schedule = Schedule.Windowed(5 * sec, FromDates(FromDurations(Seq<PositiveDuration>(
                6 * sec,
                1 * sec,
                7 * sec
            ))));
            var results = schedule.AppliedDelays().ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(4 * sec, 4 * sec, 3 * sec);
        }

        [Fact]
        public static void WindowedTest2()
        {
            var schedule = Schedule.Windowed<ScheduleTestRuntime, int>(5 * sec);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(FromDurations(Seq<PositiveDuration>(
                    6 * sec,
                    1 * sec,
                    7 * sec
                )))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(4 * sec, 4 * sec, 3 * sec);
        }

        [Fact]
        public static void SecondOfMinuteTest1()
        {
            var schedule = Schedule.SecondOfMinute(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 1, 26),
                new DateTime(2022, 1, 1, 1, 1, 1),
                new DateTime(2022, 1, 1, 1, 1, 47)
            )));
            var results = schedule.AppliedDelays().ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(37 * sec, 2 * sec, 16 * sec);
        }

        [Fact]
        public static void SecondOfMinuteTest2()
        {
            var schedule = Schedule.SecondOfMinute<ScheduleTestRuntime, int>(3);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(Seq(
                    new DateTime(2022, 1, 1, 1, 1, 26),
                    new DateTime(2022, 1, 1, 1, 1, 1),
                    new DateTime(2022, 1, 1, 1, 1, 47)
                ))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(37 * sec, 2 * sec, 16 * sec);
        }

        [Fact]
        public static void MinuteOfHourTest1()
        {
            var schedule = Schedule.MinuteOfHour(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 26, 0),
                new DateTime(2022, 1, 1, 1, 1, 0),
                new DateTime(2022, 1, 1, 1, 47, 0)
            )));
            var results = schedule.AppliedDelays().ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(37 * min, 2 * min, 16 * min);
        }

        [Fact]
        public static void MinuteOfHourTest2()
        {
            var schedule = Schedule.MinuteOfHour<ScheduleTestRuntime, int>(3);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(Seq(
                    new DateTime(2022, 1, 1, 1, 26, 0),
                    new DateTime(2022, 1, 1, 1, 1, 0),
                    new DateTime(2022, 1, 1, 1, 47, 0)
                ))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(3);
            results.Should()
                .HaveCount(3)
                .And
                .Equal(37 * min, 2 * min, 16 * min);
        }

        [Fact]
        public static void HourOfDayTest1()
        {
            var schedule = Schedule.HourOfDay(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 0, 0),
                new DateTime(2022, 1, 1, 4, 0, 0),
                new DateTime(2022, 1, 1, 6, 0, 0),
                new DateTime(2022, 1, 1, 3, 0, 0)
            )));
            var results = schedule.AppliedDelays().ToSeq().Take(4);
            results.Should()
                .HaveCount(4)
                .And
                .Equal(2 * hours, 23 * hours, 21 * hour, 24 * hours);
        }

        [Fact]
        public static void HourOfDayTest2()
        {
            var schedule = Schedule.HourOfDay<ScheduleTestRuntime, int>(3);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(Seq(
                    new DateTime(2022, 1, 1, 1, 0, 0),
                    new DateTime(2022, 1, 1, 4, 0, 0),
                    new DateTime(2022, 1, 1, 6, 0, 0),
                    new DateTime(2022, 1, 1, 3, 0, 0)
                ))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(4);
            results.Should()
                .HaveCount(4)
                .And
                .Equal(2 * hours, 23 * hours, 21 * hour, 24 * hours);
        }

        [Fact]
        public static void DayOfWeekTest1()
        {
            var schedule = Schedule.DayOfWeek(DayOfWeek.Wednesday, FromDates(Seq(
                new DateTime(2022, 1, 1, 0, 0, 0), // Saturday
                new DateTime(2022, 1, 3, 0, 0, 0), // Monday
                new DateTime(2022, 1, 7, 0, 0, 0), // Friday
                new DateTime(2022, 1, 5, 0, 0, 0) // Wednesday
            )));
            var results = schedule.AppliedDelays().ToSeq().Take(4);
            results.Should()
                .HaveCount(4)
                .And
                .Equal(4 * days, 2 * days, 5 * days, 7 * days);
        }

        [Fact]
        public static void DayOfWeekTest2()
        {
            var schedule = Schedule.DayOfWeek<ScheduleTestRuntime, int>(DayOfWeek.Wednesday);
            var results = schedule.AppliedDelays(
                new ScheduleTestRuntime(FromDates(Seq(
                    new DateTime(2022, 1, 1, 0, 0, 0), // Saturday
                    new DateTime(2022, 1, 3, 0, 0, 0), // Monday
                    new DateTime(2022, 1, 7, 0, 0, 0), // Friday
                    new DateTime(2022, 1, 5, 0, 0, 0) // Wednesday
                ))),
                Eff<ScheduleTestRuntime, int>.Success(0)).ToSeq().Take(4);
            results.Should()
                .HaveCount(4)
                .And
                .Equal(4 * days, 2 * days, 5 * days, 7 * days);
        }

        private const int Seed = 98192732;

        [Fact]
        public static void AwsDecorrelatedTest()
        {
            var schedule = Schedule.AwsDecorrelated(retry: 10, seed: Seed);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(10)
                .And
                .Subject
                .Zip(results.Skip(1))
                .Should()
                .Contain(x => x.Item1 < x.Item2);
        }

        [Fact]
        public static void PollyDecorrelatedTest()
        {
            var schedule = Schedule.PollyDecorrelated(retry: 10, seed: Seed);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(10)
                .And
                .Subject
                .Zip(results.Skip(1))
                .Should()
                .Contain(x => x.Item1 < x.Item2);
        }

        [Fact]
        public static void MaxDelayTest()
        {
            var schedule =
                Schedule.Linear(10 * seconds)
                & Schedule.MaxDelay(25 * seconds)
                & Schedule.Recurs(5);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(5)
                .And
                .Subject.Max()
                .Should().Be(25 * second);
        }

        [Fact]
        public static void MaxCumulativeDelayTest()
        {
            var schedule =
                Schedule.Linear(10 * seconds)
                & Schedule.MaxCumulativeDelay(40 * seconds)
                & Schedule.Recurs(5);
            var results = schedule.AppliedDelays().ToSeq();
            results.Should()
                .HaveCount(3)
                .And
                .Subject.Max()
                .Should().Be(30 * second);
        }

        [Fact]
        public static void JitterTest1()
        {
            var schedule =
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5);
            var noJitter = schedule.AppliedDelays().ToSeq();
            var withJitter = (schedule & Schedule.Jitter(1 * ms, 10 * ms)).AppliedDelays().ToSeq();
            withJitter.Should()
                .HaveCount(5)
                .And
                .Subject.Zip(noJitter)
                .Should()
                .Contain(x => x.Item1 > x.Item2)
                .And
                .Contain(x => x.Item1 - x.Item2 <= 100);
        }

        [Fact]
        public static void JitterTest2()
        {
            var schedule =
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5);
            var noJitter = schedule.AppliedDelays().ToSeq();
            var withJitter = (schedule & Schedule.Jitter(1.5)).AppliedDelays().ToSeq();
            withJitter.Should()
                .HaveCount(5)
                .And
                .Subject.Zip(noJitter)
                .Should()
                .Contain(x => x.Item1 > x.Item2)
                .And
                .Contain(x => x.Item1 - x.Item2 <= x.Item2 * 1.5);
        }
    }
}
