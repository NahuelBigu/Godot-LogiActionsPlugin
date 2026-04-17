namespace Loupedeck.GodotMxBridge;

/// <summary>Add a new track to the current animation (opens the track-type picker).</summary>
public sealed class AnimationNewTrackCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationNewTrackCommand()
        : base("Anim - New Track", "Add a new track to the current animation", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimNewTrack);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_new_track");
}
