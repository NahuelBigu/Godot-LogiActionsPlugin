namespace Loupedeck.GodotMxBridge;

public class ScriptNewCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    public ScriptNewCommand() : base("New Script", "Create a new script", "Script")
    {
        this.DisableLoupedeckLocalization();
    }
    protected override void RunCommand(string actionParameter) => Bridge.SendTrigger(EventIds.ScNew);
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetSceneTreeToolbarIcon(ActionKeys.SceneAttachScript, imageSize);
}
