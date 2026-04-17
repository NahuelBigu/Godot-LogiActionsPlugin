namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Maps encoder <paramref name="diff"/> to particle amount delta: step ±1 on normal turns;
/// when the host coalesces many ticks (fast spin), applies the full delta.
/// </summary>
internal static class ParticleAmountDialHelper
{
    /// <summary>Below this absolute diff, treat as a single detent (±1).</summary>
    private const Int32 FastSpinAbsDiffThreshold = 3;

    public static Int32 DeltaFromEncoderDiff(Int32 diff)
    {
        if (diff == 0) return 0;
        return Math.Abs(diff) >= FastSpinAbsDiffThreshold ? diff : Int32.Sign(diff);
    }

    /// <summary>No upper cap; Godot/clamp in editor. Keeps amount usable (GPUParticles3D.amount ≥ 1).</summary>
    public static Int32 ClampMin1(Int32 value) => Math.Max(1, value);
}
