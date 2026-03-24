using System.ComponentModel.DataAnnotations;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.App.Features.Settings;

internal sealed class SettingsEditorModel : IValidatableObject
{
    [Required, MaxLength(255)]
    public string Server1Host { get; set; } = string.Empty;

    public ServerProtocol Server1Protocol { get; set; } = ServerProtocol.Sntp;

    [Range(1, 65535)]
    public int Server1Port { get; set; } = 123;

    [MaxLength(255)]
    public string Server2Host { get; set; } = string.Empty;

    public ServerProtocol Server2Protocol { get; set; } = ServerProtocol.Sntp;

    [Range(1, 65535)]
    public int Server2Port { get; set; } = 123;

    [MaxLength(255)]
    public string Server3Host { get; set; } = string.Empty;

    public ServerProtocol Server3Protocol { get; set; } = ServerProtocol.Sntp;

    [Range(1, 65535)]
    public int Server3Port { get; set; } = 123;

    [MaxLength(255)]
    public string Server4Host { get; set; } = string.Empty;

    public ServerProtocol Server4Protocol { get; set; } = ServerProtocol.Sntp;

    [Range(1, 65535)]
    public int Server4Port { get; set; } = 123;

    [MaxLength(255)]
    public string Server5Host { get; set; } = string.Empty;

    public ServerProtocol Server5Protocol { get; set; } = ServerProtocol.Sntp;

    [Range(1, 65535)]
    public int Server5Port { get; set; } = 123;

    [Range(1, 999)]
    public int UpdateIntervalValue { get; set; } = 12;

    public IntervalUnit UpdateIntervalUnit { get; set; } = IntervalUnit.Hour;

    [Range(1, 999)]
    public int RetryIntervalValue { get; set; } = 1;

    public IntervalUnit RetryIntervalUnit { get; set; } = IntervalUnit.Minute;

    public bool DemoteServers { get; set; } = true;

    [Range(1, 20)]
    public int DemoteAfterFailures { get; set; } = 4;

    public bool AllowPeersToSync { get; set; }

    public bool AlwaysProvideTime { get; set; }

    public bool ShowTrayIconAtLogin { get; set; } = true;

    public bool StartAtBoot { get; set; } = true;

    [Range(1, 999)]
    public int MaxFreeRunValue { get; set; } = 24;

    public IntervalUnit MaxFreeRunUnit { get; set; } = IntervalUnit.Hour;

    [Range(1, 999)]
    public int AdjustmentThresholdValue { get; set; } = 2;

    public IntervalUnit AdjustmentThresholdUnit { get; set; } = IntervalUnit.Minute;

    public TimeAdjustmentMode AdjustmentMode { get; set; } = TimeAdjustmentMode.AdjustSystemTime;

    [Range(1, 365)]
    public int AutoCheckEveryDays { get; set; } = 7;

    [Required]
    public string LoggingLevel { get; set; } = "Normal";

    public int ServerCount => 5;

    public static SettingsEditorModel FromSnapshot(AppSettingsSnapshot settings)
    {
        var model = new SettingsEditorModel
        {
            UpdateIntervalValue = settings.UpdateInterval.Value,
            UpdateIntervalUnit = settings.UpdateInterval.Unit,
            RetryIntervalValue = settings.RetryInterval.Value,
            RetryIntervalUnit = settings.RetryInterval.Unit,
            DemoteServers = settings.DemoteServers,
            DemoteAfterFailures = settings.DemoteAfterFailures,
            AllowPeersToSync = settings.AllowPeersToSync,
            AlwaysProvideTime = settings.AlwaysProvideTime,
            ShowTrayIconAtLogin = settings.ShowTrayIconAtLogin,
            StartAtBoot = settings.StartAtBoot,
            MaxFreeRunValue = settings.MaxFreeRun.Value,
            MaxFreeRunUnit = settings.MaxFreeRun.Unit,
            AdjustmentThresholdValue = settings.AdjustmentThreshold.Value,
            AdjustmentThresholdUnit = settings.AdjustmentThreshold.Unit,
            AdjustmentMode = settings.AdjustmentMode,
            AutoCheckEveryDays = settings.AutoCheckEveryDays,
            LoggingLevel = settings.LoggingLevel
        };

        ApplyServer(settings.Servers.ElementAtOrDefault(0), value =>
        {
            model.Server1Host = value.Host;
            model.Server1Protocol = value.Protocol;
            model.Server1Port = value.Port;
        });

        ApplyServer(settings.Servers.ElementAtOrDefault(1), value =>
        {
            model.Server2Host = value.Host;
            model.Server2Protocol = value.Protocol;
            model.Server2Port = value.Port;
        });

        ApplyServer(settings.Servers.ElementAtOrDefault(2), value =>
        {
            model.Server3Host = value.Host;
            model.Server3Protocol = value.Protocol;
            model.Server3Port = value.Port;
        });

        ApplyServer(settings.Servers.ElementAtOrDefault(3), value =>
        {
            model.Server4Host = value.Host;
            model.Server4Protocol = value.Protocol;
            model.Server4Port = value.Port;
        });

        ApplyServer(settings.Servers.ElementAtOrDefault(4), value =>
        {
            model.Server5Host = value.Host;
            model.Server5Protocol = value.Protocol;
            model.Server5Port = value.Port;
        });

        return model;
    }

    public AppSettingsSnapshot ToSnapshot()
    {
        return new AppSettingsSnapshot(
            Servers: BuildServers(),
            UpdateInterval: new TimeInterval(UpdateIntervalValue, UpdateIntervalUnit),
            RetryInterval: new TimeInterval(RetryIntervalValue, RetryIntervalUnit),
            DemoteServers: DemoteServers,
            DemoteAfterFailures: DemoteAfterFailures,
            AllowPeersToSync: AllowPeersToSync,
            AlwaysProvideTime: AlwaysProvideTime,
            ShowTrayIconAtLogin: ShowTrayIconAtLogin,
            StartAtBoot: StartAtBoot,
            MaxFreeRun: new TimeInterval(MaxFreeRunValue, MaxFreeRunUnit),
            AdjustmentThreshold: new TimeInterval(AdjustmentThresholdValue, AdjustmentThresholdUnit),
            AdjustmentMode: AdjustmentMode,
            AutoCheckEveryDays: AutoCheckEveryDays,
            LoggingLevel: LoggingLevel);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var servers = BuildServers();

        if (servers.Count == 0)
        {
            yield return new ValidationResult(
                "Configure at least one time server.",
                [nameof(Server1Host)]);
        }

        var duplicateKey = servers
            .GroupBy(server => $"{server.Host.Trim().ToUpperInvariant()}|{server.Protocol}|{server.Port}")
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateKey is not null)
        {
            yield return new ValidationResult("Duplicate time servers are not allowed.");
        }
    }

    public string GetServerHost(int index) => GetServer(index).Host;

    public void SetServerHost(int index, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        SetServer(index, GetServer(index) with { Host = value });
    }

    public ServerProtocol GetServerProtocol(int index) => GetServer(index).Protocol;

    public void SetServerProtocol(int index, ServerProtocol value) =>
        SetServer(index, GetServer(index) with { Protocol = value });

    public int GetServerPort(int index) => GetServer(index).Port;

    public void SetServerPort(int index, int value) =>
        SetServer(index, GetServer(index) with { Port = value });

    public void MoveServer(int fromIndex, int toIndex)
    {
        ValidateServerIndex(fromIndex);
        ValidateServerIndex(toIndex);

        if (fromIndex == toIndex)
        {
            return;
        }

        var servers = GetServers().ToList();
        var movedServer = servers[fromIndex];

        servers.RemoveAt(fromIndex);
        servers.Insert(toIndex, movedServer);

        ApplyServers(servers);
    }

    private IReadOnlyList<ServerEndpointSettings> BuildServers()
    {
        return new ServerEndpointSettings?[]
        {
            CreateServer(Server1Host, Server1Protocol, Server1Port),
            CreateServer(Server2Host, Server2Protocol, Server2Port),
            CreateServer(Server3Host, Server3Protocol, Server3Port),
            CreateServer(Server4Host, Server4Protocol, Server4Port),
            CreateServer(Server5Host, Server5Protocol, Server5Port)
        }
        .OfType<ServerEndpointSettings>()
        .ToList();
    }

    private static ServerEndpointSettings? CreateServer(string host, ServerProtocol protocol, int port)
    {
        return string.IsNullOrWhiteSpace(host)
            ? null
            : new ServerEndpointSettings(host.Trim(), protocol, port);
    }

    private static void ApplyServer(ServerEndpointSettings? server, Action<ServerEndpointSettings> apply)
    {
        if (server is not null)
        {
            apply(server);
        }
    }

    private ServerEditorServer GetServer(int index)
    {
        ValidateServerIndex(index);

        return index switch
        {
            0 => new ServerEditorServer(Server1Host, Server1Protocol, Server1Port),
            1 => new ServerEditorServer(Server2Host, Server2Protocol, Server2Port),
            2 => new ServerEditorServer(Server3Host, Server3Protocol, Server3Port),
            3 => new ServerEditorServer(Server4Host, Server4Protocol, Server4Port),
            4 => new ServerEditorServer(Server5Host, Server5Protocol, Server5Port),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    private void SetServer(int index, ServerEditorServer server)
    {
        ValidateServerIndex(index);

        switch (index)
        {
            case 0:
                Server1Host = server.Host;
                Server1Protocol = server.Protocol;
                Server1Port = server.Port;
                break;
            case 1:
                Server2Host = server.Host;
                Server2Protocol = server.Protocol;
                Server2Port = server.Port;
                break;
            case 2:
                Server3Host = server.Host;
                Server3Protocol = server.Protocol;
                Server3Port = server.Port;
                break;
            case 3:
                Server4Host = server.Host;
                Server4Protocol = server.Protocol;
                Server4Port = server.Port;
                break;
            case 4:
                Server5Host = server.Host;
                Server5Protocol = server.Protocol;
                Server5Port = server.Port;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private IReadOnlyList<ServerEditorServer> GetServers() =>
    [
        GetServer(0),
        GetServer(1),
        GetServer(2),
        GetServer(3),
        GetServer(4)
    ];

    private void ApplyServers(IReadOnlyList<ServerEditorServer> servers)
    {
        ArgumentNullException.ThrowIfNull(servers);

        if (servers.Count != ServerCount)
        {
            throw new ArgumentException("Expected exactly five server slots.", nameof(servers));
        }

        for (var index = 0; index < servers.Count; index++)
        {
            SetServer(index, servers[index]);
        }
    }

    private void ValidateServerIndex(int index)
    {
        if (index < 0 || index >= ServerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private sealed record ServerEditorServer(string Host, ServerProtocol Protocol, int Port);
}
