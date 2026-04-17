namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the icon/label track
/// <see cref="ContextSnapshot.ParticlesEmitting"/> after each context poll (same path as transform visibility).
/// </summary>
public class ToggleEmittingCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ToggleEmittingCommand()
        : base("Toggle Emitting", "Toggle GPU Particles emitting state", "Particles")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null) Bridge.ContextChanged += OnBridgeContextChanged;
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        if (Bridge != null) Bridge.ContextChanged -= OnBridgeContextChanged;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    /// <summary>
    /// Same idea as <see cref="PluginDynamicFolder"/> handlers: <see cref="IBridgeTransport.ContextChanged"/>
    /// also runs from <c>LabelNudge</c> (~1/s), which does not call <see cref="GodotContextBroadcastService.DispatchSnapshot"/>.
    /// Without this, standalone commands miss repaints that dynamic-folder <c>CommandImageChanged</c> gets.
    /// </summary>
    private void OnBridgeContextChanged() => RefreshCommandSurface();

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot) => RefreshCommandSurface();

    /// <summary>
    /// Per PluginApi.xml, use <see cref="Loupedeck.PluginDynamicAction.ActionImageChanged(string)"/> with
    /// <c>null</c> when the action has no parameters — some hosts skip label refresh for the parameterless overload.
    /// </summary>
    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(string actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasParticles)
        {
            Bridge.SendBool(EventIds.PtEmitting, !s.ParticlesEmitting);
            RefreshCommandSurface();
        }
    }
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("pt_emitting", snap);
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        return Bridge.TryReadSnapshot(out var snap) && snap.ParticlesEmitting ? "Emitting" : "Stopped";
    }
}
