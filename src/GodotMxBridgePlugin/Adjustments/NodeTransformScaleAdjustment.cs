namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformScaleAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformScaleAdjustment() : base("Scale", "Adjust Uniform Scale", ActionKeys.TfScale) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfScale, 1.0);
}
