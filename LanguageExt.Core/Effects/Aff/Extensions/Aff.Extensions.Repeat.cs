using System;
using LanguageExt.Effects.Traits;

namespace LanguageExt
{
    public static partial class AffExtensions
    {
        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> Repeat<RT, A>(this Aff<RT, A> ma) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> Repeat<A>(this Aff<A> ma)
            => ScheduleAff<A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> Repeat<RT, A>(this Aff<RT, A> ma, Schedule<RT, A> schedule)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> Repeat<RT, A>(this Aff<RT, A> ma, Schedule<A> schedule)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> Repeat<RT, A>(this Aff<RT, A> ma, Schedule schedule) where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> Repeat<A>(this Aff<A> ma, Schedule<A> schedule)
            => ScheduleAff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> Repeat<A>(this Aff<A> ma, Schedule schedule)
            => ScheduleAff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> RepeatWhile<RT, A>(this Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatWhile<A>(this Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> RepeatWhile<RT, A>(
            this Aff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> predicate)
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
        public static Aff<RT, A> RepeatWhile<RT, A>(this Aff<RT, A> ma, Schedule<A> schedule, Func<A, bool> predicate)
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
        public static Aff<RT, A> RepeatWhile<RT, A>(this Aff<RT, A> ma, Schedule schedule, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatWhile<A>(this Aff<A> ma, Schedule<A> schedule, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatWhile<A>(this Aff<A> ma, Schedule schedule, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> RepeatUntil<RT, A>(this Aff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatUntil<A>(this Aff<A> ma, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<RT, A> RepeatUntil<RT, A>(
            this Aff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> predicate)
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
        public static Aff<RT, A> RepeatUntil<RT, A>(this Aff<RT, A> ma, Schedule<A> schedule, Func<A, bool> predicate)
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
        public static Aff<RT, A> RepeatUntil<RT, A>(this Aff<RT, A> ma, Schedule schedule, Func<A, bool> predicate)
            where RT : struct, HasCancel<RT>
            => ScheduleAff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatUntil<A>(this Aff<A> ma, Schedule<A> schedule, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Aff<A> RepeatUntil<A>(this Aff<A> ma, Schedule schedule, Func<A, bool> predicate)
            => ScheduleAff<A>.RepeatUntil(ma, schedule, predicate);
    }
}
