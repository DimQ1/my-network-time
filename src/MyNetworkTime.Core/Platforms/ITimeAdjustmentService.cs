namespace MyNetworkTime.Core.Platforms;

public interface ITimeAdjustmentService
{
    TimeAdjustmentAvailability GetAvailability();

    ValueTask<TimeAdjustmentResult> TryAdjustAsync(DateTimeOffset targetUtc, CancellationToken cancellationToken = default);
}
