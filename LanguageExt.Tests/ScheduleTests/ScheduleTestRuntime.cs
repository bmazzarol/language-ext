using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;

namespace LanguageExt.Next.Tests.ScheduleTests
{
    internal struct ScheduleTestTimeIO : TimeIO
    {
        private readonly Func<DateTime> _nowFn;

        internal ScheduleTestTimeIO(Func<DateTime> nowFn) => _nowFn = nowFn;

        public DateTime Now => _nowFn();
        public DateTime UtcNow => _nowFn();
        public DateTime Today => _nowFn();
        public ValueTask<Unit> SleepUntil(DateTime dt, CancellationToken token) => throw new NotImplementedException();

        public ValueTask<Unit> SleepFor(TimeSpan ts, CancellationToken token) => throw new NotImplementedException();
    }

    internal struct ScheduleTestRuntime : HasTime<ScheduleTestRuntime>
    {
        private readonly Func<DateTime> _dateFn;
        private readonly ScheduleTestTimeIO _timeIo;

        public ScheduleTestRuntime(Func<DateTime> dateFn)
        {
            _dateFn = dateFn;
            _timeIo = new ScheduleTestTimeIO(dateFn);
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationToken.None;
        }

        public ScheduleTestRuntime LocalCancel => new(_dateFn);
        public CancellationToken CancellationToken { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public Eff<ScheduleTestRuntime, TimeIO> TimeEff => Eff<ScheduleTestRuntime, TimeIO>.Success(_timeIo);
    }
}
