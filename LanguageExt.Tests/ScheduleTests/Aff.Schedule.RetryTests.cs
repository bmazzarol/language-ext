using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class AffScheduleTests
    {
        [Fact]
        public static async Task RetryRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailureRuntime(counter)
                .Retry(TestSchedule)
                .Run(Runtime.New()));
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static async Task RetryNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailure(counter)
                .Retry(TestSchedule)
                .Run());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static async Task RetryWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailureRuntime(counter)
                .RetryWhile(TestSchedule, _ => false)
                .Run(Runtime.New()));
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static async Task RetryWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailure(counter)
                .RetryWhile(TestSchedule, _ => false)
                .Run());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static async Task RetryUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailureRuntime(counter)
                .RetryUntil(TestSchedule, _ => false)
                .Run(Runtime.New()));
            result.IsFail.Should().BeTrue();
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(ExpectedRunCount);
        }

        [Fact]
        public static async Task RetryUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailure(counter)
                .RetryUntil(TestSchedule, _ => true)
                .Run());
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }
    }
}
