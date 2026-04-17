namespace Loupedeck.GodotMxBridge;

public sealed class NodeTransformPosYAdjustment : NodeTransformAxisAdjustmentBase
{
    public NodeTransformPosYAdjustment() : base("Position Y", "Adjust Position Y", ActionKeys.TfPosY) { }

    protected override void SendAxisReset(IBridgeTransport bridge) => bridge.SendFloat(EventIds.TfPosY, 0.0);
}
