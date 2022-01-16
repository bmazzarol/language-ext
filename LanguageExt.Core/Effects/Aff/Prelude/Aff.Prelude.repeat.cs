using System;
using LanguageExt.Effects.Traits;

namespace LanguageExt
{
    public static partial class Prelude
    {
        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeat<RT, A>(Aff<RT, A> ma) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeat<A>(Aff<A> ma)
            => ScheduleAff<A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeat<RT, A>(Schedule<RT, A> schedule, Aff<RT, A> ma) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeat<RT, A>(Schedule<A> schedule, Aff<RT, A> ma) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeat<RT, A>(Schedule schedule, Aff<RT, A> ma) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeat<A>(Schedule<A> schedule, Aff<A> ma)
            => ScheduleAff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeat<A>(Schedule schedule, Aff<A> ma)
            => ScheduleAff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatWhile<RT, A>(Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatWhile<A>(Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatWhile<RT, A>(Schedule<RT, A> schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatWhile<RT, A>(Schedule<A> schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatWhile<RT, A>(Schedule schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatWhile<A>(Schedule<A> schedule, Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatWhile<A>(Schedule schedule, Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatUntil<RT, A>(Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatUntil<A>(Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatUntil<RT, A>(Schedule<RT, A> schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatUntil<RT, A>(Schedule<A> schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> repeatUntil<RT, A>(Schedule schedule, Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatUntil<A>(Schedule<A> schedule, Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> repeatUntil<A>(Schedule schedule, Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, schedule, predicate);
    }
}
