namespace Loupedeck.GodotMxBridge;

public class PlayCurrentCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public PlayCurrentCommand() : base("Play Current Scene", "Run the currently edited scene", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.PlayCurrent);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("idle_play_cur", new ContextSnapshot());
}
