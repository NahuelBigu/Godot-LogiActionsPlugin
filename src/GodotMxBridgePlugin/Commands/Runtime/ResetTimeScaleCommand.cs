namespace Loupedeck.GodotMxBridge;

public class ResetTimeScaleCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ResetTimeScaleCommand() : base("Reset Time Scale", "Reset time scale to 1.0x", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ResetTimeScale);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("rt_reset_ts", new ContextSnapshot());
}
