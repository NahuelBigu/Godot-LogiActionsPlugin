namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformRotXAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformRotXAdjustment() : base("Rotation X", "Adjust Rotation X", ActionKeys.TfRotX) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfRotX, 0.0);
}
