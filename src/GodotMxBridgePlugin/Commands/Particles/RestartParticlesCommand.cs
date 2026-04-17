namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the button state can refresh when
/// particle context changes (same poll path as other particle actions).
/// </summary>
public class RestartParticlesCommand : PluginDynamicCommand, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public RestartParticlesCommand()
        : base("Restart Particles", "Restart particle emission (GPU/CPU 2D/3D)", "Particles")
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

    private void OnBridgeContextChanged() => RefreshCommandSurface();

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot) => RefreshCommandSurface();

    private void RefreshCommandSurface() => ActionImageChanged(actionParameter: null);

    protected override void RunCommand(string actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var s) && s.HasParticles)
        {
            Bridge.SendTrigger(EventIds.PtRestart);
            RefreshCommandSurface();
        }
    }
    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("pt_restart", snap);
    }
}
