namespace MyNetworkTime.Core.Protocols;

internal static class Rfc868TimeConverter
{
    private static readonly DateTimeOffset Epoch = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset ReadTimestamp(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 4)
        {
            throw new InvalidOperationException("RFC868 response must contain exactly 4 bytes.");
        }

        var seconds = ((uint)buffer[0] << 24) |
                      ((uint)buffer[1] << 16) |
                      ((uint)buffer[2] << 8) |
                      buffer[3];

        return Epoch.AddSeconds(seconds);
    }
}
