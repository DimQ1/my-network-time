namespace MyNetworkTime.Core.Protocols;

internal static class NtpTimestampConverter
{
    private static readonly DateTimeOffset Epoch = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset ReadTimestamp(ReadOnlySpan<byte> buffer, int offset)
    {
        var seconds = ReadUInt32BigEndian(buffer, offset);
        var fraction = ReadUInt32BigEndian(buffer, offset + 4);
        var milliseconds = (fraction * 1000d) / 0x1_0000_0000d;

        return Epoch.AddSeconds(seconds).AddMilliseconds(milliseconds);
    }

    public static void WriteTimestamp(Span<byte> buffer, int offset, DateTimeOffset timestamp)
    {
        var timestampUtc = timestamp.ToUniversalTime();
        var totalSeconds = (timestampUtc - Epoch).TotalSeconds;
        var seconds = (uint)Math.Floor(totalSeconds);
        var fraction = (uint)Math.Round((totalSeconds - seconds) * 0x1_0000_0000d);

        WriteUInt32BigEndian(buffer, offset, seconds);
        WriteUInt32BigEndian(buffer, offset + 4, fraction);
    }

    private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> buffer, int offset)
    {
        return ((uint)buffer[offset] << 24) |
               ((uint)buffer[offset + 1] << 16) |
               ((uint)buffer[offset + 2] << 8) |
               buffer[offset + 3];
    }

    private static void WriteUInt32BigEndian(Span<byte> buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }
}
