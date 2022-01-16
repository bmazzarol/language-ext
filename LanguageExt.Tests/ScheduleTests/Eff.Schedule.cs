using System;
using LanguageExt.Common;
using LanguageExt.Sys.Test;
using static LanguageExt.Prelude;

namespace LanguageExt.Tests.ScheduleTests
{
    public static partial class EffScheduleTests
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
        
        private static Eff<int> TestSuccess(Counter counter)
            => Eff<int>.Success(1)
                .Do(x =>
                {
                    counter.Count += x;
                    return unit;
                });

        private static Eff<Runtime, int> TestSuccessRuntime(Counter counter)
            => Eff<Runtime, int>.Success(1)
                .Do(x =>
                {
                    counter.Count += x;
                    return unit;
                });

        private static Eff<int> TestFailure(Counter counter)
            => Eff<int>.Fail(TestError) | @catch(e =>
            {
                counter.Count += 1;
                return Eff<int>.Fail(e);
            });

        private static Eff<Runtime, int> TestFailureRuntime(Counter counter)
            => Eff<int>.Fail(TestError) | @catch(e =>
            {
                counter.Count += 1;
                return Eff<Runtime, int>.Fail(e);
            });
    }
}
