using System;
using LanguageExt.Common;
using LanguageExt.Sys.Test;
using static LanguageExt.Prelude;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class AffScheduleTests
    {
        private const int NumberOfRetries = 5;
        private const int ExpectedRunCount = NumberOfRetries + 1;
        
        private static readonly Schedule TestSchedule =
            Schedule.Recurs(NumberOfRetries)
            | Schedule.Spaced(TimeSpan.FromMilliseconds(1))
            | Schedule.NoDelayOnFirstRetry();

        private static readonly Error TestError = Error.New(1, "Some Error");

        private sealed class Counter
        {
            public int Count { get; set; }

            public static Counter New() => new() { Count = 0 };
        }
        
        private static Aff<int> TestSuccess(Counter counter)
            => Aff<int>.Success(1)
                .Do(x =>
                {
                    counter.Count += x;
                    return unit;
                });

        private static Aff<Runtime, int> TestSuccessRuntime(Counter counter)
            => Aff<Runtime, int>.Success(1)
                .Do(x =>
                {
                    counter.Count += x;
                    return unit;
                });

        private static Aff<int> TestFailure(Counter counter)
            => Aff<int>.Fail(TestError) | @catch(e =>
            {
                counter.Count += 1;
                return Aff<int>.Fail(e);
            });

        private static Aff<Runtime, int> TestFailureRuntime(Counter counter)
            => Aff<int>.Fail(TestError) | @catch(e =>
            {
                counter.Count += 1;
                return Aff<Runtime, int>.Fail(e);
            });
    }
}
