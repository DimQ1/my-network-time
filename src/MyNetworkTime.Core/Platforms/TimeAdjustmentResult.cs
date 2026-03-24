namespace MyNetworkTime.Core.Platforms;

public sealed record TimeAdjustmentResult(
    bool Succeeded,
    string Message)
{
    public static TimeAdjustmentResult Success(string message) => new(true, message);

    public static TimeAdjustmentResult Failure(string message) => new(false, message);
}
