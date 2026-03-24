namespace MyNetworkTime.Core.Platforms;

public sealed record PlatformActionResult(
    bool Succeeded,
    string Message)
{
    public static PlatformActionResult Success(string message) => new(true, message);

    public static PlatformActionResult Failure(string message) => new(false, message);
}
