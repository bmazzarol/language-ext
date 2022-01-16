using System;

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
        public static Eff<RT, A> repeat<RT, A>(Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeat<A>(Eff<A> ma)
            => ScheduleEff<A>.Repeat(ma, Schedule.Forever);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeat<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeat<RT, A>(Schedule<A> schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeat<RT, A>(Schedule schedule, Eff<RT, A> ma) where RT : struct
            => ScheduleEff<RT, A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeat<A>(Schedule<A> schedule, Eff<A> ma)
            => ScheduleEff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeat<A>(Schedule schedule, Eff<A> ma)
            => ScheduleEff<A>.Repeat(ma, schedule);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatWhile<RT, A>(Eff<RT, A> ma, Func<A, bool> predicate) where RT : struct
            => ScheduleEff<RT, A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatWhile<A>(Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatWhile(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatWhile<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatWhile<RT, A>(Schedule<A> schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatWhile<RT, A>(Schedule schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatWhile<A>(Schedule<A> schedule, Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns false
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatWhile<A>(Schedule schedule, Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatWhile(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatUntil<RT, A>(Eff<RT, A> ma, Func<A, bool> predicate) where RT : struct
            => ScheduleEff<RT, A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatUntil<A>(Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatUntil(ma, Schedule.Forever, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatUntil<RT, A>(Schedule<RT, A> schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatUntil<RT, A>(Schedule<A> schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<RT, A> repeatUntil<RT, A>(Schedule schedule, Eff<RT, A> ma, Func<A, bool> predicate)
            where RT : struct
            => ScheduleEff<RT, A>.RepeatUntil(ma, schedule, predicate);

        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatUntil<A>(Schedule<A> schedule, Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatUntil(ma, schedule, predicate);
        
        /// <summary>
        /// Keeps repeating the computation until it fails or the predicate returns true  
        /// </summary>
        /// <param name="schedule">Scheduler strategy for repeating</param>
        /// <param name="ma">Computation to repeat</param>
        /// <typeparam name="RT">Runtime</typeparam>
        /// <typeparam name="A">Computation bound value type</typeparam>
        /// <returns>The result of the last invocation of ma</returns>
        public static Eff<A> repeatUntil<A>(Schedule schedule, Eff<A> ma, Func<A, bool> predicate)
            => ScheduleEff<A>.RepeatUntil(ma, schedule, predicate);
    }
}
