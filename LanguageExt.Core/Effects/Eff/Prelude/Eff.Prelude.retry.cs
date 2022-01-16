using System;
using LanguageExt.Common;

namespace LanguageExt
{
    public static partial class Prelude
    {
        /// <summary>
        /// Keeps retrying the computation   
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retry<RT, A>(Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Retry(ma, Schedule.Forever);

        /// <summary>
        /// Keeps retrying the computation 
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retry<A>(Eff<A> ma)
            => ScheduleEff<A>.Retry(ma, Schedule.Forever);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retry<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Retry(ma, schedule);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retry<RT, A>(Schedule<A> schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Retry(ma, schedule);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retry<RT, A>(Schedule schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Retry(ma, schedule);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires   
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retry<A>(Schedule<A> schedule, Eff<A> ma)
            => ScheduleEff<A>.Retry(ma, schedule);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires   
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retry<A>(Schedule schedule, Eff<A> ma)
            => ScheduleEff<A>.Retry(ma, schedule);

        /// <summary>
        /// Keeps retrying the computation until the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryWhile<RT, A>(Eff<RT, A> ma, Func<Error, bool> predicate) where RT : struct
            => ScheduleEff<RT, A>.RetryWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps retrying the computation until the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryWhile<A>(Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryWhile<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryWhile<RT, A>(Schedule<A> schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryWhile<RT, A>(Schedule schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryWhile<A>(Schedule<A> schedule, Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryWhile<A>(Schedule schedule, Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation until the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryUntil<RT, A>(Eff<RT, A> ma, Func<Error, bool> predicate) where RT : struct
            => ScheduleEff<RT, A>.RetryUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps retrying the computation until the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryUntil<A>(Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryUntil<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryUntil<RT, A>(Schedule<A> schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> retryUntil<RT, A>(Schedule schedule, Eff<RT, A> ma, Func<Error, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RetryUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns true  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryUntil<A>(Schedule<A> schedule, Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps retrying the computation, until the scheduler expires, or the predicate returns true  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for retrying</param>
        /// <param name="ma">Computation to retry</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> retryUntil<A>(Schedule schedule, Eff<A> ma, Func<Error, bool> predicate)
            => ScheduleEff<A>.RetryUntil(ma, schedule, predicate);
    }
}
