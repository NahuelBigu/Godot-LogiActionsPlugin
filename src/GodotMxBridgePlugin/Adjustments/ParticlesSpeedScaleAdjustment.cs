namespace Loupedeck.GodotMxBridge;

public class ParticlesSpeedScaleAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHas;
    private Double? _lastSpeed;

    public ParticlesSpeedScaleAdjustment()
        : base("Particles Speed Scale", "Dial: speed scale 0–64× (0 = pause)", "Particles", hasReset: false)
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
        var v   = has ? Math.Round(snapshot.ParticlesSpeedScale, 4) : (Double?)null;
        if (_lastHas == has && _lastSpeed == v) return;
        _lastHas   = has;
        _lastSpeed = v;
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (!Bridge.TryReadSnapshot(out var s) || !s.HasParticles || diff == 0) return;
        var next = ParticleSpeedScaleDialHelper.ApplyEncoderDiff(s.ParticlesSpeedScale, diff);
        Bridge.SendFloat(EventIds.PtSpeedScale, next);
        AdjustmentValueChanged();
    }

    protected override string GetAdjustmentValue(string actionParameter) =>
        !Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles
            ? "—"
            : $"{snap.ParticlesSpeedScale:F2}×";

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("speed_scale_dial");
}
