using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class EffScheduleTests
    {
        [Fact]
        public static void RepeatRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .Repeat(TestSchedule)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount);
            result.Should().Be(1);
        }

        [Fact]
        public static void RepeatNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .Repeat(TestSchedule)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount);
            result.Should().Be(1);
        }

        [Fact]
        public static void RepeatWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .RepeatWhile(TestSchedule, a => a == 2)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void RepeatWhileFailureRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailureRuntime(counter)
                .RepeatWhile(TestSchedule, a => a == 2)
                .Run(Runtime.New());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static void RepeatWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .RepeatWhile(TestSchedule, a => a == 2)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void RepeatUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .RepeatUntil(TestSchedule, a => a == 1)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void RepeatUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .RepeatUntil(TestSchedule, a => a == 1)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }
    }
}
