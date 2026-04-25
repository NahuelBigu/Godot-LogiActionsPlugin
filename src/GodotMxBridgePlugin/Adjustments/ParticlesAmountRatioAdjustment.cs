namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Subscribes to <see cref="GodotContextBroadcastService"/> so the dial readout tracks
/// <see cref="ContextSnapshot.ParticlesAmountRatio"/> after each context poll (same path as transform dials).
/// </summary>
public class ParticlesAmountRatioAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHas;
    private Boolean? _lastSupports;
    private Double? _lastRatio;

    public ParticlesAmountRatioAdjustment()
        : base("Particles Amount Ratio", "Dial: 1% steps (0–100% amount ratio)", "Particles", hasReset: false)
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
        var sup = snapshot.ParticlesSupportsAmountRatio;
        var r   = has && sup ? Math.Round(snapshot.ParticlesAmountRatio, 5) : (Double?)null;
        if (_lastHas == has && _lastSupports == sup && _lastRatio == r) return;
        _lastHas       = has;
        _lastSupports  = sup;
        _lastRatio     = r;
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (!Bridge.TryReadSnapshot(out var s) || !s.HasParticles || !s.ParticlesSupportsAmountRatio || diff == 0) return;
        var newAr = ParticleAmountRatioHelper.ApplyEncoderDiff(s.ParticlesAmountRatio, diff);
        Bridge.SendFloat(EventIds.PtAmountRatio, newAr);
        AdjustmentValueChanged();
    }

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles || !snap.ParticlesSupportsAmountRatio) return "—";
        return $"{snap.ParticlesAmountRatio:P0}";
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("amount_ratio_dial");
}
