namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformRotYAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformRotYAdjustment() : base("Rotation Y", "Adjust Rotation Y", ActionKeys.TfRotY) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfRotY, 0.0);
}
