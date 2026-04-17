namespace Loupedeck.GodotMxBridge;

public class Open3DTabCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public Open3DTabCommand() : base("Focus 3D Tab", "Switch to the 3D editor tab", "Navigation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.Focus3DTab);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetReactiveIcon("rt_focus_3d", new ContextSnapshot());
}
