namespace Loupedeck.GodotMxBridge;

public class Open2DTabCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public Open2DTabCommand() : base("Focus 2D Tab", "Switch to the 2D editor tab", "Navigation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.Focus2DTab);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetReactiveIcon("rt_focus_2d", new ContextSnapshot());
}
