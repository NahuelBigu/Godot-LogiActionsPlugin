namespace Loupedeck.GodotMxBridge;

/// <summary>
/// <see cref="ContextSnapshot.ParticlesSpeedScale"/> (0 = paused). Coarse steps; small |diff| = one tick.
/// </summary>
internal static class ParticleSpeedScaleDialHelper
{
    public const Double Step = 0.05;

    private const Int32 FastSpinAbsDiffThreshold = 3;
    private const Int32 MaxBurstSteps = 10;

    public static Double Snap(Double value)
    {
        value = Math.Clamp(value, 0.0, 64.0);
        return Math.Round(value / Step, MidpointRounding.AwayFromZero) * Step;
    }

    public static Double ApplyEncoderDiff(Double current, Int32 diff)
    {
        if (diff == 0) return Snap(current);
        var ad = Math.Abs(diff);
        var steps = ad < FastSpinAbsDiffThreshold ? 1 : Math.Min(ad, MaxBurstSteps);
        var delta = Math.Sign(diff) * steps * Step;
        return Snap(current + delta);
    }
}
