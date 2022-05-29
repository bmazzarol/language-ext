using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt.Next
{
    /// <summary>
    /// Base schedule.
    /// </summary>
    /// <remarks>Impacted by the result of the effect and has access to an environment.</remarks>
    /// <typeparam name="RT">environment</typeparam>
    /// <typeparam name="A">effect result</typeparam>
    public abstract class Sched<RT, A> where RT : struct, HasCancel<RT>
    {
        private protected Sched()
        {
        }

        [Pure]
        public static Sched<RT, A> operator |(Sched a, Sched<RT, A> b)
            => (a, b) switch
            {
                (ConstSched x, EffSched<RT, A> y) => (x | y) as Sched<RT, A>,
                (ConstSched x, AffSched<RT, A> y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator |(Sched<RT, A> a, Sched b) => b | a;

        [Pure]
        public static Sched<RT, A> operator |(Sched<A> a, Sched<RT, A> b)
            => (a, b) switch
            {
                (EffSched<A> x, EffSched<RT, A> y) => (x | y) as Sched<RT, A>,
                (EffSched<A> x, AffSched<RT, A> y) => x | y,
                (AffSched<A> x, EffSched<RT, A> y) => x | (AffSched<RT, A>)y,
                (AffSched<A> x, AffSched<RT, A> y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator |(Sched<RT, A> a, Sched<A> b) => b | a;

        [Pure]
        public static Sched<RT, A> operator |(Sched<RT, A> a, Sched<RT, A> b)
            => (a, b) switch
            {
                (EffSched<RT, A> x, EffSched<RT, A> y) => (x | y) as Sched<RT, A>,
                (EffSched<RT, A> x, AffSched<RT, A> y) => x | y,
                (AffSched<RT, A> x, EffSched<RT, A> y) => x | y,
                (AffSched<RT, A> x, AffSched<RT, A> y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator &(Sched a, Sched<RT, A> b)
            => (a, b) switch
            {
                (ConstSched x, EffSched<RT, A> y) => (x & y) as Sched<RT, A>,
                (ConstSched x, AffSched<RT, A> y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator &(Sched<RT, A> a, Sched b) => b & a;

        [Pure]
        public static Sched<RT, A> operator &(Sched<A> a, Sched<RT, A> b)
            => (a, b) switch
            {
                (EffSched<A> x, EffSched<RT, A> y) => (x & y) as Sched<RT, A>,
                (EffSched<A> x, AffSched<RT, A> y) => x & y,
                (AffSched<A> x, EffSched<RT, A> y) => x & (AffSched<RT, A>)y,
                (AffSched<A> x, AffSched<RT, A> y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator &(Sched<RT, A> a, Sched<A> b) => b & a;

        [Pure]
        public static Sched<RT, A> operator &(Sched<RT, A> a, Sched<RT, A> b)
            => (a, b) switch
            {
                (EffSched<RT, A> x, EffSched<RT, A> y) => (x & y) as Sched<RT, A>,
                (EffSched<RT, A> x, AffSched<RT, A> y) => x & y,
                (AffSched<RT, A> x, EffSched<RT, A> y) => x & y,
                (AffSched<RT, A> x, AffSched<RT, A> y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator &(Sched<RT, A> a, ScheduleResultTransformer transformer)
            => a switch
            {
                EffSched<RT, A> x => EffSched<RT, A>.Transform(x, transformer),
                AffSched<RT, A> x => AffSched<RT, A>.Transform(x, transformer),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<RT, A> operator &(ScheduleResultTransformer a, Sched<RT, A> b) => b & a;

        /// <summary>
        /// Test runs the schedule.
        /// </summary>
        /// <param name="env">environment</param>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> DryRun(
            RT env, Eff<RT, A> effect)
            => this switch
            {
                EffSched<RT, A> x => x.Run(env, effect, false),
                AffSched<RT, A> x => x.Run(env, effect, false).ToEnumerable().Result,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule.
        /// </summary>
        /// <param name="env">environment</param>
        /// <param name="effect">effect</param>
        [Pure]
        public IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> DryRun(
            RT env, Aff<RT, A> effect)
            => this switch
            {
                EffSched<RT, A> x => ((AffSched<RT, A>)x).Run(env, effect, false),
                AffSched<RT, A> x => x.Run(env, effect, false),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule returning the applied delays.
        /// </summary>
        /// <param name="env">environment</param>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<PositiveDuration> AppliedDelays(RT env, Eff<RT, A> effect)
            => this switch
            {
                EffSched<RT, A> x => x.AppliedDelays(env, effect),
                AffSched<RT, A> x => x.AppliedDelays(env, effect).ToEnumerable().Result,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule returning the applied delays.
        /// </summary>
        /// <param name="env">environment</param>
        /// <param name="effect">effect</param>
        [Pure]
        public IAsyncEnumerable<PositiveDuration> AppliedDelays(RT env, Aff<RT, A> effect)
            => this switch
            {
                EffSched<RT, A> x => ((AffSched<RT, A>)x).AppliedDelays(env, effect),
                AffSched<RT, A> x => x.AppliedDelays(env, effect),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        internal Eff<RT, S> RunEffect<S>(
            Eff<RT, A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => EffMaybe<RT, S>(env =>
            {
                var finalState = this switch
                {
                    AffSched<RT, A> x => x.Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)).Result,
                    EffSched<RT, A> x => x.Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var cachedResult = effect.Run(env);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Aff<RT, S> RunEffect<S>(
            Aff<RT, A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => AffMaybe<RT, S>(async env =>
            {
                var finalState = await (this switch
                {
                    AffSched<RT, A> x => x.Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    EffSched<RT, A> x => ((AffSched<RT, A>)x).Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                });
                var cachedResult = await effect.Run(env).ConfigureAwait(false);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Eff<RT, A> RunEffect(Eff<RT, A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);

        [Pure]
        internal Aff<RT, A> RunEffect(Aff<RT, A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);
    }

    /// <summary>
    /// Synchronous schedule with environment.
    /// </summary>
    /// <typeparam name="RT">environment</typeparam>
    /// <typeparam name="A">effect return type</typeparam>
    public sealed class EffSched<RT, A> : Sched<RT, A> where RT : struct, HasCancel<RT>
    {
        internal readonly Func<Fin<A>, ScheduleContext, Eff<RT, ScheduleResult>> StepFunction;

        internal EffSched(Func<Fin<A>, ScheduleContext, Eff<RT, ScheduleResult>> stepFunction)
            => StepFunction = stepFunction;

        private EffSched(ConstSched schedule)
            => StepFunction = (_, context) => Eff<RT, ScheduleResult>.Success(schedule.StepFunction(context));

        private EffSched(EffSched<A> schedule)
            => StepFunction = (result, context) => schedule.StepFunction(result, context);

        [Pure]
        public static implicit operator EffSched<RT, A>(ConstSched schedule)
            => new(schedule);

        [Pure]
        public static implicit operator EffSched<RT, A>(EffSched<A> schedule)
            => new(schedule);

        [Pure]
        public static EffSched<RT, A> operator |(EffSched<RT, A> a, EffSched<RT, A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra | rb);

        [Pure]
        public static EffSched<RT, A> operator &(EffSched<RT, A> a, EffSched<RT, A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra & rb);

        [Pure]
        internal static Sched<RT, A> Transform(EffSched<RT, A> schedule, ScheduleResultTransformer transformer)
            => new EffSched<RT, A>((result, context) => schedule.StepFunction(result, context)
                .Map(x => transformer(context, x)));

        [Pure]
        internal IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run(
            RT env, Eff<RT, A> effect, bool applyDelay = true)
        {
            var context = ScheduleContext.Initial;
            var effectResult = effect.Run(env);
            var result = StepFunction(effectResult, context).Run(env).IfFail(ScheduleResult.Complete);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (applyDelay && result.DelayUnsafe != PositiveDuration.Zero)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne((int)result.DelayUnsafe);
                }

                context = ScheduleContext.Next(result, context);
                effectResult = effect.Run(env);
                result = StepFunction(effectResult, context).Run(env).IfFail(ScheduleResult.Complete);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal new IEnumerable<PositiveDuration> AppliedDelays(RT env, Eff<RT, A> effect)
            => Run(env, effect, false)
                .Filter(x => !x.Result.IsComplete)
                .Map(x => x.Result.DelayUnsafe);
    }

    /// <summary>
    /// Asynchronous schedule with environment.
    /// </summary>
    /// <typeparam name="RT">environment</typeparam>
    /// <typeparam name="A">effect return type</typeparam>
    public sealed class AffSched<RT, A> : Sched<RT, A>
        where RT : struct, HasCancel<RT>
    {
        internal readonly Func<Fin<A>, ScheduleContext, Aff<RT, ScheduleResult>> StepFunction;

        internal AffSched(Func<Fin<A>, ScheduleContext, Aff<RT, ScheduleResult>> stepFunction)
            => StepFunction = stepFunction;

        private AffSched(ConstSched schedule)
            => StepFunction = (_, context) => Aff<RT, ScheduleResult>.Success(schedule.StepFunction(context));

        private AffSched(AffSched<A> schedule)
            => StepFunction = (result, context) => schedule.StepFunction(result, context);

        private AffSched(EffSched<RT, A> schedule)
            => StepFunction = (result, context) => schedule.StepFunction(result, context);

        private AffSched(EffSched<A> schedule)
            => StepFunction = (result, context) => schedule.StepFunction(result, context);

        [Pure]
        public static implicit operator AffSched<RT, A>(ConstSched schedule)
            => new(schedule);

        [Pure]
        public static implicit operator AffSched<RT, A>(AffSched<A> schedule)
            => new(schedule);

        [Pure]
        public static implicit operator AffSched<RT, A>(EffSched<RT, A> schedule)
            => new(schedule);

        [Pure]
        public static implicit operator AffSched<RT, A>(EffSched<A> schedule)
            => new(schedule);

        [Pure]
        public static AffSched<RT, A> operator |(AffSched<RT, A> a, AffSched<RT, A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra | rb);

        [Pure]
        public static AffSched<RT, A> operator &(AffSched<RT, A> a, AffSched<RT, A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra & rb);

        [Pure]
        internal static Sched<RT, A> Transform(AffSched<RT, A> schedule, ScheduleResultTransformer transformer)
            => new AffSched<RT, A>((result, context) => schedule.StepFunction(result, context)
                .Map(x => transformer(context, x)));

        [Pure]
        internal async IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run(
            RT env, Aff<RT, A> effect, bool applyDelay = true)
        {
            var context = ScheduleContext.Initial;
            var effectResult = await effect.Run(env).ConfigureAwait(false);
            var result = (await StepFunction(effectResult, context).Run(env).ConfigureAwait(false))
                .IfFail(ScheduleResult.Complete);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (applyDelay && result.DelayUnsafe != PositiveDuration.Zero)
                    await Task.Delay((int)result.DelayUnsafe).ConfigureAwait(false);

                context = ScheduleContext.Next(result, context);
                effectResult = await effect.Run(env).ConfigureAwait(false);
                result = (await StepFunction(effectResult, context).Run(env).ConfigureAwait(false))
                    .IfFail(ScheduleResult.Complete);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal new IAsyncEnumerable<PositiveDuration> AppliedDelays(RT env, Aff<RT, A> effect)
            => Run(env, effect, false)
                .Filter(x => !x.Result.IsComplete)
                .Map(x => x.Result.DelayUnsafe);
    }
}
