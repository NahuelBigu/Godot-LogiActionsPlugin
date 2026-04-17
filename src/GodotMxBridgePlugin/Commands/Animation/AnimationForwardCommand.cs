namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Step forward one frame (Ctrl+Right / <c>animation_editor/goto_next_step</c>).
/// Long-press: jump to the very end of the animation (<c>mx.anim.goto_end</c>).
/// </summary>
public sealed class AnimationForwardCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationForwardCommand()
        : base("Anim - Forward", "Step forward - Long press: jump to end", "Animation")
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
