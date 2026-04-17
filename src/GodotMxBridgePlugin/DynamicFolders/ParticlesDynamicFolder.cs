namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for GPU/CPU particles (2D & 3D): toggle emitting/one_shot,
/// amount/lifetime/speed/explosive/random; amount ratio only for GPU particle nodes.
/// Subscribes to ContextChanged to refresh values live.
/// </summary>
public class ParticlesDynamicFolder : BridgeDynamicFolder
{
    public ParticlesDynamicFolder()
    {
        DisplayName = "Particles";
        GroupName   = "Particles";
    }

    private static bool ShowAmountRatioSlot(ContextSnapshot snap) =>
        !snap.HasParticles || snap.ParticlesSupportsAmountRatio;

    protected override void OnContextChanged()
    {
        foreach (var p in new[]
                 {
                     ActionKeys.PtEmitting, ActionKeys.PtAmount, ActionKeys.PtAmountRatio, ActionKeys.PtOneShot, ActionKeys.PtLifetime, ActionKeys.PtRestart, ActionKeys.PtSpeedScale,
                     ActionKeys.PtExplosiveness, ActionKeys.PtRandomness,
                 })
            CommandImageChanged(p);
        AdjustmentValueChanged(ActionKeys.DialAmount);
        AdjustmentValueChanged(ActionKeys.DialLifetime);
        AdjustmentValueChanged(ActionKeys.DialAmountRatio);
        AdjustmentValueChanged(ActionKeys.DialSpeedScale);
        AdjustmentValueChanged(ActionKeys.DialExplosiveness);
        AdjustmentValueChanged(ActionKeys.DialRandomness);
    }

    // ── Touch grid ──────────────────────────────────────────────────────────

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
    {
        Bridge.TryReadSnapshot(out var snap);
        yield return CreateCommandName(ActionKeys.PtEmitting);
        yield return CreateCommandName(ActionKeys.PtAmount);
        if (ShowAmountRatioSlot(snap))
            yield return CreateCommandName(ActionKeys.PtAmountRatio);
        yield return CreateCommandName(ActionKeys.PtOneShot);
        yield return CreateCommandName(ActionKeys.PtLifetime);
        yield return CreateCommandName(ActionKeys.PtSpeedScale);
        yield return CreateCommandName(ActionKeys.PtExplosiveness);
        yield return CreateCommandName(ActionKeys.PtRandomness);
        yield return CreateCommandName(ActionKeys.PtRestart);
    }

    public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType _)
    {
        Bridge.TryReadSnapshot(out var snap);
        yield return CreateAdjustmentName(ActionKeys.DialAmount);
        yield return CreateAdjustmentName(ActionKeys.DialLifetime);
        if (ShowAmountRatioSlot(snap))
            yield return CreateAdjustmentName(ActionKeys.DialAmountRatio);
        yield return CreateAdjustmentName(ActionKeys.DialSpeedScale);
        yield return CreateAdjustmentName(ActionKeys.DialExplosiveness);
        yield return CreateAdjustmentName(ActionKeys.DialRandomness);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    public override void RunCommand(String actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles) return;
        switch (actionParameter)
        {
            case ActionKeys.PtEmitting:
                Bridge.SendBool(EventIds.PtEmitting, !snap.ParticlesEmitting);
                break;
            case ActionKeys.PtOneShot:
                Bridge.SendBool(EventIds.PtOneShot, !snap.ParticlesOneShot);
                break;
            case ActionKeys.PtRestart:
                Bridge.SendTrigger(EventIds.PtRestart);
                break;
        }
    }

    // ── Adjustments ──────────────────────────────────────────────────────────

    public override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles) return;
        switch (actionParameter)
        {
            case ActionKeys.DialAmount:
                {
                    var deltaAmt = ParticleAmountDialHelper.DeltaFromEncoderDiff(diff);
                    if (deltaAmt != 0)
                    {
                        var nextAmt = ParticleAmountDialHelper.ClampMin1(snap.ParticlesAmount + deltaAmt);
                        Bridge.SendInt(EventIds.PtAmount, nextAmt);
                    }
                }
                break;
            case ActionKeys.DialLifetime:
                {
                    if (diff != 0)
                        Bridge.SendFloat(EventIds.PtLifetime,
                            ParticleLifetimeDialHelper.ApplyEncoderDiff(snap.ParticlesLifetime, diff));
                }
                break;
            case ActionKeys.DialAmountRatio:
                if (snap.ParticlesSupportsAmountRatio && diff != 0)
                {
                    var newAr = ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesAmountRatio, diff);
                    Bridge.SendFloat(EventIds.PtAmountRatio, newAr);
                }
                break;
            case ActionKeys.DialSpeedScale:
                if (diff != 0)
                    Bridge.SendFloat(EventIds.PtSpeedScale,
                        ParticleSpeedScaleDialHelper.ApplyEncoderDiff(snap.ParticlesSpeedScale, diff));
                break;
            case ActionKeys.DialExplosiveness:
                if (diff != 0)
                    Bridge.SendFloat(EventIds.PtExplosiveness,
                        ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesExplosiveness, diff));
                break;
            case ActionKeys.DialRandomness:
                if (diff != 0)
                    Bridge.SendFloat(EventIds.PtRandomness,
                        ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesRandomness, diff));
                break;
        }
        AdjustmentValueChanged(actionParameter);
    }

    public override String? GetAdjustmentValue(String actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasParticles) return null;
        return actionParameter switch
        {
            ActionKeys.DialAmount       => $"{snap.ParticlesAmount}",
            ActionKeys.DialLifetime     => $"{snap.ParticlesLifetime:F2}s",
            ActionKeys.DialAmountRatio => snap.ParticlesSupportsAmountRatio ? $"{snap.ParticlesAmountRatio:P0}" : null,
            ActionKeys.DialSpeedScale    => $"{snap.ParticlesSpeedScale:F2}×",
            ActionKeys.DialExplosiveness => $"{snap.ParticlesExplosiveness:P0}",
            ActionKeys.DialRandomness    => $"{snap.ParticlesRandomness:P0}",
            _                   => null,
        };
    }

    public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(actionParameter);

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return actionParameter switch
        {
            ActionKeys.PtEmitting       => SvgIcons.GetReactiveIcon(ActionKeys.PtEmitting, snap),
            ActionKeys.PtOneShot       => SvgIcons.GetReactiveIcon(ActionKeys.PtOneShot, snap),
            ActionKeys.PtRestart        => SvgIcons.GetReactiveIcon(ActionKeys.PtRestart, snap),
            ActionKeys.PtAmount         => SvgIcons.GetDialIcon(ActionKeys.DialAmount),
            ActionKeys.PtAmountRatio   => SvgIcons.GetDialIcon(ActionKeys.DialAmountRatio),
            ActionKeys.PtLifetime       => SvgIcons.GetDialIcon(ActionKeys.DialLifetime),
            ActionKeys.PtSpeedScale    => SvgIcons.GetReactiveIcon(ActionKeys.PtSpeedScale, snap),
            ActionKeys.PtExplosiveness  => SvgIcons.GetReactiveIcon(ActionKeys.PtExplosiveness, snap),
            ActionKeys.PtRandomness     => SvgIcons.GetReactiveIcon(ActionKeys.PtRandomness, snap),
            _                => base.GetCommandImage(actionParameter, imageSize),
        };
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _)
    {
        Bridge.TryReadSnapshot(out var snap);
        return actionParameter switch
        {
            ActionKeys.PtEmitting     => snap?.ParticlesEmitting == true ? "⏹ Emitting" : "▶ Emitting",
            ActionKeys.PtAmount       => $"Amount: {snap?.ParticlesAmount ?? 0}",
            ActionKeys.PtAmountRatio => $"Ratio: {snap?.ParticlesAmountRatio ?? 1.0:P0}",
            ActionKeys.PtOneShot     => snap?.ParticlesOneShot == true ? "✓ One Shot" : "One Shot",
            ActionKeys.PtLifetime       => $"Life: {snap?.ParticlesLifetime ?? 1.0:F2}s",
            ActionKeys.PtSpeedScale    => $"Spd: {snap?.ParticlesSpeedScale ?? 1.0:F2}×",
            ActionKeys.PtExplosiveness  => $"Expl: {snap?.ParticlesExplosiveness ?? 0.0:P0}",
            ActionKeys.PtRandomness     => $"Rand: {snap?.ParticlesRandomness ?? 0.0:P0}",
            ActionKeys.PtRestart        => "🔄 Restart",
            _                => actionParameter,
        };
    }

    public override String? GetAdjustmentDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            ActionKeys.DialAmount         => "Amount",
            ActionKeys.DialLifetime       => "Lifetime",
            ActionKeys.DialAmountRatio   => "Am. Ratio",
            ActionKeys.DialSpeedScale    => "Speed",
            ActionKeys.DialExplosiveness => "Explosive",
            ActionKeys.DialRandomness    => "Random",
            _                     => null,
        };
}
