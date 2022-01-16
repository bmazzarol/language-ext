using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.Sys.Test;
using Xunit;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class AffScheduleTests
    {
        [Fact]
        public static async Task FoldRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .Fold(TestSchedule, 0, (a, b) => a + b)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount).And.Be(result);
        }

        [Fact]
        public static async Task FoldNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .Fold(TestSchedule, 0, (a, b) => a + b)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(ExpectedRunCount).And.Be(result);
        }

        [Fact]
        public static async Task FoldWhileRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .FoldWhile(TestSchedule, 0, (a, b) => a + b, a => a == 2)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task FoldWhileNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .FoldWhile(TestSchedule, 0, (a, b) => a + b, a => a == 2)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task FoldUntilRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccessRuntime(counter)
                    .FoldUntil(TestSchedule, 0, (a, b) => a + b, a => a == 1)
                    .Run(Runtime.New()))
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }

        [Fact]
        public static async Task FoldUntilNoRuntimeTest()
        {
            var counter = Counter.New();
            var result = (await TestSuccess(counter)
                    .FoldUntil(TestSchedule, 0, (a, b) => a + b, a => a == 1)
                    .Run())
                .ThrowIfFail();
            counter.Count.Should().Be(1);
            result.Should().Be(0);
        }
    }
}
