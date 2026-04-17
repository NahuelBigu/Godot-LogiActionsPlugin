using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Loupedeck.GodotMxBridge;

/// <summary>Parses the shared bridge JSON payload (schema 2) from Godot — file or HTTP body.</summary>
internal static class BridgePayloadParser
{
    public static bool TryGetContextAndOptionsRaw(string json, out string rawCtx, out string rawOpts)
    {
        rawCtx = "";
        rawOpts = "[]";
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("context", out var ctx))
                return false;
            rawCtx = ctx.GetRawText();
            rawOpts = root.TryGetProperty("options", out var opts) ? opts.GetRawText() : "[]";
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryParseContext(string json, out ContextSnapshot snapshot)
    {
        snapshot = new ContextSnapshot();
        try
        {
            using var doc = JsonDocument.Parse(json);
            return TryParseContext(doc.RootElement, out snapshot);
        }
        catch
        {
            return false;
        }
    }

    public static bool TryParseContext(JsonElement root, out ContextSnapshot snapshot)
    {
        snapshot = new ContextSnapshot();
        if (!root.TryGetProperty("context", out var ctx))
            return false;

        var main    = ctx.GetStringOrDefault("main_screen");
        var playing = ctx.GetBoolOrDefault("is_playing");
        var rp      = ctx.GetBoolOrDefault("runtime_paused");
        var ets     = ctx.GetDoubleOrDefault("engine_time_scale", 1.0);
        var rtsEff  = ctx.GetDoubleOrDefault("runtime_time_scale_effective", ets);

        var canvasSmartSnap = ctx.GetBoolOrDefault("canvas_smart_snap_active");
        var canvasGridSnap  = ctx.GetBoolOrDefault("canvas_grid_snap_active");
        var spatialSnap     = ctx.GetBoolOrDefault("spatial_snap_active");

        var hn = ctx.GetBoolOrDefault("has_node3d");
        var h2 = ctx.GetBoolOrDefault("has_node2d");
        string? path3 = null;
        string? path2 = null;
        double[] pos = [0, 0, 0];
        double[] rot = [0, 0, 0];
        var scale = 1.0;
        var vis = true;

        var hasHints = false;
        var posMin = new double[] { -1e6, -1e6, -1e6 };
        var posMax = new double[] {  1e6,  1e6,  1e6 };
        var rotMin = new double[] { -360, -360, -360 };
        var rotMax = new double[] {  360,  360,  360 };
        var scaleMin = 0.001;
        var scaleMax = 100.0;

        if (ctx.TryGetProperty("node3d_snapshot", out var n3) && n3.ValueKind == JsonValueKind.Object)
        {
            if (!hn) hn = n3.HasNonEmptyString("path");
            path3 = n3.GetStringOrDefault("path");
            pos   = n3.GetDoubleArray("position", 3);
            rot   = n3.GetDoubleArray("rotation_deg", 3);
            scale = n3.GetDoubleOrDefault("scale_uniform", 1.0);
            vis   = n3.GetBoolOrDefault("visible", true);

            if (n3.TryGetProperty("range_hints", out var rh) && rh.ValueKind == JsonValueKind.Object)
            {
                hasHints = true;
                posMin   = rh.GetDoubleArray("position_min", 3);
                posMax   = rh.GetDoubleArray("position_max", 3);
                rotMin   = rh.GetDoubleArray("rotation_min", 3);
                rotMax   = rh.GetDoubleArray("rotation_max", 3);
                scaleMin = rh.GetDoubleOrDefault("scale_min", 0.001);
                scaleMax = rh.GetDoubleOrDefault("scale_max", 100.0);
            }
        }

        var hasNode2dObj = ctx.TryGetProperty("node2d_snapshot", out var n2El) && n2El.ValueKind == JsonValueKind.Object;
        if (hasNode2dObj)
        {
            if (!h2) h2 = n2El.HasNonEmptyString("path");
            path2 = n2El.GetStringOrDefault("path");
        }

        var transformKind = NodeTransformKind.None;
        string? transformPath = null;
        string? node3PathOut = null;
        string? node2PathOut = null;

        if (!String.IsNullOrEmpty(path3))
        {
            transformKind = NodeTransformKind.Node3D;
            transformPath = path3;
            node3PathOut  = path3;
        }
        else if (!String.IsNullOrEmpty(path2) && hasNode2dObj)
        {
            transformKind = NodeTransformKind.Node2D;
            transformPath = path2;
            node2PathOut  = path2;
            pos   = n2El.GetDoubleArray("position", 3);
            rot   = n2El.GetDoubleArray("rotation_deg", 3);
            scale = n2El.GetDoubleOrDefault("scale_uniform", 1.0);
            vis   = n2El.GetBoolOrDefault("visible", true);
            if (n2El.TryGetProperty("range_hints", out var rh2) && rh2.ValueKind == JsonValueKind.Object)
            {
                hasHints = true;
                posMin   = rh2.GetDoubleArray("position_min", 3);
                posMax   = rh2.GetDoubleArray("position_max", 3);
                rotMin   = rh2.GetDoubleArray("rotation_min", 3);
                rotMax   = rh2.GetDoubleArray("rotation_max", 3);
                scaleMin = rh2.GetDoubleOrDefault("scale_min", 0.001);
                scaleMax = rh2.GetDoubleOrDefault("scale_max", 100.0);
            }
        }

        var hasPt = ctx.GetBoolOrDefault("has_particles");
        var ptEmitting = false;
        var ptPath = "";
        var ptSupportsAmountRatio = true;
        var ptAmount = 8;
        var ptAmountRatio = 1.0;
        var ptLifetime = 1.0;
        var ptSpeedScale = 1.0;
        var ptExplosiveness = 0.0;
        var ptRandomness = 0.0;
        var ptOneShot = false;

        if (ctx.TryGetProperty("particles_snapshot", out var pt) && pt.ValueKind == JsonValueKind.Object)
        {
            // Legacy behavior (pre–multi-particle fields): any non-null particles object ⇒ particle selection.
            // Keeps HasParticles in sync if an older bridge omits or delays top-level has_particles.
            hasPt         = true;
            ptPath        = pt.GetStringOrDefault("path");
            ptEmitting    = pt.GetBoolOrDefault("emitting");
            ptAmount      = pt.GetIntOrDefault("amount", 8);
            ptAmountRatio = pt.GetDoubleOrDefault("amount_ratio", 1.0);
            ptSupportsAmountRatio = pt.GetBoolOrDefault("supports_amount_ratio", true);
            ptLifetime    = pt.GetDoubleOrDefault("lifetime", 1.0);
            ptSpeedScale  = pt.GetDoubleOrDefault("speed_scale", 1.0);
            ptExplosiveness = pt.GetDoubleOrDefault("explosiveness", 0.0);
            ptRandomness  = pt.GetDoubleOrDefault("randomness", 0.0);
            ptOneShot     = pt.GetBoolOrDefault("one_shot");
        }

        var hasTm = ctx.GetBoolOrDefault("has_tilemap");
        var tmLayer = 0;
        var tmLayerCount = 0;
        var tmActiveTool = "";
        var tmRandomScatter = 0.0;
        var tmToolbar = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (ctx.TryGetProperty("tilemap_snapshot", out var tm) && tm.ValueKind == JsonValueKind.Object)
        {
            hasTm = true;
            tmLayer      = tm.GetIntOrDefault("current_layer", 0);
            tmLayerCount = tm.GetIntOrDefault("layer_count", 0);
            tmActiveTool = tm.GetStringOrDefault("active_tool").Trim();
            tmRandomScatter = tm.GetDoubleOrDefault("random_scatter", 0.0);
            if (tm.TryGetProperty("toolbar", out var tbar) && tbar.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in tbar.EnumerateObject())
                {
                    if (TryParseGodotToolbarBool(prop.Value, out var b))
                        tmToolbar[prop.Name] = b;
                }
            }
        }

        var isScriptTab = ctx.GetBoolOrDefault("is_script_tab") || IsScriptScreen(main);
        var scriptFile  = ctx.GetStringOrDefault("script_file");

        var hasAnim = false;
        var animName = "";
        var animPos = 0.0;
        var animLen = 1.0;
        var animPlaying = false;
        var animPaused = false;
        var animLoop = false;
        var animLoopMode = 0;
        var animTrackCount = 0;
        var animSelectedTrack = -1;
        var animTrackNames = Array.Empty<string>();
        var animClipNames = Array.Empty<string>();

        if (ctx.TryGetProperty("animation_snapshot", out var animSnap) && animSnap.ValueKind == JsonValueKind.Object)
        {
            var aName = animSnap.GetStringOrDefault("animation_name");
            if (!String.IsNullOrEmpty(aName))
            {
                hasAnim           = true;
                animName          = aName;
                animPos           = animSnap.GetDoubleOrDefault("position");
                animLen           = animSnap.GetDoubleOrDefault("length", 1.0);
                animPlaying       = animSnap.GetBoolOrDefault("is_playing");
                animPaused        = animSnap.GetBoolOrDefault("is_paused");
                if (animSnap.TryGetProperty("loop_mode", out var lmEl) && lmEl.ValueKind == JsonValueKind.Number)
                {
                    animLoopMode = lmEl.GetInt32();
                    if (animLoopMode < 0 || animLoopMode > 2)
                        animLoopMode = animSnap.GetBoolOrDefault("loop") ? 1 : 0;
                }
                else
                    animLoopMode = animSnap.GetBoolOrDefault("loop") ? 1 : 0;
                animLoop            = animLoopMode != 0;
                animTrackCount    = animSnap.GetIntOrDefault("track_count");
                animSelectedTrack = animSnap.GetIntOrDefault("selected_track", -1);
                animTrackNames    = animSnap.GetStringArray("track_names", Math.Max(animTrackCount, 0));
                if (animSnap.TryGetProperty("animation_names", out var clipArr) && clipArr.ValueKind == JsonValueKind.Array)
                {
                    var tmp = new List<string>();
                    foreach (var item in clipArr.EnumerateArray())
                    {
                        if (tmp.Count >= 128) break;
                        tmp.Add(item.ValueKind == JsonValueKind.String ? (item.GetString() ?? "") : "");
                    }
                    animClipNames = tmp.ToArray();
                }
            }
        }

        var hasCol = false;
        var colDim = CollisionPhysicsDimension.None;
        var colPath = (string?)null;
        var colLayer = 0;
        var colMask = 0;
        var colNames = new string[32];
        if (ctx.TryGetProperty("collision_snapshot", out var colEl) && colEl.ValueKind == JsonValueKind.Object)
        {
            var p = colEl.GetStringOrDefault("path");
            if (!String.IsNullOrEmpty(p))
            {
                hasCol = true;
                colPath = p;
                var dim = colEl.GetStringOrDefault("dimension");
                colDim = dim.Equals("3d", StringComparison.OrdinalIgnoreCase)
                    ? CollisionPhysicsDimension.Physics3D
                    : dim.Equals("2d", StringComparison.OrdinalIgnoreCase)
                        ? CollisionPhysicsDimension.Physics2D
                        : CollisionPhysicsDimension.None;
                colLayer = colEl.GetPhysicsLayerMaskInt32("collision_layer");
                colMask  = colEl.GetPhysicsLayerMaskInt32("collision_mask");
                colNames = colEl.GetStringArray("layer_names", 32);
            }
        }

        var hasCanvas = false;
        var canvasPath = (string?)null;
        var canvasVis = 0;
        var canvasLight = 0;
        var names2D = new string[RenderLayerSlotCount.Value];
        if (ctx.TryGetProperty("canvas_item_snapshot", out var cvs) && cvs.ValueKind == JsonValueKind.Object)
        {
            var cp = cvs.GetStringOrDefault("path");
            if (!String.IsNullOrEmpty(cp))
            {
                hasCanvas = true;
                canvasPath = cp;
                canvasVis   = cvs.GetPhysicsLayerMaskInt32("visibility_layer");
                canvasLight = cvs.GetPhysicsLayerMaskInt32("light_mask");
                names2D     = cvs.GetStringArray("layer_names", RenderLayerSlotCount.Value);
            }
        }

        var hasVi = false;
        var viPath = (string?)null;
        var viLayers = 0;
        var names3D = new string[RenderLayerSlotCount.Value];
        if (ctx.TryGetProperty("visual_instance_snapshot", out var viSnap) && viSnap.ValueKind == JsonValueKind.Object)
        {
            var vp = viSnap.GetStringOrDefault("path");
            if (!String.IsNullOrEmpty(vp))
            {
                hasVi = true;
                viPath = vp;
                viLayers = viSnap.GetPhysicsLayerMaskInt32("layers");
                names3D  = viSnap.GetStringArray("layer_names", RenderLayerSlotCount.Value);
            }
        }

        snapshot = new ContextSnapshot
        {
            MainScreen       = main,
            IsPlaying        = playing,
            RuntimePaused    = rp,
            EngineTimeScale           = ets,
            RuntimeTimeScaleEffective = rtsEff,
            TransformKind    = transformKind,
            TransformPath    = transformPath,
            HasNode3d        = hn,
            HasNode2d        = h2,
            Node3dPath       = node3PathOut,
            Node2dPath       = node2PathOut,
            Position         = pos,
            RotationDeg      = rot,
            ScaleUniform     = scale,
            Visible          = vis,
            HasRangeHints    = hasHints,
            PositionMin      = posMin,
            PositionMax      = posMax,
            RotationMin      = rotMin,
            RotationMax      = rotMax,
            ScaleMin         = scaleMin,
            ScaleMax         = scaleMax,
            HasParticles         = hasPt,
            ParticlesPath        = String.IsNullOrEmpty(ptPath) ? null : ptPath,
            ParticlesSupportsAmountRatio = ptSupportsAmountRatio,
            ParticlesEmitting    = ptEmitting,
            ParticlesAmount      = ptAmount,
            ParticlesAmountRatio = ptAmountRatio,
            ParticlesLifetime    = ptLifetime,
            ParticlesSpeedScale  = ptSpeedScale,
            ParticlesExplosiveness = ptExplosiveness,
            ParticlesRandomness  = ptRandomness,
            ParticlesOneShot     = ptOneShot,
            HasTileMap          = hasTm,
            TileMapCurrentLayer = tmLayer,
            TileMapLayerCount   = tmLayerCount,
            TileMapActiveTool   = tmActiveTool ?? "",
            TileMapRandomScatter = tmRandomScatter,
            TileMapToolbar       = tmToolbar,
            IsScriptTab    = isScriptTab,
            ScriptFileName = scriptFile,
            HasCollisionObject      = hasCol,
            CollisionDimension      = colDim,
            CollisionObjectPath     = colPath,
            CollisionLayerBits      = colLayer,
            CollisionMaskBits       = colMask,
            CollisionPhysicsLayerNames = colNames,
            HasCanvasItem            = hasCanvas,
            CanvasItemPath           = canvasPath,
            CanvasVisibilityLayerBits = canvasVis,
            CanvasLightMaskBits      = canvasLight,
            RenderLayerNames2D       = names2D,
            HasVisualInstance3D      = hasVi,
            VisualInstance3DPath     = viPath,
            VisualInstanceLayersBits = viLayers,
            RenderLayerNames3D       = names3D,
            CanvasSmartSnapActive  = canvasSmartSnap,
            CanvasGridSnapActive   = canvasGridSnap,
            SpatialSnapActive      = spatialSnap,
            IsDebugging            = playing,
            HasAnimation           = hasAnim,
            AnimationName          = animName,
            AnimationPosition      = animPos,
            AnimationLength        = animLen,
            AnimationPlaying       = animPlaying,
            AnimationPaused        = animPaused,
            AnimationLoop          = animLoop,
            AnimationLoopMode      = animLoopMode,
            AnimationTrackCount    = animTrackCount,
            AnimationSelectedTrack = animSelectedTrack,
            AnimationTrackNames    = animTrackNames,
            AnimationClipNames     = animClipNames,
        };
        return true;
    }

    public static bool TryParseFocusedProp(string json, out FocusedPropSnapshot prop)
    {
        prop = new FocusedPropSnapshot();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("options", out var opts))
                return false;

            foreach (var opt in opts.EnumerateArray())
            {
                var id = opt.GetStringOrDefault("id");
                if (id != EventIds.InspectorProp) continue;

                prop = new FocusedPropSnapshot
                {
                    Label = opt.GetStringOrDefault("label"),
                    Min   = opt.GetDoubleOrDefault("min", 0.0),
                    Max   = opt.GetDoubleOrDefault("max", 1.0),
                    Step  = opt.GetDoubleOrDefault("step", 0.001),
                    Value = opt.GetDoubleOrDefault("value", 0.0),
                };
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Godot <c>JSON.stringify</c> uses JSON booleans; tolerate 0/1 and string forms from older payloads.</summary>
    private static bool TryParseGodotToolbarBool(JsonElement v, out bool b)
    {
        b = false;
        switch (v.ValueKind)
        {
            case JsonValueKind.True:
                b = true;
                return true;
            case JsonValueKind.False:
                return true;
            case JsonValueKind.Number:
                b = v.GetInt32() != 0;
                return true;
            case JsonValueKind.String:
                var s = v.GetString();
                if (String.IsNullOrEmpty(s))
                    return false;
                if (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1")
                {
                    b = true;
                    return true;
                }
                if (s.Equals("false", StringComparison.OrdinalIgnoreCase) || s == "0")
                    return true;
                return false;
            default:
                return false;
        }
    }

    private static bool IsScriptScreen(string mainScreen) =>
        mainScreen.Equals("Script", StringComparison.OrdinalIgnoreCase)
        || mainScreen.Equals("Código", StringComparison.OrdinalIgnoreCase);
}
