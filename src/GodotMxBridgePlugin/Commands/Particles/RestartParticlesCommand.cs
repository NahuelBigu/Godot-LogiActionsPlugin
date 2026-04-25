namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the button state can refresh when
/// particle context changes (same poll path as other particle actions).
/// </summary>
public class RestartParticlesCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasParticles;

    public RestartParticlesCommand()
        : base("Restart Particles", "Restart particle emission (GPU/CPU 2D/3D)", "Particles")
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
        if (_lastHasParticles == has) return;
        _lastHasParticles = has;
        RefreshCommandSurface();
    }

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(string actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasParticles)
        {
            Bridge.SendTrigger(EventIds.PtRestart);
            _lastHasParticles = null;
            RefreshCommandSurface();
        }
    }
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("pt_restart", snap);
    }
}
