namespace MyNetworkTime.Core.Platforms;

public sealed record TimeAdjustmentAvailability(
    bool IsSupported,
    bool CanAdjustNow,
    bool RequiresElevation,
    string Summary,
    string Guidance)
{
    public static TimeAdjustmentAvailability Unsupported(string summary, string guidance) =>
        new(
            IsSupported: false,
            CanAdjustNow: false,
            RequiresElevation: false,
            Summary: summary,
            Guidance: guidance);

    public static TimeAdjustmentAvailability Available(string summary, string guidance) =>
        new(
            IsSupported: true,
            CanAdjustNow: true,
            RequiresElevation: false,
            Summary: summary,
            Guidance: guidance);

    public static TimeAdjustmentAvailability NeedsElevation(string summary, string guidance) =>
        new(
            IsSupported: true,
            CanAdjustNow: false,
            RequiresElevation: true,
            Summary: summary,
            Guidance: guidance);
}
