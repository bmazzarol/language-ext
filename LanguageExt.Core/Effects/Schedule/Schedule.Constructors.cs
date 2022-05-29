using System;
using System.Diagnostics.Contracts;
using LanguageExt.Effects.Traits;

namespace LanguageExt.Next
{
    public static partial class Schedule
    {
        /// <summary>
        /// Creates a schedule that always returns a constant result.
        /// </summary>
        /// <param name="result">result to return</param>
        [Pure]
        private static Sched Constant(ScheduleResult result)
            => new ConstSched(_ => result);

        /// <summary>
        /// Creates a schedule from a provided step function.
        /// </summary>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched New(Func<ScheduleContext, ScheduleResult> stepFunction)
            => new ConstSched(stepFunction);

        /// <summary>
        /// Creates a schedule from a provided step function.
        /// </summary>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<A>(Func<Fin<A>, ScheduleContext, Eff<ScheduleResult>> stepFunction)
            => new EffSched<A>(stepFunction);

        /// <summary>
        /// Creates a schedule from a provided step function.
        /// </summary>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<A>(Func<Fin<A>, ScheduleContext, Aff<ScheduleResult>> stepFunction)
            => new AffSched<A>(stepFunction);

        /// <summary>
        /// Creates a schedule from a provided step function.
        /// </summary>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<RT, A>(Func<Fin<A>, ScheduleContext, Eff<RT, ScheduleResult>> stepFunction)
            where RT : struct, HasCancel<RT> => new EffSched<RT, A>(stepFunction);

        /// <summary>
        /// Creates a schedule from a provided step function.
        /// </summary>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<RT, A>(Func<Fin<A>, ScheduleContext, Aff<RT, ScheduleResult>> stepFunction)
            where RT : struct, HasCancel<RT> => new AffSched<RT, A>(stepFunction);

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched New<S>(S state, Func<S, ScheduleContext, ScheduleResult> stepFunction)
            => new ConstSched(context => stepFunction(state, context));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(S state, Func<S, Fin<A>, ScheduleContext, Eff<ScheduleResult>> stepFunction)
            => new EffSched<A>((result, context) => stepFunction(state, result, context));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(S state, Func<S, Fin<A>, ScheduleContext, Aff<ScheduleResult>> stepFunction)
            => new AffSched<A>((result, context) => stepFunction(state, result, context));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Eff<RT, ScheduleResult>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new EffSched<RT, A>((result, context) => stepFunction(state, result, context));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Aff<RT, ScheduleResult>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new AffSched<RT, A>((result, context) => stepFunction(state, result, context));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <remarks>This schedule will fold over the state and mutate it</remarks>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched New<S>(
            S state, Func<S, ScheduleContext, (ScheduleResult Result, S State)> stepFunction)
            => new ConstSched(context =>
            {
                var (r, s) = stepFunction(state, context);
                state = s;
                return r;
            });

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <remarks>This schedule will fold over the state and mutate it</remarks>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Eff<(ScheduleResult Result, S State)>> stepFunction)
            => new EffSched<A>((result, context) => stepFunction(state, result, context)
                .Map(t =>
                {
                    state = t.State;
                    return t.Result;
                }));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <remarks>This schedule will fold over the state and mutate it</remarks>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Aff<(ScheduleResult Result, S State)>> stepFunction)
            => new AffSched<A>((result, context) => stepFunction(state, result, context)
                .Map(t =>
                {
                    state = t.State;
                    return t.Result;
                }));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <remarks>This schedule will fold over the state and mutate it</remarks>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Eff<RT, (ScheduleResult Result, S State)>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new EffSched<RT, A>((result, context) => stepFunction(state, result, context)
                .Map(t =>
                {
                    state = t.State;
                    return t.Result;
                }));

        /// <summary>
        /// Creates a schedule from the provided step function with additional user provided state. 
        /// </summary>
        /// <remarks>This schedule will fold over the state and mutate it</remarks>
        /// <param name="state">some initial state</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, Func<S, Fin<A>, ScheduleContext, Aff<RT, (ScheduleResult Result, S State)>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new AffSched<RT, A>((result, context) => stepFunction(state, result, context)
                .Map(t =>
                {
                    state = t.State;
                    return t.Result;
                }));

        /// <summary>
        /// Creates a schedule from the provided step function with additional
        /// user provided state and initial seed duration. 
        /// </summary>
        /// <param name="state">user provided state</param>
        /// <param name="seed">seed duration</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched New<S>(
            S state, PositiveDuration seed, Func<S, PositiveDuration, ScheduleContext, PositiveDuration> stepFunction)
            => New((state, seed, stepFunction), static (t, ctx)
                => ScheduleResult.DelayAndRunAgain(ctx.HasNotStarted ? t.seed : t.stepFunction(t.state, t.seed, ctx)));

        /// <summary>
        /// Creates a schedule from the provided step function with additional
        /// user provided state and initial seed duration. 
        /// </summary>
        /// <param name="state">user provided state</param>
        /// <param name="seed">seed duration</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(
            S state, PositiveDuration seed,
            Func<S, PositiveDuration, Fin<A>, ScheduleContext, Eff<PositiveDuration>> stepFunction)
            => new EffSched<A>((result, context)
                => stepFunction(state, seed, result, context)
                    .Map(x => ScheduleResult.DelayAndRunAgain(context.HasNotStarted ? seed : x)));

        /// <summary>
        /// Creates a schedule from the provided step function with additional
        /// user provided state and initial seed duration. 
        /// </summary>
        /// <param name="state">user provided state</param>
        /// <param name="seed">seed duration</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<A> New<S, A>(
            S state, PositiveDuration seed,
            Func<S, PositiveDuration, Fin<A>, ScheduleContext, Aff<PositiveDuration>> stepFunction)
            => new AffSched<A>((result, context)
                => stepFunction(state, seed, result, context)
                    .Map(x => ScheduleResult.DelayAndRunAgain(context.HasNotStarted ? seed : x)));

        /// <summary>
        /// Creates a schedule from the provided step function with additional
        /// user provided state and initial seed duration. 
        /// </summary>
        /// <param name="state">user provided state</param>
        /// <param name="seed">seed duration</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, PositiveDuration seed,
            Func<S, PositiveDuration, Fin<A>, ScheduleContext, Eff<RT, PositiveDuration>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new EffSched<RT, A>((result, context)
                => stepFunction(state, seed, result, context)
                    .Map(x => ScheduleResult.DelayAndRunAgain(context.HasNotStarted ? seed : x)));

        /// <summary>
        /// Creates a schedule from the provided step function with additional
        /// user provided state and initial seed duration. 
        /// </summary>
        /// <param name="state">user provided state</param>
        /// <param name="seed">seed duration</param>
        /// <param name="stepFunction">step function</param>
        [Pure]
        public static Sched<RT, A> New<S, RT, A>(
            S state, PositiveDuration seed,
            Func<S, PositiveDuration, Fin<A>, ScheduleContext, Aff<RT, PositiveDuration>> stepFunction)
            where RT : struct, HasCancel<RT>
            => new AffSched<RT, A>((result, context)
                => stepFunction(state, seed, result, context)
                    .Map(x => ScheduleResult.DelayAndRunAgain(context.HasNotStarted ? seed : x)));
    }
}
