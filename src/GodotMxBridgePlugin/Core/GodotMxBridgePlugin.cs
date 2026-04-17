using Loupedeck;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Plugin entry point. Creates the bridge transport and starts polling.
/// All DynamicFolders, Commands, and Adjustments resolve the transport via
/// <see cref="Bridge"/> (singleton per plugin instance).
/// </summary>
public class GodotMxBridgePlugin : Plugin
{
    /// <summary>
    /// Universal actions: RunCommand fires regardless of foreground app.
    /// Profile still auto-switches when Godot gains focus via GodotMxBridgeApplication.
    /// </summary>
    public override Boolean UsesApplicationApiOnly => true;

    /// <summary>
    /// false = link to GodotMxBridgeApplication so the profile activates when Godot is focused.
    /// </summary>
    public override Boolean HasNoApplication => false;

    /// <summary>
    /// Singleton bridge transport accessible by all SDK-discovered classes.
    /// Explicitly static because Loupedeck SDK creates plugin instances and dynamic folders/commands
    /// independently using reflection, meaning we cannot pass dependencies via constructors.
    /// This singleton ensures all controls share the same connection lifecycle.
    /// </summary>
    internal static IBridgeTransport Bridge { get; private set; } = null!;

    /// <summary>
    /// Defers <see cref="HttpBridgeTransport.Start"/> briefly so Logi Plugin Service is not hit with HTTP
    /// from the same moment as <see cref="Load"/> (work runs on a pool thread; this only staggers startup).
    /// Together with <see cref="BridgePollSchedule.InitialDelayMs"/>, first context poll is ~≤4s after load.
    /// </summary>
    private static readonly TimeSpan BridgePollStartDelay = TimeSpan.FromSeconds(2);

    private HttpBridgeTransport? _httpTransport;
    private CancellationTokenSource? _bridgeStartDelayCts;

    public GodotMxBridgePlugin()
    {
        PluginLog.Init(this.Log);
    }

    public override void Load()
    {
        PluginLog.Info("GodotMxBridge plugin loading...");

        var transport = new HttpBridgeTransport();
        _httpTransport = transport;
        Bridge = transport;
        transport.GodotServiceReachableChanged += OnGodotServiceReachableChanged;

        ReportBridgeNotReady(GodotBridgeStatusMessages.DefaultNotConnected);

        _bridgeStartDelayCts = new CancellationTokenSource();
        var token = _bridgeStartDelayCts.Token;
        var t = transport;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(BridgePollStartDelay, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                    return;
                t.Start();
            }
            catch (OperationCanceledException)
            {
                /* unload before delay elapsed */
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"GodotMxBridge: deferred bridge start failed: {ex.Message}");
            }
        }, token);

        PluginLog.Info("GodotMxBridge plugin loaded (bridge polling deferred).");
    }

    public override void Unload()
    {
        PluginLog.Info("GodotMxBridge plugin unloading...");
        try
        {
            _bridgeStartDelayCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            /* ignore */
        }

        _bridgeStartDelayCts?.Dispose();
        _bridgeStartDelayCts = null;

        if (_httpTransport != null)
        {
            _httpTransport.GodotServiceReachableChanged -= OnGodotServiceReachableChanged;
            _httpTransport.Stop();
            _httpTransport = null;
        }
    }

    /// <summary>
    /// Error + help URL: <a href="https://logitech.github.io/actions-sdk-docs/csharp/plugin-features/plugin-status/">Plugin Status</a>.
    /// </summary>
    private void ReportBridgeNotReady(string message)
    {
        var loc = this.Localization;
        var localizedMessage = loc.GetString(message, message);
        var localizedLink = loc.GetString(GodotBridgeStatusMessages.SetupHelpLinkTitle, GodotBridgeStatusMessages.SetupHelpLinkTitle);
        this.OnPluginStatusChanged(
            global::Loupedeck.PluginStatus.Error,
            localizedMessage,
            GodotBridgeStatusMessages.SetupHelpUrl,
            localizedLink);
    }

    /// <summary>Updates status when the Godot HTTP bridge is reachable.</summary>
    private void OnGodotServiceReachableChanged(bool reachable)
    {
        if (reachable)
        {
            this.OnPluginStatusChanged(global::Loupedeck.PluginStatus.Normal, null);
            return;
        }

        ReportBridgeNotReady(GodotBridgeStatusMessages.BridgeDisconnected);
    }
}
