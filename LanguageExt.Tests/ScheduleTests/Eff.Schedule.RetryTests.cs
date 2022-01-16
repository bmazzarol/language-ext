using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class EffScheduleTests
    {
        [Fact]
        public static void RetryRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailureRuntime(counter)
                .Retry(TestSchedule)
                .Run(Runtime.New());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static void RetryNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailure(counter)
                .Retry(TestSchedule)
                .Run();
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static void RetryWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailureRuntime(counter)
                .RetryWhile(TestSchedule, _ => false)
                .Run(Runtime.New());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static void RetryWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailure(counter)
                .RetryWhile(TestSchedule, _ => false)
                .Run();
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static void RetryUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailureRuntime(counter)
                .RetryUntil(TestSchedule, _ => false)
                .Run(Runtime.New());
            result.IsFail.Should().BeTrue();
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static void RetryUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = TestFailure(counter)
                .RetryUntil(TestSchedule, _ => true)
                .Run();
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }
    }
}
