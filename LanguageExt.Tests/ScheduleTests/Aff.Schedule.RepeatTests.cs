using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class AffScheduleTests
    {
        [Fact]
        public static async Task RepeatRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .Repeat(TestSchedule)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount);
            result.Should().Be(1);
        }

        [Fact]
        public static async Task RepeatNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .Repeat(TestSchedule)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount);
            result.Should().Be(1);
        }

        [Fact]
        public static async Task RepeatWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .RepeatWhile(TestSchedule, a => a == 2)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task RepeatWhileFailureRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestFailureRuntime(counter)
                .RepeatWhile(TestSchedule, a => a == 2)
                .Run(Runtime.New()));
            result.IsFail.Should().BeTrue();
            counter.Count.Should().Be(1);
        }

        [Fact]
        public static async Task RepeatWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .RepeatWhile(TestSchedule, a => a == 2)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task RepeatUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .RepeatUntil(TestSchedule, a => a == 1)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task RepeatUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .RepeatUntil(TestSchedule, a => a == 1)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }
    }
}
