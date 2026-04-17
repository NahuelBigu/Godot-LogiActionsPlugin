namespace Loupedeck.GodotMxBridge;

public class ScriptCutCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptCutCommand() : base("Cut Script", "Cut text in script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScCut);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("sc_cut", new ContextSnapshot());
}
