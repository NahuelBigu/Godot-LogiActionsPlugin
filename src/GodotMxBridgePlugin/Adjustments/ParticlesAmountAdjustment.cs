namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dial to change <see cref="ContextSnapshot.ParticlesAmount"/> (±1 per detent; larger jumps when spinning fast).
/// Subscribes to <see cref="GodotContextBroadcastService"/> for live readouts.
/// </summary>
public class ParticlesAmountAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ParticlesAmountAdjustment()
        : base("Particles Amount", "Dial to increase or decrease particle amount", "Particles", hasReset: false)
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
        if (!Bridge.TryReadSnapshot(out var s) || !s.HasParticles) return;
        var delta = ParticleAmountDialHelper.DeltaFromEncoderDiff(diff);
        if (delta == 0) return;
        var next = ParticleAmountDialHelper.ClampMin1(s.ParticlesAmount + delta);
        Bridge.SendInt(EventIds.PtAmount, next);
    }

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles) return "—";
        return $"{snap.ParticlesAmount}";
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("amount_dial");
}
