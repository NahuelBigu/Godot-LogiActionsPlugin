using System.Text;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Godot editor Game/Juego tab time scale presets (same order as the dropdown).
/// Used by encoder dials to step discretely and to render neighbor + full-strip readouts.
/// </summary>
internal static class TimeScalePresetHelper
{
    /// <summary>Numeric values sent to <see cref="EventIds.TimeScale"/>.</summary>
    public static readonly double[] Presets =
        [0.0625, 0.125, 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0, 4.0, 8.0, 16.0];

    public static int FindClosestPresetIndex(double current)
    {
        var best = 0;
        var bestDist = Math.Abs(Presets[0] - current);
        for (var i = 1; i < Presets.Length; i++)
        {
            var dist = Math.Abs(Presets[i] - current);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    /// <summary>Label with × suffix (matches Godot dropdown wording).</summary>
    public static string FormatLabel(double v)
    {
        if (Math.Abs(v - 0.0625) < 0.001) return "1/16×";
        if (Math.Abs(v - 0.125)  < 0.001) return "1/8×";
        if (Math.Abs(v - 0.25)   < 0.001) return "1/4×";
        if (Math.Abs(v - 0.5)    < 0.001) return "1/2×";
        if (Math.Abs(v - 0.75)   < 0.001) return "3/4×";
        return $"{v:G}×";
    }

    /// <summary>Shorter label for dense preset strip (no ×).</summary>
    private static string FormatCompact(int index)
    {
        var v = Presets[index];
        if (Math.Abs(v - 0.0625) < 0.001) return "1/16";
        if (Math.Abs(v - 0.125)  < 0.001) return "1/8";
        if (Math.Abs(v - 0.25)   < 0.001) return "1/4";
        if (Math.Abs(v - 0.5)    < 0.001) return "1/2";
        if (Math.Abs(v - 0.75)   < 0.001) return "3/4";
        if (Math.Abs(v - 1.0)    < 0.001) return "1";
        return $"{v:G}";
    }

    /// <summary>
    /// Multi-line text for the hardware dial: previous / current / next step, then all presets with the active slot marked.
    /// </summary>
    public static string BuildEncoderReadout(double engineTimeScale)
    {
        var idx = FindClosestPresetIndex(engineTimeScale);
        var prev = idx > 0 ? FormatLabel(Presets[idx - 1]) : "—";
        var next = idx < Presets.Length - 1 ? FormatLabel(Presets[idx + 1]) : "—";
        var cur = FormatLabel(Presets[idx]);

        var sb = new StringBuilder();
        sb.Append(prev).Append(" ◄ ").Append(cur).Append(" ► ").Append(next);
        sb.Append('\n');
        for (var i = 0; i < Presets.Length; i++)
        {
            if (i > 0)
                sb.Append(" - ");
            if (i == idx)
                sb.Append('▸').Append(FormatCompact(i)).Append('◂');
            else
                sb.Append(FormatCompact(i));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Readout for a full <see cref="ContextSnapshot"/>: idle hint, preset strip from <see cref="ContextSnapshot.EngineTimeScale"/>,
    /// and an extra “Live …×” line when effective speed (Game tab, etc.) differs from the engine dial factor.
    /// </summary>
    public static string BuildEncoderReadoutForSnapshot(ContextSnapshot snap)
    {
        if (!snap.IsPlaying)
            return "Run project - dial inactive";

        var body = BuildEncoderReadout(snap.EngineTimeScale);
        if (Math.Abs(snap.RuntimeTimeScaleEffective - snap.EngineTimeScale) > 0.0001)
            return body + "\nLive " + FormatLabel(snap.RuntimeTimeScaleEffective);
        return body;
    }
}
