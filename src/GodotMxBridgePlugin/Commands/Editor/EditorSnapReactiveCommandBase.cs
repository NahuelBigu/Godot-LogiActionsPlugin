using System;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Editor shortcut command with reactive icon/label from <see cref="ContextSnapshot"/> snap flags
/// (Godot toolbar toggles), via <see cref="GodotContextBroadcastService"/> only.
/// </summary>
public abstract class EditorSnapReactiveCommandBase : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly String _shortcutPath;
    private readonly String _shortLabel;
    private readonly Func<ContextSnapshot, Boolean> _isActive;
    private Boolean? _lastPaintedActive;

    protected EditorSnapReactiveCommandBase(
        String displayName,
        String description,
        String group,
        String shortcutPath,
        String shortLabel,
        Func<ContextSnapshot, Boolean> isActive)
        : base(displayName, description, group)
    {
        _shortcutPath  = shortcutPath;
        _shortLabel    = shortLabel;
        _isActive      = isActive;
        this.DisableLoupedeckLocalization();
    }

    protected override Boolean OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot)
    {
        var active = _isActive(snapshot);
        if (_lastPaintedActive == active) return;
        _lastPaintedActive = active;
        RefreshCommandSurface();
    }

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(String actionParameter)
    {
        Bridge.SendEditorShortcut(_shortcutPath);
        Bridge.RequestFreshSnapshot();
        _lastPaintedActive = null;
        RefreshCommandSurface();
    }

    protected abstract BitmapImage GetSnapIcon(Boolean active);

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return GetSnapIcon(_isActive(snap));
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        if (!Bridge.TryReadSnapshot(out var snap))
            return _shortLabel;
        return _isActive(snap) ? $"{_shortLabel} - on" : $"{_shortLabel} - off";
    }
}
