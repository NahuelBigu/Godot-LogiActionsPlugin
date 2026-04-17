namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Step backward one frame (Ctrl+Left / <c>animation_editor/goto_prev_step</c>).
/// Long-press: jump to the very start of the animation (<c>mx.anim.goto_start</c>).
/// </summary>
public sealed class AnimationBackwardCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationBackwardCommand()
        : base("Anim - Backward", "Step backward - Long press: jump to start", "Animation")
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
        Bridge.SendTrigger(EventIds.AnimStepBackward);

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_backward");

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        if (!snap.HasAnimation) return "Backward";
        return $"◄ {snap.AnimationPosition:F3}s";
    }

    protected override bool ProcessTouchEvent(string actionParameter, DeviceTouchEvent touchEvent)
    {
        if (touchEvent.EventType == DeviceTouchEventType.LongRelease)
            Bridge.SendTrigger(EventIds.AnimGoToStart);
        return base.ProcessTouchEvent(actionParameter, touchEvent);
    }
}
