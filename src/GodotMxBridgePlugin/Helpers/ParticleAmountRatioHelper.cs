namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Amount ratio in [0, 1]; each detent is <see cref="Step"/> (1%). Encoder often sends <c>diff</c> 2 for one
/// click — same idea as <see cref="ParticleAmountDialHelper"/>: small |diff| counts as one step, not diff×Step.
/// </summary>
internal static class ParticleAmountRatioHelper
{
    /// <summary>One percentage point in Godot’s 0–1 ratio.</summary>
    public const Double Step = 0.01;

    /// <summary>Below this, treat as a single 1% step (avoids 2% jumps when host sends diff=2).</summary>
    private const Int32 FastSpinAbsDiffThreshold = 3;

    /// <summary>Cap % points per callback when spinning fast (keeps coalesced ticks from jumping too far).</summary>
    private const Int32 MaxBurstPercentSteps = 4;

    public static Double ClampAndSnap(Double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return Math.Round(value / Step, MidpointRounding.AwayFromZero) * Step;
    }

    public static Double ApplyEncoderDiff(Double currentRatio, Int32 diff)
    {
        if (diff == 0) return ClampAndSnap(currentRatio);
        var ad = Math.Abs(diff);
        var percentSteps = ad < FastSpinAbsDiffThreshold ? 1 : Math.Min(ad, MaxBurstPercentSteps);
        var delta = Math.Sign(diff) * percentSteps * Step;
        return ClampAndSnap(currentRatio + delta);
    }
}
