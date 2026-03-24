using System.Runtime.InteropServices;
using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.App.Services;

internal sealed class PlatformTimeAdjustmentService : ITimeAdjustmentService
{
    public TimeAdjustmentAvailability GetAvailability()
    {
#if WINDOWS
        return IsRunningElevated()
            ? TimeAdjustmentAvailability.Available(
                summary: "Windows can query network time and adjust the system clock directly from this app session.",
                guidance: "Automatic adjustment follows the selected sync mode and threshold while the app is running.")
            : TimeAdjustmentAvailability.NeedsElevation(
                summary: "Windows supports direct clock adjustment, but this app session is not elevated.",
                guidance: "Run the app as administrator to change the system clock, or open Date & Time settings for a manual correction.");
#elif ANDROID
        return TimeAdjustmentAvailability.Unsupported(
            summary: "Android can compare network time, but normal apps do not usually have permission to change the system clock.",
            guidance: "Use the sampled drift information, then open Android Date & Time settings if correction is needed.");
#elif IOS
        return TimeAdjustmentAvailability.Unsupported(
            summary: "iOS can compare network time, but public APIs do not allow apps to change the system clock directly.",
            guidance: "Open Settings > General > Date & Time manually if you need to review automatic time configuration.");
#else
        return TimeAdjustmentAvailability.Unsupported(
            summary: "Direct time adjustment is not available on this platform.",
            guidance: "Use monitor mode and system settings guidance instead.");
#endif
    }

    public ValueTask<TimeAdjustmentResult> TryAdjustAsync(DateTimeOffset targetUtc, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

#if WINDOWS
        var availability = GetAvailability();
        if (!availability.CanAdjustNow)
        {
            return ValueTask.FromResult(TimeAdjustmentResult.Failure(availability.Guidance));
        }

        var utc = targetUtc.ToUniversalTime();
        var systemTime = new SystemTime
        {
            Year = checked((ushort)utc.Year),
            Month = checked((ushort)utc.Month),
            Day = checked((ushort)utc.Day),
            Hour = checked((ushort)utc.Hour),
            Minute = checked((ushort)utc.Minute),
            Second = checked((ushort)utc.Second),
            Milliseconds = checked((ushort)utc.Millisecond)
        };

        if (SetSystemTime(ref systemTime))
        {
            return ValueTask.FromResult(TimeAdjustmentResult.Success("Windows system clock updated from the latest network sample."));
        }

        var errorCode = Marshal.GetLastWin32Error();
        var message = errorCode switch
        {
            5 => "Windows denied access while changing the system time.",
            1314 => "Windows denied the time change because the app is not running as administrator.",
            _ => $"SetSystemTime failed with Win32 error {errorCode}."
        };

        return ValueTask.FromResult(TimeAdjustmentResult.Failure(message));
#else
        return ValueTask.FromResult(TimeAdjustmentResult.Failure(GetAvailability().Guidance));
#endif
    }

#if WINDOWS
    private static bool IsRunningElevated()
    {
        using var identity = global::System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new global::System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(global::System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetSystemTime(ref SystemTime systemTime);

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Milliseconds;
    }
#endif
}
