#pragma warning disable S4136, CA1000

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LanguageExt.Effects.Traits;
using LanguageExt.SysX.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt.SysX.Diag
{
    /// <summary>
    /// An `Activity` has an operation name, an ID, a start time and duration, tags, and baggage.
    ///
    /// Activities should be created by calling the `span` functions, configured as necessary.  Each `span` function
    /// takes an `Eff` or `Aff` operation to run (which is the activity).  The runtime system will maintain the parent-
    /// child relationships for the activities, and maintains the 'current' activity.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Activity2<TR> where TR : struct, HasActivitySource<TR>, HasCancel<TR>
    {
        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Eff<TR, T> span<T>(string name, Eff<TR, T> operation) =>
            span(name, ActivityKind.Internal, default, default, DateTimeOffset.Now, operation);

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Aff<TR, T> span<T>(string name, Aff<TR, T> operation) =>
            span(name, ActivityKind.Internal, default, default, DateTimeOffset.Now, operation);

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Eff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            Eff<TR, T> operation
        ) => span(name, activityKind, default, default, DateTimeOffset.Now, operation);

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Aff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            Aff<TR, T> operation
        ) => span(name, activityKind, default, default, DateTimeOffset.Now, operation);

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Eff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            HashMap<string, object> activityTags,
            Eff<TR, T> operation
        ) => span(name, activityKind, activityTags, default, DateTimeOffset.Now, operation);

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Aff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            HashMap<string, object> activityTags,
            Aff<TR, T> operation
        ) => span(name, activityKind, activityTags, default, DateTimeOffset.Now, operation);

        private readonly record struct DisposableActivity(Activity? Activity) : IDisposable
        {
            public void Dispose() => Activity?.Dispose();
        }

        private static Eff<TR, DisposableActivity> startActivity(
            string name,
            ActivityKind activityKind,
            HashMap<string, object> activityTags,
            Seq<ActivityLink> activityLinks,
            DateTimeOffset startTime,
            ActivityContext? parentContext = default
        ) =>
            from rt in runtime<TR>()
            from source in rt.ActivitySourceEff.Map(
                x =>
                    new DisposableActivity(
                        rt.CurrentActivity == null
                            ? x.StartActivity(name, activityKind)
                            : x.StartActivity(
                                name,
                                activityKind,
                                parentContext ?? rt.CurrentActivity.Context,
                                activityTags,
                                activityLinks,
                                startTime
                            )
                    )
            )
            select source;

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="parentContext">The parent `ActivityContext` object to initialize the created activity object
        /// with</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="activityLinks">The optional `ActivityLink` list to initialise the created activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created activity object.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Eff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            ActivityContext parentContext,
            HashMap<string, object> activityTags,
            Seq<ActivityLink> activityLinks,
            DateTimeOffset startTime,
            Eff<TR, T> operation
        ) =>
            use(
                startActivity(
                    name,
                    activityKind,
                    activityTags,
                    activityLinks,
                    startTime,
                    parentContext
                ),
                act => localEff<TR, TR, T>(rt => rt.SetActivity(act.Activity), operation)
            );

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="parentContext">The parent `ActivityContext` object to initialize the created activity object
        /// with</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="activityLinks">The optional `ActivityLink` list to initialise the created activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created activity object.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        public static Aff<TR, T> span<T>(
            string name,
            ActivityKind activityKind,
            ActivityContext parentContext,
            HashMap<string, object> activityTags,
            Seq<ActivityLink> activityLinks,
            DateTimeOffset startTime,
            Aff<TR, T> operation
        ) =>
            use(
                startActivity(
                    name,
                    activityKind,
                    activityTags,
                    activityLinks,
                    startTime,
                    parentContext
                ),
                act => localAff<TR, TR, T>(rt => rt.SetActivity(act.Activity), operation)
            );

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="activityLinks">The optional `ActivityLink` list to initialise the created activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created activity object.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        private static Eff<TR, A> span<A>(
            string name,
            ActivityKind activityKind,
            HashMap<string, object> activityTags,
            Seq<ActivityLink> activityLinks,
            DateTimeOffset startTime,
            Eff<TR, A> operation
        ) =>
            use(
                startActivity(name, activityKind, activityTags, activityLinks, startTime),
                act => localEff<TR, TR, A>(rt => rt.SetActivity(act.Activity), operation)
            );

        /// <summary>
        /// Creates a new activity if there are active listeners for it, using the specified name, activity kind, parent
        /// activity context, tags, optional activity link and optional start time.
        /// </summary>
        /// <param name="name">The operation name of the activity.</param>
        /// <param name="activityKind">The activity kind.</param>
        /// <param name="activityTags">The optional tags list to initialise the created activity object with.</param>
        /// <param name="activityLinks">The optional `ActivityLink` list to initialise the created activity object with.</param>
        /// <param name="startTime">The optional start timestamp to set on the created activity object.</param>
        /// <param name="operation">The operation to whose activity will be traced</param>
        /// <returns>The result of the `operation`</returns>
        static Aff<TR, A> span<A>(
            string name,
            ActivityKind activityKind,
            HashMap<string, object> activityTags,
            Seq<ActivityLink> activityLinks,
            DateTimeOffset startTime,
            Aff<TR, A> operation
        ) =>
            use(
                startActivity(name, activityKind, activityTags, activityLinks, startTime),
                act => localAff<TR, TR, A>(rt => rt.SetActivity(act.Activity), operation)
            );

        /// <summary>
        /// Set the state trace string
        /// </summary>
        /// <param name="traceStateString">Trace state string</param>
        /// <returns>Unit effect</returns>
        public static Eff<TR, Unit> setTraceState(string traceStateString) =>
            Eff<TR, Unit>(rt =>
            {
                if (rt.CurrentActivity is not null)
                    rt.CurrentActivity.TraceStateString = traceStateString;
                return unit;
            });

        /// <summary>
        /// Read the trace-state string of the current activity
        /// </summary>
        public static Eff<TR, Option<string>> traceState =>
            Eff<TR, Option<string>>(rt => rt.CurrentActivity?.TraceStateString);

        /// <summary>
        /// Read the trace ID of the current activity
        /// </summary>
        public static Eff<TR, Option<ActivityTraceId>> traceId =>
            Eff<TR, Option<ActivityTraceId>>(rt => Optional(rt.CurrentActivity?.TraceId));

        /// <summary>
        /// Add baggage to the current activity
        /// </summary>
        /// <param name="key">Baggage key</param>
        /// <param name="value">Baggage value</param>
        /// <returns>Unit effect</returns>
        public static Eff<TR, Unit> addBaggage(string key, string? value) =>
            Eff<TR, Unit>(rt =>
            {
                rt.CurrentActivity?.AddBaggage(key, value);
                return unit;
            });

        /// <summary>
        /// Read the baggage of the current activity
        /// </summary>
        public static Eff<TR, HashMap<string, string?>> baggage =>
            Eff<TR, HashMap<string, string?>>(
                rt =>
                    rt.CurrentActivity is not null
                        ? rt.CurrentActivity.Baggage.ToHashMap()
                        : HashMap<string, string?>()
            );

        /// <summary>
        /// Add tag to the current activity
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="value">Tag value</param>
        /// <returns>Unit effect</returns>
        public static Eff<TR, Unit> addTag(string name, string? value) =>
            Eff<TR, Unit>(rt =>
            {
                rt.CurrentActivity?.AddTag(name, value);
                return unit;
            });

        /// <summary>
        /// Add tag to the current activity
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="value">Tag value</param>
        public static Eff<TR, Unit> addTag(string name, object? value) =>
            Eff<TR, Unit>(rt =>
            {
                rt.CurrentActivity?.AddTag(name, value);
                return unit;
            });

        /// <summary>
        /// Read the tags of the current activity
        /// </summary>
        public static Eff<TR, HashMap<string, string?>> tags =>
            Eff<TR, HashMap<string, string?>>(
                rt =>
                    rt.CurrentActivity is not null
                        ? rt.CurrentActivity.Tags.ToHashMap()
                        : HashMap<string, string?>()
            );

        /// <summary>
        /// Read the tags of the current activity
        /// </summary>
        public static Eff<TR, HashMap<string, object?>> tagObjects =>
            Eff<TR, HashMap<string, object?>>(
                rt =>
                    rt.CurrentActivity is not null
                        ? rt.CurrentActivity.TagObjects.ToHashMap()
                        : HashMap<string, object?>()
            );

        /// <summary>
        /// Read the context of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<ActivityContext>> context =>
            Eff<TR, Option<ActivityContext>>(rt => Optional(rt.CurrentActivity?.Context));

        /// <summary>
        /// Read the duration of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<TimeSpan>> duration =>
            Eff<TR, Option<TimeSpan>>(rt => Optional(rt.CurrentActivity?.Duration));

        /// <summary>
        /// Read the events of the current activity
        /// </summary>
        public static Eff<TR, Seq<ActivityEvent>> events =>
            Eff<TR, Seq<ActivityEvent>>(
                rt =>
                    rt.CurrentActivity is not null
                        ? rt.CurrentActivity.Events.ToSeq()
                        : Seq<ActivityEvent>()
            );

        /// <summary>
        /// Read the ID of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<string>> id =>
            Eff<TR, Option<string>>(rt => Optional(rt.CurrentActivity?.Id));

        /// <summary>
        /// Read the kind of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<ActivityKind>> kind =>
            Eff<TR, Option<ActivityKind>>(rt => Optional(rt.CurrentActivity?.Kind));

        /// <summary>
        /// Read the links of the current activity
        /// </summary>
        public static Eff<TR, Seq<ActivityLink>> links =>
            Eff<TR, Seq<ActivityLink>>(
                rt =>
                    rt.CurrentActivity is not null
                        ? rt.CurrentActivity.Links.ToSeq()
                        : Seq<ActivityLink>()
            );

        /// <summary>
        /// Read the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<Activity>> current =>
            Eff<TR, Option<Activity>>(rt => Optional(rt.CurrentActivity));

        /// <summary>
        /// Read the parent ID of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<string>> parentId =>
            Eff<TR, Option<string>>(rt => Optional(rt.CurrentActivity?.ParentId));

        /// <summary>
        /// Read the parent span ID of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<ActivitySpanId>> parentSpanId =>
            Eff<TR, Option<ActivitySpanId>>(rt => Optional(rt.CurrentActivity?.ParentSpanId));

        /// <summary>
        /// Read the recorded flag of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<bool>> recorded =>
            Eff<TR, Option<bool>>(rt => Optional(rt.CurrentActivity?.Recorded));

        /// <summary>
        /// Read the display-name of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<string>> displayName =>
            Eff<TR, Option<string>>(rt => Optional(rt.CurrentActivity?.DisplayName));

        /// <summary>
        /// Read the operation-name of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<string>> operationName =>
            Eff<TR, Option<string>>(rt => Optional(rt.CurrentActivity?.OperationName));

        /// <summary>
        /// Read the root ID of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<string>> rootId =>
            Eff<TR, Option<string>>(rt => Optional(rt.CurrentActivity?.RootId));

        /// <summary>
        /// Read the span ID of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<ActivitySpanId>> spanId =>
            Eff<TR, Option<ActivitySpanId>>(rt => Optional(rt.CurrentActivity?.SpanId));

        /// <summary>
        /// Read the start-time of the current activity
        /// </summary>
        /// <remarks>None if there is no current activity</remarks>
        public static Eff<TR, Option<DateTime>> startTimeUTC =>
            Eff<TR, Option<DateTime>>(rt => Optional(rt.CurrentActivity?.StartTimeUtc));
    }
}
