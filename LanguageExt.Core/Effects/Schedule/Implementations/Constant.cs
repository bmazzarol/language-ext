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
    /// <remarks>Not impacted by the result of the effect or environment.</remarks>
    public abstract class Sched
    {
        private protected Sched()
        {
        }

        [Pure]
        public static Sched operator |(Sched a, Sched b)
            => (a, b) switch
            {
                (ConstSched x, ConstSched y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched operator &(Sched a, Sched b)
            => (a, b) switch
            {
                (ConstSched x, ConstSched y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched operator &(Sched a, ScheduleResultTransformer transformer)
            => a switch
            {
                ConstSched x => ConstSched.Transform(x, transformer),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched operator &(ScheduleResultTransformer a, Sched b) => b & a;

        /// <summary>
        /// Test runs the schedule.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<(ScheduleContext Context, ScheduleResult Result)> DryRun()
            => this switch
            {
                ConstSched x => x.Run(false),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule returning the applied delays.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<PositiveDuration> AppliedDelays()
            => this switch
            {
                ConstSched x => x.AppliedDelays(),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        internal Eff<S> RunEffect<S, A>(
            Eff<A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => EffMaybe(() =>
            {
                var finalState = this switch
                {
                    ConstSched x => x.Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var cachedResult = effect.Run();
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Eff<RT, S> RunEffect<S, RT, A>(
            Eff<RT, A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate) where RT : struct
            => EffMaybe<RT, S>(env =>
            {
                var finalState = this switch
                {
                    ConstSched x => x.Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var cachedResult = effect.Run(env);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Aff<S> RunEffect<S, A>(
            Aff<A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => AffMaybe(async () =>
            {
                var finalState = await (this switch
                {
                    ConstSched x => x.Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                });
                var cachedResult = await effect.Run().ConfigureAwait(true);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Aff<RT, S> RunEffect<S, RT, A>(
            Aff<RT, A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            where RT : struct, HasCancel<RT>
            => AffMaybe<RT, S>(async env =>
            {
                var finalState = await (this switch
                {
                    ConstSched x => x.Run(env, effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                });
                var cachedResult = await effect.Run(env).ConfigureAwait(false);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Eff<A> RunEffect<A>(Eff<A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);

        [Pure]
        internal Eff<RT, A> RunEffect<RT, A>(Eff<RT, A> effect, Func<Fin<A>, bool> pred) where RT : struct
            => RunEffect(effect, default(A), static (_, x) => x, pred);

        [Pure]
        internal Aff<A> RunEffect<A>(Aff<A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);

        [Pure]
        internal Aff<RT, A> RunEffect<RT, A>(Aff<RT, A> effect, Func<Fin<A>, bool> pred)
            where RT : struct, HasCancel<RT>
            => RunEffect(effect, default(A), static (_, x) => x, pred);
    }

    /// <summary>
    /// Schedule that is not influenced by the result of the effect.
    /// </summary>
    public sealed class ConstSched : Sched
    {
        internal readonly Func<ScheduleContext, ScheduleResult> StepFunction;

        internal ConstSched(Func<ScheduleContext, ScheduleResult> stepFunction)
            => StepFunction = stepFunction;

        [Pure]
        public static ConstSched operator |(ConstSched a, ConstSched b)
            => new(context => a.StepFunction(context) | b.StepFunction(context));

        [Pure]
        public static ConstSched operator &(ConstSched a, ConstSched b)
            => new(context => a.StepFunction(context) & b.StepFunction(context));

        [Pure]
        internal static ConstSched Transform(ConstSched a, ScheduleResultTransformer transformer)
            => new(context => transformer(context, a.StepFunction(context)));

        [Pure]
        internal IEnumerable<(ScheduleContext Context, ScheduleResult Result)> Run(bool applyDelay = true)
        {
            var context = ScheduleContext.Initial;
            var result = StepFunction(context);
            while (!result.IsComplete)
            {
                yield return (context, result);

                if (applyDelay && result.DelayUnsafe != PositiveDuration.Zero)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne((int)result.DelayUnsafe);
                }

                context = ScheduleContext.Next(result, context);
                result = StepFunction(context);
            }

            yield return (context, result);
        }

        [Pure]
        internal IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run<A>(
            Eff<A> effect)
        {
            var context = ScheduleContext.Initial;
            var effectResult = effect.Run();
            var result = StepFunction(context);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (result.DelayUnsafe != PositiveDuration.Zero)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne((int)result.DelayUnsafe);
                }

                context = ScheduleContext.Next(result, context);
                effectResult = effect.Run();
                result = StepFunction(context);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run<RT, A>(
            RT env, Eff<RT, A> effect) where RT : struct
        {
            var context = ScheduleContext.Initial;
            var effectResult = effect.Run(env);
            var result = StepFunction(context);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (result.DelayUnsafe != PositiveDuration.Zero)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne((int)result.DelayUnsafe);
                }

                context = ScheduleContext.Next(result, context);
                effectResult = effect.Run(env);
                result = StepFunction(context);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal async IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run<A>(
            Aff<A> effect)
        {
            var context = ScheduleContext.Initial;
            var effectResult = await effect.Run().ConfigureAwait(false);
            var result = StepFunction(context);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (result.DelayUnsafe != PositiveDuration.Zero)
                    await Task.Delay((int)result.DelayUnsafe).ConfigureAwait(false);

                context = ScheduleContext.Next(result, context);
                effectResult = await effect.Run().ConfigureAwait(false);
                result = StepFunction(context);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal async IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)>
            Run<RT, A>(RT env, Aff<RT, A> effect)
            where RT : struct, HasCancel<RT>
        {
            var context = ScheduleContext.Initial;
            var effectResult = await effect.Run(env).ConfigureAwait(false);
            var result = StepFunction(context);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (result.DelayUnsafe != PositiveDuration.Zero)
                    await Task.Delay((int)result.DelayUnsafe).ConfigureAwait(false);

                context = ScheduleContext.Next(result, context);
                effectResult = await effect.Run(env).ConfigureAwait(false);
                result = StepFunction(context);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal new IEnumerable<PositiveDuration> AppliedDelays()
            => Run(false)
                .Filter(x => !x.Result.IsComplete)
                .Map(x => x.Result.DelayUnsafe);
    }
}
