namespace Loupedeck.GodotMxBridge;

public class RestartSceneCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public RestartSceneCommand() : base("Restart Scene", "Restart the current running scene", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.Restart);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("rt_restart", new ContextSnapshot());
}
