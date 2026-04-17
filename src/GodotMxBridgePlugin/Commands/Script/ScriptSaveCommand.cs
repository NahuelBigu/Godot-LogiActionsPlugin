namespace Loupedeck.GodotMxBridge;

public class ScriptSaveCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptSaveCommand() : base("Save Script", "Save current script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScSave);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("sc_save", new ContextSnapshot());
}
