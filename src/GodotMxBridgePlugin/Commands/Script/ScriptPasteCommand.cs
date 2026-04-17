namespace Loupedeck.GodotMxBridge;

public class ScriptPasteCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptPasteCommand() : base("Paste Script", "Paste text in script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScPaste);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("sc_paste", new ContextSnapshot());
}
