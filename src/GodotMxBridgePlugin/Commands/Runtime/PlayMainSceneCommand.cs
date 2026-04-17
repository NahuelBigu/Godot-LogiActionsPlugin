namespace Loupedeck.GodotMxBridge;

public class PlayMainSceneCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public PlayMainSceneCommand() : base("Play Main Scene", "Run the main scene (F5)", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.PlayMain);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("idle_play_main", new ContextSnapshot());
}
