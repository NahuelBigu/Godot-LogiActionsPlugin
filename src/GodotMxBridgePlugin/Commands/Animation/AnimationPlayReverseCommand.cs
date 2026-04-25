namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Reverse play toggle: start reverse when stopped/paused, pause when currently playing.
/// Mirrors <see cref="AnimationPlayPauseCommand"/> behavior while using reverse playback as "play" action.
/// </summary>
public sealed class AnimationPlayReverseCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasAnim;
    private Boolean? _lastPlaying;
    private Boolean? _lastPaused;

    public AnimationPlayReverseCommand()
        : base("Anim - Play Reverse", "Play the animation backwards from the current position", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot s)
    {
        var h = s.HasAnimation;
        var p = s.AnimationPlaying;
        var u = s.AnimationPaused;
        if (_lastHasAnim == h && _lastPlaying == p && _lastPaused == u) return;
        _lastHasAnim  = h;
        _lastPlaying  = p;
        _lastPaused   = u;
        ActionImageChanged(actionParameter: null);
    }

    protected override void RunCommand(string actionParameter)
    {
        Bridge.TryReadSnapshot(out var snap);
        Bridge.SendTrigger(snap.AnimationPlaying ? EventIds.AnimPause : EventIds.AnimPlayReverse);
        _lastHasAnim = null;
        ActionImageChanged(actionParameter: null);
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
