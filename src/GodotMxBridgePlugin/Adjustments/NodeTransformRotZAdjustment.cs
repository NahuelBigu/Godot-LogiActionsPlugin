namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformRotZAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformRotZAdjustment() : base("Rotation Z", "Adjust Rotation Z", ActionKeys.TfRotZ) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfRotZ, 0.0);
}
