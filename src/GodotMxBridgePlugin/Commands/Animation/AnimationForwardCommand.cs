namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Step forward one frame (Ctrl+Right / <c>animation_editor/goto_next_step</c>).
/// Long-press: jump to the very end of the animation (<c>mx.anim.goto_end</c>).
/// </summary>
public sealed class AnimationForwardCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasAnim;
    private Double? _lastPositionRounded;

    public AnimationForwardCommand()
        : base("Anim - Forward", "Step forward - Long press: jump to end", "Animation")
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
        var t = h ? Math.Round(s.AnimationPosition, 3) : (Double?)null;
        if (_lastHasAnim == h && _lastPositionRounded == t) return;
        _lastHasAnim           = h;
        _lastPositionRounded   = t;
        ActionImageChanged(actionParameter: null);
    }

    protected override void RunCommand(string actionParameter) =>
        Bridge.SendTrigger(EventIds.AnimStepForward);

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_forward");

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        if (!snap.HasAnimation) return "Forward";
        return $"► {snap.AnimationPosition:F3}s";
    }

    protected override bool ProcessTouchEvent(string actionParameter, DeviceTouchEvent touchEvent)
    {
        if (touchEvent.EventType == DeviceTouchEventType.LongRelease)
            Bridge.SendTrigger(EventIds.AnimGoToEnd);
        return base.ProcessTouchEvent(actionParameter, touchEvent);
    }
}
