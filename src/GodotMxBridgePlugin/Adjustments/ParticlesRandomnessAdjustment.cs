namespace Loupedeck.GodotMxBridge;

public class ParticlesRandomnessAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHas;
    private Double? _lastVal;

    public ParticlesRandomnessAdjustment()
        : base("Particles Randomness", "Dial: emission randomness ratio (1% steps)", "Particles", hasReset: false)
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
        var v   = has ? Math.Round(snapshot.ParticlesRandomness, 5) : (Double?)null;
        if (_lastHas == has && _lastVal == v) return;
        _lastHas = has;
        _lastVal = v;
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (!Bridge.TryReadSnapshot(out var s) || !s.HasParticles || diff == 0) return;
        var next = ParticleAmountRatioHelper.ApplyEncoderDiff(s.ParticlesRandomness, diff);
        Bridge.SendFloat(EventIds.PtRandomness, next);
        AdjustmentValueChanged();
    }

    protected override string GetAdjustmentValue(string actionParameter) =>
        !Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles
            ? "—"
            : $"{snap.ParticlesRandomness:P0}";

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("randomness_dial");
}
