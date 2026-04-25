namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Cycles the loop mode of the current animation: off → linear → ping-pong → off.
/// Icon reacts to <see cref="ContextSnapshot.AnimationLoopMode"/>.
/// </summary>
public sealed class AnimationLoopToggleCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasAnim;
    private Int32? _lastLoopMode;

    public AnimationLoopToggleCommand()
        : base("Anim - Toggle Loop", "Cycle animation loop: off, repeat, ping-pong", "Animation")
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
        var m = s.AnimationLoopMode;
        if (_lastHasAnim == h && _lastLoopMode == m) return;
        _lastHasAnim   = h;
        _lastLoopMode  = m;
        ActionImageChanged(actionParameter: null);
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimToggleLoop);
        _lastHasAnim  = null;
        _lastLoopMode = null;
        ActionImageChanged(actionParameter: null);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetAnimationLoopIcon(snap.AnimationLoopMode);
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return snap.AnimationLoopMode switch
        {
            1 => "Loop ON",
            2 => "Ping-Pong",
            _ => "Loop OFF",
        };
    }
}
