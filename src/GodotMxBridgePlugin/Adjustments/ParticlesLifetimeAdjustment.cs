namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the dial readout tracks
/// <see cref="ContextSnapshot.ParticlesLifetime"/> after each context poll (same path as transform dials).
/// </summary>
public class ParticlesLifetimeAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ParticlesLifetimeAdjustment()
        : base("Particles Lifetime", "Dial: lifetime (0.01s steps; same encoder behavior as speed dial)", "Particles", hasReset: false)
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
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (!Bridge.TryReadSnapshot(out var s) || !s.HasParticles || diff == 0) return;
        Bridge.SendFloat(EventIds.PtLifetime, ParticleLifetimeDialHelper.ApplyEncoderDiff(s.ParticlesLifetime, diff));
    }

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles) return "—";
        return $"{snap.ParticlesLifetime:F2}s";
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("lifetime_dial");
}
