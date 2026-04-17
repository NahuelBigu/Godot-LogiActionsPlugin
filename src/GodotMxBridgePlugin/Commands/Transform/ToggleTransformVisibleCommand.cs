namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the eye icon tracks
/// <see cref="ContextSnapshot.Visible"/> from Godot after each HTTP poll (same path as transform dials).
/// </summary>
public class ToggleTransformVisibleCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ToggleTransformVisibleCommand()
        : base("Toggle Visibility", "Toggle Node2D/Node3D visibility", "Transform")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override Boolean OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null) Bridge.ContextChanged += OnBridgeContextChanged;
        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        if (Bridge != null) Bridge.ContextChanged -= OnBridgeContextChanged;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    private void OnBridgeContextChanged() => RefreshCommandSurface();

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot) => RefreshCommandSurface();

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(String actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasTransformNode)
        {
            Bridge.SendBool(EventIds.TfVisible, !s.Visible);
            RefreshCommandSurface();
        }
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("n3d_vis", snap);
    }
}
