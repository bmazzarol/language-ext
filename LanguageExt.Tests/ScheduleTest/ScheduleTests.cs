﻿#nullable enable

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using FluentAssertions;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt.Tests.ScheduleTest
{
    public sealed class ScheduleTests
    {
        [Fact]
        public static void ForeverTest()
        {
            var result = Schedule.Forever;
            result
                .AsEnumerable()
                .Take(10)
                .Should()
                .HaveCount(10)
                .And
                .OnlyContain(x => x == Duration.Zero);
        }

        [Fact]
        public static void NeverTest()
        {
            var result = Schedule.Never;
            result
                .AsEnumerable()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public static void OnceTest()
        {
            var result = Schedule.Once;
            result
                .AsEnumerable()
                .Should()
                .ContainSingle(x => x == Duration.Zero);
        }

        [Fact]
        public static void FromDurationsTest()
        {
            var result = Schedule.FromDurations(1 * sec, 2 * sec, 3 * sec);
            result
                .AsEnumerable()
                .Should()
                .Equal(1 * sec, 2 * sec, 3 * sec);
        }

        [Fact]
        public static void FromDurationsTest2()
        {
            var result = Schedule.FromDurations(
                Range(1, 5)
                    .Where(x => x % 2 == 0)
                    .Select<int, Duration>(x => x * seconds));
            result
                .AsEnumerable()
                .Should()
                .Equal(2 * sec, 4 * sec);
        }

        [Fact]
        public static void RecursTest()
        {
            var results = Schedule.Recurs(5);
            results
                .AsEnumerable()
                .Should()
                .HaveCount(5)
                .And
                .Contain(x => x == Duration.Zero);
        }

        [Fact]
        public static void SpacedTest()
        {
            var results = Schedule.Spaced(5 * sec);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .HaveCount(5)
                .And
                .OnlyContain(x => x == 5 * sec);
        }

        [Fact]
        public static void LinearTest()
        {
            var results = Schedule.Linear(1 * sec);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(1 * sec, 2 * sec, 3 * sec, 4 * sec, 5 * sec);
        }

        [Fact]
        public static void LinearTest2()
        {
            var results = Schedule.Linear(100 * ms, 2);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(100, 300, 500, 700, 900);
        }

        [Fact]
        public static void ExponentialTest()
        {
            var results = Schedule.Exponential(1 * sec);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(1 * sec, 2 * sec, 4 * sec, 8 * sec, 16 * sec);
        }

        [Fact]
        public static void ExponentialTest2()
        {
            var results = Schedule.Exponential(1 * sec, 3);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(1 * sec, 3 * sec, 9 * sec, 27 * sec, 81 * sec);
        }

        [Fact]
        public static void FibonacciTest()
        {
            var results = Schedule.Fibonacci(1 * sec);
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(1 * sec, 2 * sec, 3 * sec, 5 * sec, 8 * sec);
        }

        [Fact]
        public static void NoDelayOnFirstTest()
        {
            var transformer = Schedule.NoDelayOnFirst;
            var results = transformer(Schedule.Spaced(10 * sec));
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(0 * sec, 10 * sec, 10 * sec, 10 * sec, 10 * sec);
        }

        [Fact]
        public static void MaxDelayTest()
        {
            var transformer = Schedule.MaxDelay(25 * sec);
            var results = transformer(Schedule.Linear(10 * sec));
            results
                .AsEnumerable()
                .Take(5)
                .Max()
                .Should().Be(25 * sec);
        }

        [Fact]
        public static void MaxCumulativeDelayTest()
        {
            var transformer = Schedule.MaxCumulativeDelay(40 * sec);
            var results = transformer(Schedule.Linear(10 * sec));
            results
                .AsEnumerable()
                .ToSeq()
                .Should()
                .HaveCount(3)
                .And
                .Subject.Max()
                .Should().Be(30 * sec);
        }

        [Fact]
        public static void UnionTest()
        {
            var results = Schedule.Spaced(5 * sec).Union(Schedule.Exponential(1 * sec));
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(1 * sec, 2 * sec, 4 * sec, 5 * sec, 5 * sec);
        }

        [Fact]
        public static void IntersectTest()
        {
            var results = Schedule.Spaced(5 * sec).Intersect(Schedule.Exponential(1 * sec));
            results
                .AsEnumerable()
                .Take(5)
                .Should()
                .Equal(5 * sec, 5 * sec, 5 * sec, 8 * sec, 16 * sec);
        }

        [Fact]
        public static void AppendTest()
        {
            var results =
                Schedule.FromDurations(1 * sec, 2 * sec, 3 * sec) +
                Schedule.FromDurations(4 * sec, 5 * sec, 6 * sec);
            results
                .AsEnumerable()
                .Should()
                .Equal(1 * sec, 2 * sec, 3 * sec, 4 * sec, 5 * sec, 6 * sec);
        }

        [Pure]
        private static Seq<DateTime> FromDuration(Duration duration)
        {
            var now = DateTime.Now;
            return Range(0, (int)((TimeSpan)duration).TotalSeconds)
                .Select(i => now + TimeSpan.FromSeconds(i))
                .ToSeq();
        }

        [Pure]
        private static Seq<DateTime> FromDurations(Seq<Duration> durations)
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
            var results = Schedule.UpTo(5 * sec, FromDates(FromDuration(2 * min)));
            results
                .AsEnumerable()
                .Should()
                .Equal(0, 0, 0, 0);
        }

        [Fact]
        public static void FixedTest()
        {
            var results = Schedule.Fixed(5 * sec, FromDates(FromDurations(Seq<Duration>(
                6 * sec,
                1 * sec,
                4 * sec
            ))));
            results
                .AsEnumerable()
                .Take(3)
                .Should()
                .Equal(0, 4 * sec, 1 * sec);
        }

        [Fact]
        public static void WindowedTest()
        {
            var results = Schedule.Windowed(5 * sec, FromDates(FromDurations(Seq<Duration>(
                6 * sec,
                1 * sec,
                7 * sec
            ))));
            results
                .AsEnumerable()
                .Take(3)
                .Should()
                .Equal(4 * sec, 4 * sec, 3 * sec);
        }

        [Fact]
        public static void SecondOfMinuteTest()
        {
            var results = Schedule.SecondOfMinute(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 1, 26),
                new DateTime(2022, 1, 1, 1, 1, 1),
                new DateTime(2022, 1, 1, 1, 1, 47)
            )));
            results
                .AsEnumerable()
                .Take(3)
                .Should()
                .Equal(37 * sec, 2 * sec, 16 * sec);
        }

        [Fact]
        public static void MinuteOfHourTest()
        {
            var results = Schedule.MinuteOfHour(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 26, 0),
                new DateTime(2022, 1, 1, 1, 1, 0),
                new DateTime(2022, 1, 1, 1, 47, 0)
            )));
            results
                .AsEnumerable()
                .Take(3)
                .Should()
                .Equal(37 * min, 2 * min, 16 * min);
        }

        [Fact]
        public static void HourOfDayTest()
        {
            var results = Schedule.HourOfDay(3, FromDates(Seq(
                new DateTime(2022, 1, 1, 1, 0, 0),
                new DateTime(2022, 1, 1, 4, 0, 0),
                new DateTime(2022, 1, 1, 6, 0, 0),
                new DateTime(2022, 1, 1, 3, 0, 0)
            )));
            results
                .AsEnumerable()
                .Take(4)
                .Should()
                .Equal(2 * hours, 23 * hours, 21 * hour, 24 * hours);
        }

        [Fact]
        public static void DayOfWeekTest()
        {
            var results = Schedule.DayOfWeek(DayOfWeek.Wednesday, FromDates(Seq(
                new DateTime(2022, 1, 1, 0, 0, 0), // Saturday
                new DateTime(2022, 1, 3, 0, 0, 0), // Monday
                new DateTime(2022, 1, 7, 0, 0, 0), // Friday
                new DateTime(2022, 1, 5, 0, 0, 0) // Wednesday
            )));
            results
                .AsEnumerable()
                .Take(4)
                .Should()
                .Equal(4 * days, 2 * days, 5 * days, 7 * days);
        }

        private const int Seed = 98192732;

        [Fact]
        public static void JitterTest1()
        {
            var noJitter = (
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5)).AsEnumerable().ToSeq();
            var withJitter = (
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5)
                & Schedule.Jitter(1 * ms, 10 * ms)).AsEnumerable().ToSeq();
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
            var noJitter = (
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5)).AsEnumerable().ToSeq();
            var withJitter = (
                Schedule.Linear(10 * seconds)
                & Schedule.Recurs(5)
                & Schedule.Jitter(1.5)).AsEnumerable().ToSeq();
            withJitter.Should()
                .HaveCount(5)
                .And
                .Subject.Zip(noJitter)
                .Should()
                .Contain(x => x.Item1 > x.Item2)
                .And
                .Contain(x => x.Item1 - x.Item2 <= x.Item2 * 1.5);
        }

        [Fact]
        public static void DecorrelatedTest()
        {
            var schedule = Schedule.Linear(10 * sec) | Schedule.Decorrelate(seed: Seed);
            var result = schedule.Take(5).ToSeq();
            result.Zip(result.Skip(1))
                .Should()
                .Contain(x => x.Left > x.Right);
        }

        [Fact]
        public static void ResetAfterTest()
        {
            var results =
                Schedule.Linear(10 * sec)
                | Schedule.ResetAfter(25 * sec);
            results
                .AsEnumerable()
                .Take(4)
                .Should()
                .Equal(10 * sec, 20 * sec, 10 * sec, 20 * sec);
        }

        [Fact]
        public static void RepeatTest()
        {
            var results =
                Schedule.FromDurations(1 * sec, 5 * sec, 20 * sec)
                | Schedule.Repeat(3);
            results
                .AsEnumerable()
                .Should()
                .HaveCount(9)
                .And
                .Equal(1 * sec, 5 * sec, 20 * sec,
                    1 * sec, 5 * sec, 20 * sec,
                    1 * sec, 5 * sec, 20 * sec);
        }
    }
}