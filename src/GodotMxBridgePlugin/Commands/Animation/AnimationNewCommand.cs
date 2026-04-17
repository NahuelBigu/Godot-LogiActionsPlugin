namespace Loupedeck.GodotMxBridge;

/// <summary>Create a new animation in the AnimationPlayer (opens the "New Animation" dialog).</summary>
public sealed class AnimationNewCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationNewCommand()
        : base("Anim - New Animation", "Open the New Animation dialog", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) =>
        Bridge.SendTrigger(EventIds.AnimNewAnim);

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_new");
}
