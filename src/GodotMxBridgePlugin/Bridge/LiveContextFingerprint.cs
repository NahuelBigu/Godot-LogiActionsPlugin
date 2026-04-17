using System;
using System.Linq;

namespace Loupedeck.GodotMxBridge;

/// <summary>Stable fingerprint of editor state that drives on-device labels (transforms, tabs, particles).</summary>
internal static class LiveContextFingerprint
{
    /// <summary>
    /// Hash of the values users see on MX dials. Used so <see cref="HttpBridgeTransport"/> poll coalescing
    /// detects logical changes even if raw JSON text matches.
    /// </summary>
    public static Int32 Compute(ContextSnapshot s)
    {
        var hc = new HashCode();
        hc.Add(s.MainScreen);
        hc.Add(s.IsPlaying);
        hc.Add(s.RuntimePaused);
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.EngineTimeScale, 6)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.RuntimeTimeScaleEffective, 6)));
        hc.Add((Int32)s.TransformKind);
        hc.Add(s.TransformPath ?? "");
        foreach (var x in s.Position) hc.Add(BitConverter.DoubleToInt64Bits(Round(x, 5)));
        foreach (var x in s.RotationDeg) hc.Add(BitConverter.DoubleToInt64Bits(Round(x, 3)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ScaleUniform, 5)));
        hc.Add(s.Visible);
        hc.Add(s.HasRangeHints);
        hc.Add(s.HasParticles);
        hc.Add(s.ParticlesPath ?? "");
        hc.Add(s.ParticlesSupportsAmountRatio);
        hc.Add(s.ParticlesEmitting);
        hc.Add(s.ParticlesAmount);
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ParticlesAmountRatio, 5)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ParticlesLifetime, 4)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ParticlesSpeedScale, 4)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ParticlesExplosiveness, 5)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.ParticlesRandomness, 5)));
        hc.Add(s.ParticlesOneShot);
        hc.Add(s.IsScriptTab);
        hc.Add(s.ScriptFileName ?? "");
        hc.Add(s.HasTileMap);
        hc.Add(s.TileMapCurrentLayer);
        hc.Add(s.TileMapLayerCount);
        hc.Add(s.TileMapActiveTool ?? "");
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.TileMapRandomScatter, 5)));
        foreach (var kv in s.TileMapToolbar.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hc.Add(kv.Key);
            hc.Add(kv.Value);
        }

        hc.Add(s.HasCollisionObject);
        hc.Add((Int32)s.CollisionDimension);
        hc.Add(s.CollisionObjectPath ?? "");
        hc.Add(s.CollisionLayerBits);
        hc.Add(s.CollisionMaskBits);
        foreach (var n in s.CollisionPhysicsLayerNames)
            hc.Add(n ?? "");
        hc.Add(s.HasCanvasItem);
        hc.Add(s.CanvasItemPath ?? "");
        hc.Add(s.CanvasVisibilityLayerBits);
        hc.Add(s.CanvasLightMaskBits);
        foreach (var n in s.RenderLayerNames2D)
            hc.Add(n ?? "");
        hc.Add(s.HasVisualInstance3D);
        hc.Add(s.VisualInstance3DPath ?? "");
        hc.Add(s.VisualInstanceLayersBits);
        foreach (var n in s.RenderLayerNames3D)
            hc.Add(n ?? "");
        hc.Add(s.CanvasSmartSnapActive);
        hc.Add(s.CanvasGridSnapActive);
        hc.Add(s.SpatialSnapActive);

        // Animation editor
        hc.Add(s.HasAnimation);
        hc.Add(s.AnimationName ?? "");
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.AnimationPosition, 3)));
        hc.Add(BitConverter.DoubleToInt64Bits(Round(s.AnimationLength, 4)));
        hc.Add(s.AnimationPlaying);
        hc.Add(s.AnimationPaused);
        hc.Add(s.AnimationLoop);
        hc.Add(s.AnimationLoopMode);
        hc.Add(s.AnimationTrackCount);
        hc.Add(s.AnimationSelectedTrack);
        foreach (var n in s.AnimationTrackNames)
            hc.Add(n ?? "");
        foreach (var n in s.AnimationClipNames)
            hc.Add(n ?? "");

        return hc.ToHashCode();
    }

    private static Double Round(Double v, Int32 digits) => Math.Round(v, digits, MidpointRounding.AwayFromZero);
}
