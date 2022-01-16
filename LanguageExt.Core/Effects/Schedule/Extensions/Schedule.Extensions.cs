using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;
using static LanguageExt.ScheduleStatus;

namespace LanguageExt
{
    using TestScheduleResult = IEnumerable<(IScheduleStatus Status, IScheduleResult Result)>;

    public static class ScheduleExtensions
    {
        /// <summary>
        /// Dry runs a schedule, can be used to test a given schedule.
        /// </summary>
        /// <param name="schedule">schedule to run</param>
        /// <param name="env">runtime to test against</param>
        /// <param name="resultGen">some provided result generator, used to drive the schedule</param>
        /// <returns>enumerable sequence of schedule results, can be infinite</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TestScheduleResult DryRun<RT, A>(
            this Schedule<RT, A> schedule, RT env, Func<IScheduleStatus, Fin<A>> resultGen)
        {
            var status = RunOnce;
            var value = resultGen(status);
            var result = schedule.InternalSchedule(env, value, status);
            while (result is not CompleteEffect)
            {
                yield return (status, result);
                status = status.RegisterIteration(result);
                value = resultGen(status);
                result = schedule.InternalSchedule(env, value, status);
            }

            yield return (status, result);
        }

        /// <summary>
        /// Dry runs a schedule, can be used to test a given schedule.
        /// </summary>
        /// <param name="schedule">schedule to run</param>
        /// <param name="resultGen">some provided result generator, used to drive the schedule</param>
        /// <returns>enumerable sequence of schedule results, can be infinite</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TestScheduleResult DryRun<A>(
            this Schedule<A> schedule, Func<IScheduleStatus, Fin<A>> resultGen)
            => Schedule<A>.Widen(schedule).DryRun(default, resultGen);

        /// <summary>
        /// Dry runs a schedule, can be used to test a given schedule.
        /// </summary>
        /// <param name="schedule">schedule to run</param>
        /// <returns>enumerable sequence of schedule results, can be infinite</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TestScheduleResult DryRun(
            this Schedule schedule)
            => Schedule.Widen(schedule).DryRun(default, _ => default);

        /// <summary>
        /// Returns all the applied delay values in the test schedule result.
        /// </summary>
        [Pure]
        public static Seq<TimeSpan> AppliedDelays(this TestScheduleResult result)
            => result.Fold(Seq.empty<TimeSpan>(),
                (seq, r) => r.Result is ReRunEffectAfterDelay x ? seq.Add(x.Delay) : seq);

        /// <summary>
        /// Runs an asynchronous effect against the schedule.
        /// </summary>
        /// <param name="schedule">schedule</param>
        /// <param name="env">runtime</param>
        /// <param name="effect">effect</param>
        /// <returns>async enumerable sequence of results</returns>
        public static async IAsyncEnumerable<(Fin<A> Result, IScheduleResult Status)> Run<RT, A>(
            this Schedule<RT, A> schedule, RT env, Aff<RT, A> effect)
            where RT : struct, HasCancel<RT>
        {
            var status = RunOnce;
            var value = await effect.ReRun(env).ConfigureAwait(false);
            var result = schedule.InternalSchedule(env, value, status);
            while (result is not CompleteEffect && !env.CancellationToken.IsCancellationRequested)
            {
                yield return (value, result);

                if (result is ReRunEffectAfterDelay x)
                    await Task.Delay(x.Delay, env.CancellationToken).ConfigureAwait(false);

                status = status.RegisterIteration(result);
                value = await effect.ReRun(env).ConfigureAwait(false);
                result = schedule.InternalSchedule(env, value, status);
            }

            yield return (env.CancellationToken.IsCancellationRequested ? Common.Errors.Cancelled : value, result);
        }

        /// <summary>
        /// Runs an asynchronous effect against the schedule.
        /// </summary>
        /// <param name="schedule">schedule</param>
        /// <param name="effect">effect</param>
        /// <returns>async enumerable sequence of results</returns>
        public static async IAsyncEnumerable<(Fin<A> Result, IScheduleResult Status)> Run<A>(
            this Schedule<A> schedule, Aff<A> effect)
        {
            var status = RunOnce;
            var value = await effect.ReRun().ConfigureAwait(false);
            var result = schedule.InternalSchedule(default, value, status);
            while (result is not CompleteEffect)
            {
                yield return (value, result);

                if (result is ReRunEffectAfterDelay x)
                    await Task.Delay(x.Delay).ConfigureAwait(false);

                status = status.RegisterIteration(result);
                value = await effect.ReRun().ConfigureAwait(false);
                result = schedule.InternalSchedule(default, value, status);
            }

            yield return (value, result);
        }

        /// <summary>
        /// Runs a synchronous effect against the schedule.
        /// </summary>
        /// <param name="schedule">schedule</param>
        /// <param name="env">runtime</param>
        /// <param name="effect">effect</param>
        /// <returns>enumerable sequence of results</returns>
        public static IEnumerable<(Fin<A> Result, IScheduleResult Status)> Run<RT, A>(
            this Schedule<RT, A> schedule, RT env, Eff<RT, A> effect)
            where RT : struct
        {
            var status = RunOnce;
            var value = effect.ReRun(env);
            var result = schedule.InternalSchedule(env, value, status);
            while (result is not CompleteEffect)
            {
                yield return (value, result);

                if (result is ReRunEffectAfterDelay x)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne(x.Delay);
                }

                status = status.RegisterIteration(result);
                value = effect.ReRun(env);
                result = schedule.InternalSchedule(env, value, status);
            }

            yield return (value, result);
        }

        /// <summary>
        /// Runs a synchronous effect against the schedule.
        /// </summary>
        /// <param name="schedule">schedule</param>
        /// <param name="effect">effect</param>
        /// <returns>enumerable sequence of results</returns>
        public static IEnumerable<(Fin<A> Result, IScheduleResult Status)> Run<A>(
            this Schedule<A> schedule, Eff<A> effect)
        {
            var status = RunOnce;
            var value = effect.ReRun();
            var result = schedule.InternalSchedule(default, value, status);
            while (result is not CompleteEffect)
            {
                yield return (value, result);

                if (result is ReRunEffectAfterDelay x)
                {
                    var wait = new AutoResetEvent(false);
                    wait.WaitOne(x.Delay);
                }

                status = status.RegisterIteration(result);
                value = effect.ReRun();
                result = schedule.InternalSchedule(default, value, status);
            }

            yield return (value, result);
        }

        [Pure]
        internal static Func<S, (Fin<A> Result, IScheduleResult Status), S> LiftFoldFn<A, S>(
            this Func<S, A, S> fold) => (s, t) => t.Result.IsSucc ? fold(s, (A)t.Result) : s;

        [Pure]
        internal static Func<(Fin<A> Result, IScheduleResult Status), bool> LiftPredFn<A>(
            this Func<Fin<A>, bool> pred) => t => pred(t.Result);

        /// <summary>
        /// Adds a runtime and value to a schedule.
        /// </summary>
        [Pure]
        public static Schedule<RT, A> WithRuntimeAndValue<RT, A>(this Schedule schedule)
            => Schedule<RT, A>.Widen(schedule);

        /// <summary>
        /// Adds a runtime to a schedule.
        /// </summary>
        [Pure]
        public static Schedule<RT, A> WithRuntime<RT, A>(this Schedule<A> schedule)
            => Schedule<RT, A>.Widen(schedule);

        /// <summary>
        /// Adds a value to a schedule.
        /// </summary>
        [Pure]
        public static Schedule<A> WithValue<A>(this Schedule schedule)
            => Schedule<A>.Widen(schedule);
    }
}
