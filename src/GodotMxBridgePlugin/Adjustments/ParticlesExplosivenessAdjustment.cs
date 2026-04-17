namespace Loupedeck.GodotMxBridge;

public class ParticlesExplosivenessAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ParticlesExplosivenessAdjustment()
        : base("Particles Explosiveness", "Dial: 0 = continuous, 1 = burst (1% steps)", "Particles", hasReset: false)
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
        var next = ParticleAmountRatioHelper.ApplyEncoderDiff(s.ParticlesExplosiveness, diff);
        Bridge.SendFloat(EventIds.PtExplosiveness, next);
    }

    protected override string GetAdjustmentValue(string actionParameter) =>
        !Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles
            ? "—"
            : $"{snap.ParticlesExplosiveness:P0}";

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("explosiveness_dial");
}
