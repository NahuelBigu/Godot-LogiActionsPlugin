namespace Loupedeck.GodotMxBridge;

public class OpenScriptTabCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public OpenScriptTabCommand() : base("Focus Script Tab", "Switch to the Script editor tab", "Navigation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.FocusScriptTab);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetReactiveIcon("rt_focus_script", new ContextSnapshot());
}
