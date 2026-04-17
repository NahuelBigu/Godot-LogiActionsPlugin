namespace Loupedeck.GodotMxBridge;

public class ScriptRunCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptRunCommand() : base("Run Script", "Run current script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScRun);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) => SvgIcons.GetReactiveIcon("sc_run", new ContextSnapshot());
}
