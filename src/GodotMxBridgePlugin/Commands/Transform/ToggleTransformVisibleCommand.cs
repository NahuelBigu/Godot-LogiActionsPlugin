namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the eye icon tracks
/// <see cref="ContextSnapshot.Visible"/> from Godot after each HTTP poll (same path as transform dials).
/// </summary>
public class ToggleTransformVisibleCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasTransform;
    private Boolean? _lastVisible;

    public ToggleTransformVisibleCommand()
        : base("Toggle Visibility", "Toggle Node2D/Node3D visibility", "Transform")
    {
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
        var has = snapshot.HasTransformNode;
        var vis = snapshot.Visible;
        if (_lastHasTransform == has && _lastVisible == vis) return;
        _lastHasTransform = has;
        _lastVisible      = vis;
        RefreshCommandSurface();
    }

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(String actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasTransformNode)
        {
            Bridge.SendBool(EventIds.TfVisible, !s.Visible);
            _lastHasTransform = null;
            _lastVisible      = null;
            RefreshCommandSurface();
        }
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("n3d_vis", snap);
    }
}
