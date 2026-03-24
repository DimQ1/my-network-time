using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Sync;

public static class TimeAdjustmentPolicy
{
    public static TimeAdjustmentDecision Evaluate(
        AppSettingsSnapshot settings,
        TimeSpan offset,
        TimeAdjustmentAvailability availability)
    {
        var offsetMagnitude = TimeSpan.FromTicks(Math.Abs(offset.Ticks));
        var threshold = settings.AdjustmentThreshold.ToTimeSpan();

        if (settings.AdjustmentMode == TimeAdjustmentMode.DoNotUpdateTime)
        {
            return new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.MonitoringOnly,
                Summary: "Time is synchronized in monitor mode.",
                Guidance: "The app is configured to observe network time without changing the system clock.");
        }

        if (offsetMagnitude < threshold)
        {
            return new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.WithinThreshold,
                Summary: "Time is synchronized within the configured adjustment threshold.",
                Guidance: $"No clock change was needed because the drift stayed below {settings.AdjustmentThreshold}.");
        }

        if (!availability.IsSupported || !availability.CanAdjustNow)
        {
            return new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.Unavailable,
                Summary: availability.RequiresElevation
                    ? "Time sample captured, but clock adjustment requires elevated permissions."
                    : "Time sample captured, but direct clock adjustment is not available on this platform.",
                Guidance: availability.Guidance);
        }

        return settings.AdjustmentMode switch
        {
            TimeAdjustmentMode.AdjustSystemTime => new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.AutoAdjust,
                Summary: "Time synchronized and system clock adjusted.",
                Guidance: "The app will apply the sampled network time immediately."),
            TimeAdjustmentMode.AskUser => new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.AwaitingUserConfirmation,
                Summary: "Time sample ready. Confirm the system clock adjustment from the dashboard.",
                Guidance: "Manual confirmation is required before the system clock changes."),
            _ => new TimeAdjustmentDecision(
                Kind: TimeAdjustmentDecisionKind.Unavailable,
                Summary: "Clock adjustment mode is not available.",
                Guidance: "Review the selected time adjustment mode in Settings.")
        };
    }
}

public sealed record TimeAdjustmentDecision(
    TimeAdjustmentDecisionKind Kind,
    string Summary,
    string Guidance);

public enum TimeAdjustmentDecisionKind
{
    MonitoringOnly = 0,
    WithinThreshold = 1,
    AutoAdjust = 2,
    AwaitingUserConfirmation = 3,
    Unavailable = 4
}
