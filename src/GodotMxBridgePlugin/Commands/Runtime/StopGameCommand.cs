namespace Loupedeck.GodotMxBridge;

public class StopGameCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public StopGameCommand() : base("Stop Game", "Stop the running scene", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.Stop);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("rt_stop", new ContextSnapshot());
}
