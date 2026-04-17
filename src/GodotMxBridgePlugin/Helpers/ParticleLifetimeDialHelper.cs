namespace Loupedeck.GodotMxBridge;

/// <summary>
/// <see cref="ContextSnapshot.ParticlesLifetime"/> — same encoder rules as
/// <see cref="ParticleSpeedScaleDialHelper"/>: <c>|diff| &lt; 3</c> = one step, else capped burst.
/// </summary>
internal static class ParticleLifetimeDialHelper
{
    public const Double MinSeconds = 0.01;
    public const Double MaxSeconds = 600.0;
    public const Double Step = 0.01;

    private const Int32 FastSpinAbsDiffThreshold = 3;
    private const Int32 MaxBurstSteps = 10;

    public static Double Snap(Double value)
    {
        value = Math.Clamp(value, MinSeconds, MaxSeconds);
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
