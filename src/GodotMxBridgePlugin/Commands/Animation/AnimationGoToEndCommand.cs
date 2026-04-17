namespace Loupedeck.GodotMxBridge;

/// <summary>Play the current animation backwards from the end.</summary>
public sealed class AnimationGoToEndCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationGoToEndCommand()
        : base("Anim - Play from End (Reverse)", "Play the animation backwards from the end", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimGoToEnd);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_to_end");
}
