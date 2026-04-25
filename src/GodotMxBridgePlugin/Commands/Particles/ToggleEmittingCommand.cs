namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the icon/label track
/// <see cref="ContextSnapshot.ParticlesEmitting"/> after each context poll (same path as transform visibility).
/// </summary>
public class ToggleEmittingCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasParticles;
    private Boolean? _lastEmitting;

    public ToggleEmittingCommand()
        : base("Toggle Emitting", "Toggle GPU Particles emitting state", "Particles")
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

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot)
    {
        var has = snapshot.HasParticles;
        var em  = snapshot.ParticlesEmitting;
        if (_lastHasParticles == has && _lastEmitting == em) return;
        _lastHasParticles = has;
        _lastEmitting     = em;
        RefreshCommandSurface();
    }

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
            _lastHasParticles = null;
            _lastEmitting     = null;
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
