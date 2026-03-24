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
}
