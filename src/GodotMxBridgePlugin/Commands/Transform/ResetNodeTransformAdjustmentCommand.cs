namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Resets whichever transform dial (Position/Rotation/Scale axis) was last used, for Node2D or Node3D.
/// </summary>
public class ResetNodeTransformAdjustmentCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ResetNodeTransformAdjustmentCommand()
        : base("Reset Active Dial", "Reset the last-used transform dial to its default value", "Transform")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override Boolean OnLoad()
    {
        NodeTransformAdjustmentTracker.ActiveKeyChanged += OnActiveKeyChanged;
        return base.OnLoad();
    }

    protected override Boolean OnUnload()
    {
        NodeTransformAdjustmentTracker.ActiveKeyChanged -= OnActiveKeyChanged;
        return base.OnUnload();
    }

    private void OnActiveKeyChanged() => ActionImageChanged();

    protected override void RunCommand(String actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s0) && !s0.HasTransformNode)
            NodeTransformAdjustmentTracker.Clear();

        var key = NodeTransformAdjustmentTracker.ActiveKey;
        if (key == null || !Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                       || !NodeTransformHelper.AxisApplies(key, snap)) return;

        switch (key)
        {
            case ActionKeys.TfPosX:    Bridge.SendFloat(EventIds.TfPosX,  0.0); break;
            case ActionKeys.TfPosY:    Bridge.SendFloat(EventIds.TfPosY,  0.0); break;
            case ActionKeys.TfPosZ:    Bridge.SendFloat(EventIds.TfPosZ,  0.0); break;
            case ActionKeys.TfRotX:    Bridge.SendFloat(EventIds.TfRotX,  0.0); break;
            case ActionKeys.TfRotY:    Bridge.SendFloat(EventIds.TfRotY,  0.0); break;
            case ActionKeys.TfRotZ:    Bridge.SendFloat(EventIds.TfRotZ,  0.0); break;
            case ActionKeys.TfScale: Bridge.SendFloat(EventIds.TfScale, 1.0); break;
            default: return;
        }
        NodeTransformAdjustmentTracker.NotifyResetApplied(key);
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        bool active = NodeTransformAdjustmentTracker.ActiveKey != null
                      && Bridge.TryReadSnapshot(out var snap)
                      && snap.HasTransformNode
                      && NodeTransformHelper.AxisApplies(NodeTransformAdjustmentTracker.ActiveKey!, snap);
        return SvgIcons.GetTransformResetIcon(active);
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        var key = NodeTransformAdjustmentTracker.ActiveKey;
        if (key == null || !Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                        || !NodeTransformHelper.AxisApplies(key, snap))
            return "Reset active dial";
        return $"Reset {NodeTransformHelper.GetDisplayName(key) ?? key}";
    }
}
