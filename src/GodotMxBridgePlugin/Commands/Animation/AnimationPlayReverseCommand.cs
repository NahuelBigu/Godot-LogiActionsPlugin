namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Reverse play toggle: start reverse when stopped/paused, pause when currently playing.
/// Mirrors <see cref="AnimationPlayPauseCommand"/> behavior while using reverse playback as "play" action.
/// </summary>
public sealed class AnimationPlayReverseCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationPlayReverseCommand()
        : base("Anim - Play Reverse", "Play the animation backwards from the current position", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null) Bridge.ContextChanged += Refresh;
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        if (Bridge != null) Bridge.ContextChanged -= Refresh;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot _) => Refresh();
    private void Refresh() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(string actionParameter)
    {
        Bridge.TryReadSnapshot(out var snap);
        Bridge.SendTrigger(snap.AnimationPlaying ? EventIds.AnimPause : EventIds.AnimPlayReverse);
        Refresh();
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetAnimIcon(snap.AnimationPlaying ? "anim_pause" : "anim_play_reverse");
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        if (snap.AnimationPlaying) return "Pause";
        if (snap.AnimationPaused) return "Resume Reverse";
        return "Play Reverse";
    }
}
