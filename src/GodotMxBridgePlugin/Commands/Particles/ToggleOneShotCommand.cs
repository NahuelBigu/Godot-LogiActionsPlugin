namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the icon/label track
/// <see cref="ContextSnapshot.ParticlesOneShot"/> after each context poll (same path as transform visibility).
/// </summary>
public class ToggleOneShotCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasParticles;
    private Boolean? _lastOneShot;

    public ToggleOneShotCommand()
        : base("Toggle One Shot", "Toggle GPU Particles one_shot state", "Particles")
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
        var os  = snapshot.ParticlesOneShot;
        if (_lastHasParticles == has && _lastOneShot == os) return;
        _lastHasParticles = has;
        _lastOneShot      = os;
        RefreshCommandSurface();
    }

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(string actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasParticles)
        {
            Bridge.SendBool(EventIds.PtOneShot, !s.ParticlesOneShot);
            _lastHasParticles = null;
            _lastOneShot      = null;
            RefreshCommandSurface();
        }
    }
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("pt_one_shot", snap);
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        return Bridge.TryReadSnapshot(out var snap) && snap.ParticlesOneShot ? "One Shot" : "Loop";
    }
}
