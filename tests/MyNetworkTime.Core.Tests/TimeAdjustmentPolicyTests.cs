using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Tests;

public sealed class TimeAdjustmentPolicyTests
{
    [Fact]
    public void Evaluate_ReturnsAutoAdjust_WhenModeIsAutomatic_OffsetExceedsThreshold_AndAdjustmentIsAvailable()
    {
        var settings = AppSettingsDefaults.Create() with
        {
            AdjustmentMode = TimeAdjustmentMode.AdjustSystemTime,
            AdjustmentThreshold = new TimeInterval(1, IntervalUnit.Minute)
        };

        var decision = TimeAdjustmentPolicy.Evaluate(
            settings,
            offset: TimeSpan.FromMinutes(3),
            availability: TimeAdjustmentAvailability.Available("ready", "ready"));

        Assert.Equal(TimeAdjustmentDecisionKind.AutoAdjust, decision.Kind);
    }

    [Fact]
    public void Evaluate_ReturnsAwaitingUserConfirmation_WhenModeIsAskUser_AndAdjustmentIsAvailable()
    {
        var settings = AppSettingsDefaults.Create() with
        {
            AdjustmentMode = TimeAdjustmentMode.AskUser,
            AdjustmentThreshold = new TimeInterval(1, IntervalUnit.Minute)
        };

        var decision = TimeAdjustmentPolicy.Evaluate(
            settings,
            offset: TimeSpan.FromMinutes(2),
            availability: TimeAdjustmentAvailability.Available("ready", "ready"));

        Assert.Equal(TimeAdjustmentDecisionKind.AwaitingUserConfirmation, decision.Kind);
    }

    [Fact]
    public void Evaluate_ReturnsUnavailable_WhenWindowsNeedsElevation()
    {
        var settings = AppSettingsDefaults.Create() with
        {
            AdjustmentMode = TimeAdjustmentMode.AdjustSystemTime,
            AdjustmentThreshold = new TimeInterval(1, IntervalUnit.Minute)
        };

        var decision = TimeAdjustmentPolicy.Evaluate(
            settings,
            offset: TimeSpan.FromMinutes(2),
            availability: TimeAdjustmentAvailability.NeedsElevation("needs elevation", "run elevated"));

        Assert.Equal(TimeAdjustmentDecisionKind.Unavailable, decision.Kind);
        Assert.Equal("run elevated", decision.Guidance);
    }

    [Fact]
    public void Evaluate_ReturnsWithinThreshold_WhenOffsetIsSmall()
    {
        var settings = AppSettingsDefaults.Create() with
        {
            AdjustmentMode = TimeAdjustmentMode.AdjustSystemTime,
            AdjustmentThreshold = new TimeInterval(5, IntervalUnit.Minute)
        };

        var decision = TimeAdjustmentPolicy.Evaluate(
            settings,
            offset: TimeSpan.FromMinutes(2),
            availability: TimeAdjustmentAvailability.Available("ready", "ready"));

        Assert.Equal(TimeAdjustmentDecisionKind.WithinThreshold, decision.Kind);
    }
}
