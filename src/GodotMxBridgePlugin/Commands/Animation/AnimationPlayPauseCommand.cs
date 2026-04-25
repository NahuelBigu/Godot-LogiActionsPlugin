namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Single button: Play when stopped, Pause when playing, Play/resume when paused.
/// Icon and label react to <see cref="ContextSnapshot.AnimationPlaying"/> and <see cref="ContextSnapshot.AnimationPaused"/>.
/// </summary>
public sealed class AnimationPlayPauseCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasAnim;
    private Boolean? _lastPlaying;
    private Boolean? _lastPaused;

    public AnimationPlayPauseCommand()
        : base("Anim - Play / Pause", "Play or pause the current animation", "Animation")
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
        Bridge.SendTrigger(snap.AnimationPlaying ? EventIds.AnimPause : EventIds.AnimPlay);
        _lastHasAnim = null;
        ActionImageChanged(actionParameter: null);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetAnimIcon(snap.AnimationPlaying ? "anim_pause" : "anim_play");
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        if (snap.AnimationPlaying) return "Pause";
        if (snap.AnimationPaused)  return "Resume";
        return "Play";
    }
}
