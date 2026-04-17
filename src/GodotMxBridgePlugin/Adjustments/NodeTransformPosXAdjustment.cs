namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformPosXAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformPosXAdjustment() : base("Position X", "Adjust Position X", ActionKeys.TfPosX) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfPosX, 0.0);
}
