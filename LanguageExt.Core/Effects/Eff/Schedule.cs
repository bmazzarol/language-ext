using System;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    internal static class ScheduleEff<RT, A> where RT : struct
    {
        private static Eff<RT, S> Run<S>(
            Eff<RT, A> ma, S state, Schedule<RT, A> schedule, Func<S, A, S> fold, Func<Fin<A>, bool> pred)
            => EffMaybe<RT, S>(env =>
            {
                var finalState = schedule.Run(env, ma).FoldWhile(state, fold.LiftFoldFn(), pred.LiftPredFn());
                var cachedResult = ma.Run(env);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        private static Eff<RT, A> Run(Eff<RT, A> ma, Schedule<RT, A> schedule, Func<Fin<A>, bool> pred)
            => Run(ma, default(A), schedule, static (_, x) => x, pred);

        public static Eff<RT, A> Repeat(Eff<RT, A> ma, Schedule<RT, A> schedule)
            => Run(ma, schedule, static x => x.IsSucc);

        public static Eff<RT, A> Repeat(Eff<RT, A> ma, Schedule<A> schedule)
            => Repeat(ma, Schedule<RT, A>.Widen(schedule));

        public static Eff<RT, A> Repeat(Eff<RT, A> ma, Schedule schedule)
            => Repeat(ma, Schedule<RT, A>.Widen(schedule));

        public static Eff<RT, A> Retry(Eff<RT, A> ma, Schedule<RT, A> schedule)
            => Run(ma, schedule, static x => x.IsFail);

        public static Eff<RT, A> Retry(Eff<RT, A> ma, Schedule<A> schedule)
            => Retry(ma, Schedule<RT, A>.Widen(schedule));

        public static Eff<RT, A> Retry(Eff<RT, A> ma, Schedule schedule)
            => Retry(ma, Schedule<RT, A>.Widen(schedule));

        public static Eff<RT, A> RepeatWhile(Eff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && pred((A)x));

        public static Eff<RT, A> RepeatWhile(Eff<RT, A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RepeatWhile(Eff<RT, A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RetryWhile(Eff<RT, A> ma, Schedule<RT, A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && pred((Error)x));

        public static Eff<RT, A> RetryWhile(Eff<RT, A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RetryWhile(Eff<RT, A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RepeatUntil(Eff<RT, A> ma, Schedule<RT, A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && !pred((A)x));

        public static Eff<RT, A> RepeatUntil(Eff<RT, A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RepeatUntil(Eff<RT, A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RetryUntil(Eff<RT, A> ma, Schedule<RT, A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && !pred((Error)x));

        public static Eff<RT, A> RetryUntil(Eff<RT, A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, A> RetryUntil(Eff<RT, A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<RT, A>.Widen(schedule), pred);

        public static Eff<RT, S> Fold<S>(Eff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold)
            => Run(ma, state, schedule, fold, static x => x.IsSucc);

        public static Eff<RT, S> Fold<S>(Eff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<RT, A>.Widen(schedule), state, fold);

        public static Eff<RT, S> Fold<S>(Eff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<RT, A>.Widen(schedule), state, fold);

        public static Eff<RT, S> FoldWhile<S>(
            Eff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && pred((A)x));

        public static Eff<RT, S> FoldWhile<S>(
            Eff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Eff<RT, S> FoldWhile<S>(
            Eff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Eff<RT, S> FoldUntil<S>(
            Eff<RT, A> ma, Schedule<RT, A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && !pred((A)x));

        public static Eff<RT, S> FoldUntil<S>(
            Eff<RT, A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);

        public static Eff<RT, S> FoldUntil<S>(
            Eff<RT, A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<RT, A>.Widen(schedule), state, fold, pred);
    }

    internal static class ScheduleEff<A>
    {
        private static Eff<S> Run<S>(Eff<A> ma, S state, Schedule<A> schedule, Func<S, A, S> fold,
            Func<Fin<A>, bool> pred)
            => EffMaybe(() =>
            {
                var finalState = schedule.Run(ma).FoldWhile(state, fold.LiftFoldFn(), pred.LiftPredFn());
                var cachedResult = ma.Run();
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        private static Eff<A> Run(Eff<A> ma, Schedule<A> schedule, Func<Fin<A>, bool> pred)
            => Run(ma, default(A), schedule, static (_, x) => x, pred);

        public static Eff<A> Repeat(Eff<A> ma, Schedule<A> schedule)
            => Run(ma, schedule, static x => x.IsSucc);

        public static Eff<A> Repeat(Eff<A> ma, Schedule schedule)
            => Repeat(ma, Schedule<A>.Widen(schedule));

        public static Eff<A> Retry(Eff<A> ma, Schedule<A> schedule)
            => Run(ma, schedule, static x => x.IsFail);

        public static Eff<A> Retry(Eff<A> ma, Schedule schedule)
            => Retry(ma, Schedule<A>.Widen(schedule));

        public static Eff<A> RepeatWhile(Eff<A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && pred((A)x));

        public static Eff<A> RepeatWhile(Eff<A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatWhile(ma, Schedule<A>.Widen(schedule), pred);

        public static Eff<A> RetryWhile(Eff<A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && pred((Error)x));

        public static Eff<A> RetryWhile(Eff<A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryWhile(ma, Schedule<A>.Widen(schedule), pred);

        public static Eff<A> RepeatUntil(Eff<A> ma, Schedule<A> schedule, Func<A, bool> pred)
            => Run(ma, schedule, x => x.IsSucc && !pred((A)x));

        public static Eff<A> RepeatUntil(Eff<A> ma, Schedule schedule, Func<A, bool> pred)
            => RepeatUntil(ma, Schedule<A>.Widen(schedule), pred);

        public static Eff<A> RetryUntil(Eff<A> ma, Schedule<A> schedule, Func<Error, bool> pred)
            => Run(ma, schedule, x => x.IsFail && !pred((Error)x));

        public static Eff<A> RetryUntil(Eff<A> ma, Schedule schedule, Func<Error, bool> pred)
            => RetryUntil(ma, Schedule<A>.Widen(schedule), pred);

        public static Eff<S> Fold<S>(Eff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold)
            => Run(ma, state, schedule, fold, static x => x.IsSucc);

        public static Eff<S> Fold<S>(Eff<A> ma, Schedule schedule, S state, Func<S, A, S> fold)
            => Fold(ma, Schedule<A>.Widen(schedule), state, fold);

        public static Eff<S> FoldWhile<S>(
            Eff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && pred((A)x));

        public static Eff<S> FoldWhile<S>(
            Eff<A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldWhile(ma, Schedule<A>.Widen(schedule), state, fold, pred);

        public static Eff<S> FoldUntil<S>(
            Eff<A> ma, Schedule<A> schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => Run(ma, state, schedule, fold, x => x.IsSucc && !pred((A)x));

        public static Eff<S> FoldUntil<S>(
            Eff<A> ma, Schedule schedule, S state, Func<S, A, S> fold, Func<A, bool> pred)
            => FoldUntil(ma, Schedule<A>.Widen(schedule), state, fold, pred);
    }
}
