namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformPosZAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformPosZAdjustment() : base("Position Z", "Adjust Position Z", ActionKeys.TfPosZ) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfPosZ, 0.0);
}
