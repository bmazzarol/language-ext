using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.Next
{
    /// <summary>
    /// Base schedule.
    /// </summary>
    /// <remarks>Impacted by the result of the effect. Has no environment</remarks>
    /// <typeparam name="A">effect return</typeparam>
    public abstract class Sched<A>
    {
        private protected Sched()
        {
        }

        [Pure]
        public static Sched<A> operator |(Sched a, Sched<A> b)
            => (a, b) switch
            {
                (ConstSched x, EffSched<A> y) => (x | y) as Sched<A>,
                (ConstSched x, AffSched<A> y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<A> operator |(Sched<A> a, Sched b) => b | a;

        [Pure]
        public static Sched<A> operator |(Sched<A> a, Sched<A> b)
            => (a, b) switch
            {
                (EffSched<A> x, EffSched<A> y) => (x | y) as Sched<A>,
                (EffSched<A> x, AffSched<A> y) => x | y,
                (AffSched<A> x, EffSched<A> y) => x | y,
                (AffSched<A> x, AffSched<A> y) => x | y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<A> operator &(Sched a, Sched<A> b)
            => (a, b) switch
            {
                (ConstSched x, EffSched<A> y) => (x & y) as Sched<A>,
                (ConstSched x, AffSched<A> y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<A> operator &(Sched<A> a, Sched b) => b & a;

        [Pure]
        public static Sched<A> operator &(Sched<A> a, Sched<A> b)
            => (a, b) switch
            {
                (EffSched<A> x, EffSched<A> y) => (x & y) as Sched<A>,
                (EffSched<A> x, AffSched<A> y) => x & y,
                (AffSched<A> x, EffSched<A> y) => x & y,
                (AffSched<A> x, AffSched<A> y) => x & y,
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<A> operator &(Sched<A> a, ScheduleResultTransformer transformer)
            => a switch
            {
                EffSched<A> x => EffSched<A>.Transform(x, transformer),
                AffSched<A> x => AffSched<A>.Transform(x, transformer),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        public static Sched<A> operator &(ScheduleResultTransformer a, Sched<A> b) => b & a;

        /// <summary>
        /// Test runs the schedule.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> DryRun(
            Eff<A> effect)
            => this switch
            {
                EffSched<A> x => x.Run(effect, false),
                AffSched<A> x => x.Run(effect, false).ToEnumerable().Result,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> DryRun(
            Aff<A> effect)
            => this switch
            {
                EffSched<A> x => ((AffSched<A>)x).Run(effect, false),
                AffSched<A> x => x.Run(effect, false),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule returning the applied delays.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IEnumerable<PositiveDuration> AppliedDelays(Eff<A> effect)
            => this switch
            {
                EffSched<A> x => x.AppliedDelays(effect),
                AffSched<A> x => x.AppliedDelays(effect).ToEnumerable().Result,
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Test runs the schedule returning the applied delays.
        /// </summary>
        /// <param name="effect">effect</param>
        [Pure]
        public IAsyncEnumerable<PositiveDuration> AppliedDelays(Aff<A> effect)
            => this switch
            {
                EffSched<A> x => ((AffSched<A>)x).AppliedDelays(effect),
                AffSched<A> x => x.AppliedDelays(effect),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Pure]
        internal Eff<S> RunEffect<S>(
            Eff<A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => EffMaybe(() =>
            {
                var finalState = this switch
                {
                    AffSched<A> x => x.Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)).Result,
                    EffSched<A> x => x.Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var cachedResult = effect.Run();
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Aff<S> RunEffect<S>(
            Aff<A> effect, S state, Func<S, A, S> folder, Func<Fin<A>, bool> predicate)
            => AffMaybe(async () =>
            {
                var finalState = await (this switch
                {
                    AffSched<A> x => x.Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    EffSched<A> x => ((AffSched<A>)x).Run(effect).FoldWhile(state,
                        (s, t) => t.EffectResult.IsSucc ? folder(s, (A)t.EffectResult) : s,
                        t => predicate(t.EffectResult)),
                    _ => throw new ArgumentOutOfRangeException()
                });
                var cachedResult = await effect.Run().ConfigureAwait(false);
                return cachedResult.IsSucc ? finalState : Fin<S>.Fail((Error)cachedResult);
            });

        [Pure]
        internal Eff<A> RunEffect(Eff<A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);

        [Pure]
        internal Aff<A> RunEffect(Aff<A> effect, Func<Fin<A>, bool> pred)
            => RunEffect(effect, default(A), static (_, x) => x, pred);
    }

    /// <summary>
    /// Synchronous schedule with no environment.
    /// </summary>
    /// <typeparam name="A">effect return type</typeparam>
    public sealed class EffSched<A> : Sched<A>
    {
        internal readonly Func<Fin<A>, ScheduleContext, Eff<ScheduleResult>> StepFunction;

        internal EffSched(Func<Fin<A>, ScheduleContext, Eff<ScheduleResult>> stepFunction)
            => StepFunction = stepFunction;

        [Pure]
        public static implicit operator EffSched<A>(ConstSched schedule)
            => new((_, context) => Eff<ScheduleResult>.Success(schedule.StepFunction(context)));

        [Pure]
        public static EffSched<A> operator |(EffSched<A> a, EffSched<A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra | rb);

        [Pure]
        public static EffSched<A> operator |(EffSched<A> a, ScheduleResultTransformer transformer)
            => new((result, context) => a.StepFunction(result, context)
                .Map(x => transformer(context, x)));

        [Pure]
        public static EffSched<A> operator &(EffSched<A> a, EffSched<A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra & rb);

        [Pure]
        internal static Sched<A> Transform(EffSched<A> schedule, ScheduleResultTransformer transformer)
            => new EffSched<A>((result, context) => schedule.StepFunction(result, context)
                .Map(x => transformer(context, x)));

        [Pure]
        internal IEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run(
            Eff<A> effect, bool applyDelay = true)
        {
            var context = ScheduleContext.Initial;
            var effectResult = effect.Run();
            var result = StepFunction(effectResult, context).Run().IfFail(ScheduleResult.Complete);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (applyDelay && result.DelayUnsafe != PositiveDuration.Zero)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne((int)result.DelayUnsafe);
                }

                context = ScheduleContext.Next(result, context);
                effectResult = effect.Run();
                result = StepFunction(effectResult, context).Run().IfFail(ScheduleResult.Complete);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal new IEnumerable<PositiveDuration> AppliedDelays(Eff<A> effect)
            => Run(effect, false)
                .Filter(x => !x.Result.IsComplete)
                .Map(x => x.Result.DelayUnsafe);
    }

    /// <summary>
    /// Asynchronous schedule with no environment.
    /// </summary>
    /// <typeparam name="A">effect return type</typeparam>
    public sealed class AffSched<A> : Sched<A>
    {
        internal readonly Func<Fin<A>, ScheduleContext, Aff<ScheduleResult>> StepFunction;

        internal AffSched(Func<Fin<A>, ScheduleContext, Aff<ScheduleResult>> stepFunction)
            => StepFunction = stepFunction;

        [Pure]
        public static implicit operator AffSched<A>(ConstSched schedule)
            => new((_, context) => Aff<ScheduleResult>.Success(schedule.StepFunction(context)));

        [Pure]
        public static implicit operator AffSched<A>(EffSched<A> schedule)
            => new((result, context) => schedule.StepFunction(result, context));

        [Pure]
        public static AffSched<A> operator |(AffSched<A> a, AffSched<A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra | rb);

        [Pure]
        public static AffSched<A> operator &(AffSched<A> a, AffSched<A> b)
            => new((result, context) =>
                from ra in a.StepFunction(result, context)
                from rb in b.StepFunction(result, context)
                select ra & rb);

        [Pure]
        internal static Sched<A> Transform(AffSched<A> schedule, ScheduleResultTransformer transformer)
            => new AffSched<A>((result, context) => schedule.StepFunction(result, context)
                .Map(x => transformer(context, x)));

        [Pure]
        internal async IAsyncEnumerable<(ScheduleContext Context, Fin<A> EffectResult, ScheduleResult Result)> Run(
            Aff<A> effect, bool applyDelay = true)
        {
            var context = ScheduleContext.Initial;
            var effectResult = await effect.Run().ConfigureAwait(false);
            var result = (await StepFunction(effectResult, context).Run().ConfigureAwait(false))
                .IfFail(ScheduleResult.Complete);
            while (!result.IsComplete)
            {
                yield return (context, effectResult, result);

                if (applyDelay && result.DelayUnsafe != PositiveDuration.Zero)
                    await Task.Delay((int)result.DelayUnsafe).ConfigureAwait(false);

                context = ScheduleContext.Next(result, context);
                effectResult = await effect.Run().ConfigureAwait(false);
                result = (await StepFunction(effectResult, context).Run().ConfigureAwait(false))
                    .IfFail(ScheduleResult.Complete);
            }

            yield return (context, effectResult, result);
        }

        [Pure]
        internal new IAsyncEnumerable<PositiveDuration> AppliedDelays(Aff<A> effect)
            => Run(effect, false)
                .Filter(x => !x.Result.IsComplete)
                .Map(x => x.Result.DelayUnsafe);
    }
}
