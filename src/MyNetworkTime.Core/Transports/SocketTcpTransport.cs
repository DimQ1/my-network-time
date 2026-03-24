using System.Net.Sockets;

namespace MyNetworkTime.Core.Transports;

public sealed class SocketTcpTransport(TimeProvider timeProvider) : ITcpTransport
{
    public async ValueTask<TcpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        int expectedBytes,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        using var client = new TcpClient();
        await client.ConnectAsync(host, port, linkedCts.Token);
        await using var stream = client.GetStream();

        if (!payload.IsEmpty)
        {
            await stream.WriteAsync(payload, linkedCts.Token);
            await stream.FlushAsync(linkedCts.Token);
        }

        var buffer = new byte[expectedBytes];
        var read = 0;

        while (read < expectedBytes)
        {
            var chunk = await stream.ReadAsync(buffer.AsMemory(read, expectedBytes - read), linkedCts.Token);
            if (chunk == 0)
            {
                throw new IOException("Unexpected end of stream while reading time server response.");
            }

            read += chunk;
        }

        return new TcpTransportResponse(buffer, timeProvider.GetUtcNow());
    }
}
