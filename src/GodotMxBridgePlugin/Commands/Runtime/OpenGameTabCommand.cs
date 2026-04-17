namespace Loupedeck.GodotMxBridge;

public class OpenGameTabCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public OpenGameTabCommand() : base("Focus Game Tab", "Switch to the Game tab in the editor", "Navigation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.FocusGame);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("rt_focus_game", new ContextSnapshot());
}
