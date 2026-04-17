using System;
using System.Threading;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// TileMap hardware tile aligned with <see cref="EditorSnapReactiveCommandBase"/>: subscriber + context refresh.
/// Event handlers only signal a repaint; icon/label state is evaluated lazily from the bridge snapshot cache
/// in <see cref="GetCommandImage"/> and <see cref="GetCommandDisplayName"/>.
/// </summary>
public abstract class TileMapToolReactiveCommandBase : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly String _displayName;
    private readonly String _toolKey;
    private readonly String _surfaceLabel;

    // ── Throttled diagnostics ─────────────────────────────────────────────────
    /// <summary>Tick of the last emitted diagnostic log (shared across all instances).</summary>
    private static Int64 _diagLastTick;
    /// <summary>Total times OnContextChanged/OnSnapshot fired (to confirm they are arriving).</summary>
    private static Int32 _diagEventCount;

    protected TileMapToolReactiveCommandBase(
        String displayName,
        String description,
        String toolKey,
        String surfaceLabel)
        : base(displayName, description, "TileMap")
    {
        _displayName  = displayName;
        _toolKey      = toolKey;
        _surfaceLabel = surfaceLabel;
        this.DisableLoupedeckLocalization();
    }

    protected override Boolean OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null)
            Bridge.ContextChanged += OnContextChanged;
        PluginLog.Info($"[TM-DIAG] OnLoad key={_toolKey}");
        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        if (Bridge != null)
            Bridge.ContextChanged -= OnContextChanged;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    private void OnContextChanged()
    {
        var n = Interlocked.Increment(ref _diagEventCount);
        // Log the first one, then every 20 events to avoid spam.
        if (n == 1 || n % 20 == 0)
            LogSnapState("CtxChanged", n);
        ActionImageChanged(actionParameter: null);
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot)
    {
        var n = Interlocked.Increment(ref _diagEventCount);
        if (n == 1 || n % 20 == 0)
            LogSnapState("Snapshot", n);
        ActionImageChanged(actionParameter: null);
    }

    protected override void RunCommand(String actionParameter)
    {
        PluginLog.Info($"[TM-DIAG] RunCommand key={_toolKey}");
        TileMapBridgeCommands.SendToolTrigger(_toolKey);
        Bridge?.RequestFreshSnapshot();
        ActionImageChanged(actionParameter: null);
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        var lit = SvgIcons.IsTileMapToolLit(_toolKey, snap);
        // Throttled log: only when relevant state changes, or every 5s for this tool.
        LogGetImageDiag(snap, lit);
        return SvgIcons.GetTileMapToolSurfaceIcon(_toolKey, lit, imageSize);
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        _displayName;

    // ── Diagnostic helpers ────────────────────────────────────────────────────

    private void LogSnapState(String trigger, Int32 eventCount)
    {
        if (Bridge == null) { PluginLog.Info($"[TM-DIAG] {trigger} #{eventCount} key={_toolKey} — Bridge=null"); return; }
        Bridge.TryReadSnapshot(out var snap);
        EmitSnapLog(trigger, eventCount, snap);
    }

    private void LogGetImageDiag(ContextSnapshot snap, Boolean lit)
    {
        // Log only for the "paint" tool (avoids 7x repetition), throttled to 5s.
        if (_toolKey != "paint") return;
        var now = Environment.TickCount64;
        var last = Interlocked.Read(ref _diagLastTick);
        if (unchecked(now - last) < 5000) return;
        Interlocked.Exchange(ref _diagLastTick, now);
        EmitSnapLog("GetImg", -1, snap);
    }

    private void EmitSnapLog(String trigger, Int32 count, ContextSnapshot snap)
    {
        var tb = snap.TileMapToolbar;
        var keys = new[] { "paint", "line", "rect", "bucket", "picker", "eraser", "random_tile" };
        var sb = new System.Text.StringBuilder();
        foreach (var k in keys)
        {
            if (sb.Length > 0) sb.Append(' ');
            tb.TryGetValue(k, out var v);
            sb.Append($"{k}={(v ? 1 : 0)}");
        }
        var countStr = count >= 0 ? $"#{count} " : "";
        PluginLog.Info(
            $"[TM-DIAG] {trigger} {countStr}key={_toolKey} " +
            $"HasTM={snap.HasTileMap} active='{snap.TileMapActiveTool}' " +
            $"IsTMLit={SvgIcons.IsTileMapToolLit(_toolKey, snap)} || {sb}");
    }
}
