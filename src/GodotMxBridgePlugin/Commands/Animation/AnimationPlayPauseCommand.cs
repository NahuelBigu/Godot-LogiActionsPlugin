namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Single button: Play when stopped, Pause when playing, Play/resume when paused.
/// Icon and label react to <see cref="ContextSnapshot.AnimationPlaying"/> and <see cref="ContextSnapshot.AnimationPaused"/>.
/// </summary>
public sealed class AnimationPlayPauseCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationPlayPauseCommand()
        : base("Anim - Play / Pause", "Play or pause the current animation", "Animation")
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
        Bridge.SendTrigger(snap.AnimationPlaying ? EventIds.AnimPause : EventIds.AnimPlay);
        Refresh();
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
