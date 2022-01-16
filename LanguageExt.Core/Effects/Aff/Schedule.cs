using System;
using LanguageExt.Common;
using LanguageExt.Core.Extensions;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    internal static class ScheduleAff<RT, A> where RT : struct, HasCancel<RT>
    {
        private static Aff<RT, S> Run<S>(
            Aff<RT, A> ma, S state, Schedule<RT, A> schedule, Func<S, A, S> fold, Func<Fin<A>, bool> pred)
            => AffMaybe<RT, S>(async env =>
            {
                var finalState = await schedule.Run(env, ma).FoldWhile(state, fold.LiftFoldFn(), pred.LiftPredFn());
                var cachedResult = await ma.Run(env);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        private static Aff<RT, A> Run(Aff<RT, A> ma, Schedule<RT, A> schedule, Func<Fin<A>, bool> pred)
            => Run(ma, default(A), schedule, static (_, x) => x, pred);

        public static Aff<RT, A> Repeat(Aff<RT, A> ma, Schedule<RT, A> schedule)
            => Run(ma, schedule, static x => x.IsSucc);

        public static Aff<RT, A> Repeat(Aff<RT, A> ma, Schedule<A> schedule)
            => Repeat(ma, Schedule<RT, A>.Widen(schedule));

        public static Aff<RT, A> Repeat(Aff<RT, A> ma, Schedule schedule)
            => Repeat(ma, Schedule<RT, A>.Widen(schedule));

        public static Aff<RT, A> Retry(Aff<RT, A> ma, Schedule<RT, A> schedule)
            => Run(ma, schedule, static x => x.IsFail);

        public static Aff<RT, A> Retry(Aff<RT, A> ma, Schedule<A> schedule)
            => Retry(ma, Schedule<RT, A>.Widen(schedule));

        public static Aff<RT, A> Retry(Aff<RT, A> ma, Schedule schedule)
            => Retry(ma, Schedule<RT, A>.Widen(schedule));

        public static Aff<RT, A> RepeatWhile(Aff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && pred((A)x));

        public static Aff<RT, A> RepeatWhile(Aff<RT, A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RepeatWhile(Aff<RT, A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RetryWhile(Aff<RT, A> ma, Schedule<RT, A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && pred((Error)x));

        public static Aff<RT, A> RetryWhile(Aff<RT, A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RetryWhile(Aff<RT, A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RepeatUntil(Aff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && !pred((A)x));

        public static Aff<RT, A> RepeatUntil(Aff<RT, A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RepeatUntil(Aff<RT, A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RetryUntil(Aff<RT, A> ma, Schedule<RT, A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && !pred((Error)x));

        public static Aff<RT, A> RetryUntil(Aff<RT, A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, A> RetryUntil(Aff<RT, A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Aff<RT, S> Fold<S>(Aff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold)
            => Run(ma, state, schedule, fold, static x => x.IsSucc);

        public static Aff<RT, S> Fold<S>(Aff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<RT, A>.Widen(schedule), state, fold);

        public static Aff<RT, S> Fold<S>(Aff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<RT, A>.Widen(schedule), state, fold);

        public static Aff<RT, S> FoldWhile<S>(
            Aff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && pred((A)x));

        public static Aff<RT, S> FoldWhile<S>(
            Aff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Aff<RT, S> FoldWhile<S>(
            Aff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Aff<RT, S> FoldUntil<S>(
            Aff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && !pred((A)x));

        public static Aff<RT, S> FoldUntil<S>(
            Aff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Aff<RT, S> FoldUntil<S>(
            Aff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);
    }

    internal static class ScheduleAff<A>
    {
        private static Aff<S> Run<S>(
            Aff<A> ma, S state, Schedule<A> schedule, Func<S, A, S> fold, Func<Fin<A>, bool> pred) =>
            AffMaybe(async () =>
            {
                var finalState = await schedule.Run(ma).FoldWhile(state, fold.LiftFoldFn(), pred.LiftPredFn());
                var cachedResult = await ma.Run();
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        private static Aff<A> Run(Aff<A> ma, Schedule<A> schedule, Func<Fin<A>, bool> pred)
            => Run(ma, default(A), schedule, static (_, x) => x, pred);

        public static Aff<A> Repeat(Aff<A> ma, Schedule<A> schedule)
            => Run(ma, schedule, static x => x.IsSucc);

        public static Aff<A> Repeat(Aff<A> ma, Schedule schedule)
            => Repeat(ma, Schedule<A>.Widen(schedule));

        public static Aff<A> Retry(Aff<A> ma, Schedule<A> schedule)
            => Run(ma, schedule, static x => x.IsFail);

        public static Aff<A> Retry(Aff<A> ma, Schedule schedule)
            => Retry(ma, Schedule<A>.Widen(schedule));

        public static Aff<A> RepeatWhile(Aff<A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && pred((A)x));

        public static Aff<A> RepeatWhile(Aff<A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<A>.Widen(schedule), pred);

        public static Aff<A> RetryWhile(Aff<A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && pred((Error)x));

        public static Aff<A> RetryWhile(Aff<A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<A>.Widen(schedule), pred);

        public static Aff<A> RepeatUntil(Aff<A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && !pred((A)x));

        public static Aff<A> RepeatUntil(Aff<A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<A>.Widen(schedule), pred);

        public static Aff<A> RetryUntil(Aff<A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && !pred((Error)x));

        public static Aff<A> RetryUntil(Aff<A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<A>.Widen(schedule), pred);

        public static Aff<S> Fold<S>(Aff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold)
            => Run(ma, state, schedule, fold, static x => x.IsSucc);

        public static Aff<S> Fold<S>(Aff<A> ma, Schedule schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<A>.Widen(schedule), state, fold);

        public static Aff<S> FoldWhile<S>(
            Aff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && pred((A)x));

        public static Aff<S> FoldWhile<S>(
            Aff<A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<A>.Widen(schedule), state, fold, pred);

        public static Aff<S> FoldUntil<S>(
            Aff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && !pred((A)x));

        public static Aff<S> FoldUntil<S>(
            Aff<A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<A>.Widen(schedule), state, fold, pred);
    }
}
