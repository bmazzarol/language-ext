using Xunit;
using System;
using LanguageExt;
using LanguageExt.Common;
using System.Threading.Tasks;
using FluentAssertions;
using static LanguageExt.Prelude;

namespace AffTests
{
    public class AffMapFailTests
    {
        [Fact] // Fails
        public async Task Aff_WithFailingTaskInAff_EndsWithMapFailValue()
        {
            // Arrange
            var affWithException = Aff(async () => await FailingTask());
            var affWithMappedFailState = affWithException.MapFail(error => Error.New(error.Message + "MapFail Aff"));

            // Act
            var fin = await affWithMappedFailState.Run();
            var result = fin.Match(_ => "Success!", error => error.Message);

            // Assert
            Assert.True(result.IndexOf("MapFail Aff", StringComparison.Ordinal) > 0); 
        }

        [Fact] // Fails
        public async Task Aff_WithExceptionInAff_EndsWithMapFailValue()
        {
            // Arrange
            var affWithException = Aff<Unit>(() => throw new Exception());
            var affWithMappedFailState = affWithException.MapFail(error => Error.New(error.Message + "MapFail Aff"));

            // Act
            var fin = await affWithMappedFailState.Run();
            var result = fin.Match(_ => "Success!", error => error.Message);

            // Assert
            Assert.True(result.IndexOf("MapFail Aff", StringComparison.Ordinal) > 0); 
        }

        [Fact] // Fails
        public async Task Aff_FailingTaskToAff_EndsWithMapFailValue()
        {
            // Arrange
            var affWithException = FailingTask().ToAff();
            var affWithMappedFailState = affWithException.MapFail(error => Error.New(error.Message + "MapFail Aff"));

            // Act
            var fin = await affWithMappedFailState.Run();
            var result = fin.Match(_ => "Success!", error => error.Message);

            // Assert
            Assert.True(result.IndexOf("MapFail Aff", StringComparison.Ordinal) > 0); 
        }

        [Fact] // Succeeds
        public async Task EffToAff_WithExceptionInEff_EndsWithMapFailValue()
        {
            // Arrange
            var effWithException = Eff<Unit>(() => throw new Exception("Error!"));
            var affWithException = effWithException.ToAff();

            var affWithMappedFailState = affWithException.MapFail(error => Error.New(error.Message + "MapFail Aff"));

            // Act
            var fin = await affWithMappedFailState.Run();
            var result = fin.Match(_ => "Success!", error => error.Message);

            // Assert
            Assert.True(result.IndexOf("MapFail Aff", StringComparison.Ordinal) > 0); 
        }

        private static async Task<Unit> FailingTask()
        {
            await Task.Delay(1);
            throw new Exception("Error!");
        }

        [Fact(DisplayName = "Fork against Aff<T> can be cancelled")]
        public static void CancelForkTest()
        {
            var counter = Atom(0);
            Aff<int> EffectToFork() =>
                AffMaybe<int>(async () =>
                {
                    swap(counter, i => i + 1);
                    return await counter.Value.AsValueTask();
                }).Repeat(Schedule.Forever);

            var daemonEffect = EffectToFork().Fork();
            
            // start the effect
            var cancelEffect = daemonEffect.Run().ThrowIfFail();

            // cancel the running effect
            cancelEffect.RunUnit();
            
            counter.Value.Should().BeGreaterThan(0);
        }
    }
}
