namespace Loupedeck.GodotMxBridge;

public class ScriptCopyCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptCopyCommand() : base("Copy Script", "Copy text in script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScCopy);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("sc_copy", new ContextSnapshot());
}
