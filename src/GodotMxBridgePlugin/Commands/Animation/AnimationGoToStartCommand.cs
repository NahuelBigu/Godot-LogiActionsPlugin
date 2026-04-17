namespace Loupedeck.GodotMxBridge;

/// <summary>Play the current animation forward from the beginning.</summary>
public sealed class AnimationGoToStartCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationGoToStartCommand()
        : base("Anim - Play from Start", "Play the animation forward from the beginning", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimGoToStart);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_to_start");
}
