using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class EffScheduleTests
    {
        [Fact]
        public static void FoldRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .Fold(TestSchedule, 0, (a, b) => a + b)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount).And.Be(result);
        }

        [Fact]
        public static void FoldNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .Fold(TestSchedule, 0, (a, b) => a + b)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount).And.Be(result);
        }

        [Fact]
        public static void FoldWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .FoldWhile(TestSchedule, 0, (a, b) => a + b, a => a == 2)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void FoldWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .FoldWhile(TestSchedule, 0, (a, b) => a + b, a => a == 2)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void FoldUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccessRuntime(counter)
                .FoldUntil(TestSchedule, 0, (a, b) => a + b, a => a == 1)
                .Run(Runtime.New())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static void FoldUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestSuccess(counter)
                .FoldUntil(TestSchedule, 0, (a, b) => a + b, a => a == 1)
                .Run()
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }
    }
}
