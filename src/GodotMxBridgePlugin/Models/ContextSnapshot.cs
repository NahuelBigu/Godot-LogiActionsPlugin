using System;
using System.Collections.Generic;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Immutable snapshot of the Godot editor state read from <c>context.json</c> (schema 2).
/// Covers all supported contexts: runtime, Node2D/Node3D transform, particles, tilemap, script and debug.
/// </summary>
public sealed class ContextSnapshot
{
    // ── Editor state ─────────────────────────────────────────────────────────
    public string  MainScreen      { get; init; } = "";
    public bool    IsPlaying       { get; init; }
    public bool    RuntimePaused   { get; init; }
    public double  EngineTimeScale { get; init; } = 1.0;
    /// <summary>Effective simulation speed while playing (engine × Game-tab debugger factor when exposed).</summary>
    public double  RuntimeTimeScaleEffective { get; init; } = 1.0;

    // ── Node2D / Node3D transform (unified numeric layout) ───────────────────
    public NodeTransformKind TransformKind { get; init; } = NodeTransformKind.None;
    /// <summary>Path of the node that owns the transform dial (2D or 3D).</summary>
    public string? TransformPath { get; init; }
    public bool    HasNode3d { get; init; }
    public bool    HasNode2d { get; init; }
    /// <summary>Non-null when <see cref="TransformKind"/> is Node3D (compat).</summary>
    public string? Node3dPath { get; init; }
    /// <summary>Non-null when <see cref="TransformKind"/> is Node2D.</summary>
    public string? Node2dPath { get; init; }
    public double[] Position     { get; init; } = [0, 0, 0];
    public double[] RotationDeg  { get; init; } = [0, 0, 0];
    public double   ScaleUniform { get; init; } = 1.0;
    public bool     Visible      { get; init; } = true;

    public bool HasTransformNode => TransformKind != NodeTransformKind.None;

    // ── Transform range hints (optional — sent by Godot when export_range is set) ──
    /// <summary>True when Godot provided per-axis min/max limits for the current node.</summary>
    public bool     HasRangeHints  { get; init; }
    public double[] PositionMin    { get; init; } = [-1e6, -1e6, -1e6];
    public double[] PositionMax    { get; init; } = [ 1e6,  1e6,  1e6];
    public double[] RotationMin    { get; init; } = [-360, -360, -360];
    public double[] RotationMax    { get; init; } = [ 360,  360,  360];
    public double   ScaleMin       { get; init; } = 0.001;
    public double   ScaleMax       { get; init; } = 100.0;

    // ── GPU/CPU particles (2D & 3D) ──────────────────────────────────────────
    public bool   HasParticles        { get; init; }
    /// <summary>Scene path of the selected particle node, when <see cref="HasParticles"/>.</summary>
    public string? ParticlesPath      { get; init; }
    /// <summary><c>true</c> for <c>GPUParticles2D</c>/<c>GPUParticles3D</c> (has <c>amount_ratio</c>). CPU nodes omit ratio in the UI.</summary>
    public bool   ParticlesSupportsAmountRatio { get; init; } = true;
    public bool   ParticlesEmitting   { get; init; }
    public int    ParticlesAmount     { get; init; } = 8;
    public double ParticlesAmountRatio { get; init; } = 1.0;
    public double ParticlesLifetime   { get; init; } = 1.0;
    public double ParticlesSpeedScale { get; init; } = 1.0;
    public double ParticlesExplosiveness { get; init; }
    public double ParticlesRandomness { get; init; }
    public bool   ParticlesOneShot    { get; init; }

    // ── TileMap ──────────────────────────────────────────────────────────────
    public bool HasTileMap         { get; init; }
    public int  TileMapCurrentLayer { get; init; }
    public int  TileMapLayerCount  { get; init; }
    /// <summary>Editor tool id from Godot snapshot: paint, line, rect, bucket, picker, eraser, select, unknown.</summary>
    public string TileMapActiveTool { get; init; } = "";

    /// <summary>TileMap «Scattering» spinbox value when the control exists (0 if unknown).</summary>
    public double TileMapRandomScatter { get; init; }

    /// <summary>Pressed TileMap toolbar toggles from Godot (paint, eraser, picker, random_tile, …).</summary>
    public Dictionary<string, bool> TileMapToolbar { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    // ── Script editor ────────────────────────────────────────────────────────
    public bool   IsScriptTab    { get; init; }
    public string ScriptFileName { get; init; } = "";

    // ── CollisionObject2D / CollisionObject3D (physics layers & mask) ───────
    /// <summary>True when a <c>CollisionObject2D</c> or <c>CollisionObject3D</c> is the bridge collision target.</summary>
    public bool HasCollisionObject { get; init; }
    public CollisionPhysicsDimension CollisionDimension { get; init; } = CollisionPhysicsDimension.None;
    public string? CollisionObjectPath { get; init; }
    /// <summary>Bitmask: layer 1 = bit 0 (value 1), matching Godot.</summary>
    public int CollisionLayerBits { get; init; }
    public int CollisionMaskBits { get; init; }
    /// <summary>32 entries; empty strings mean “show layer number” on the console.</summary>
    public string[] CollisionPhysicsLayerNames { get; init; } = new string[32];

    // ── CanvasItem (2D): visibility_layer + light_mask (2D render layer names) ─
    public bool HasCanvasItem { get; init; }
    public string? CanvasItemPath { get; init; }
    public int CanvasVisibilityLayerBits { get; init; }
    public int CanvasLightMaskBits { get; init; }
    public string[] RenderLayerNames2D { get; init; } = new string[RenderLayerSlotCount.Value];

    // ── VisualInstance3D: layers (3D render; Camera3D cull mask, lights, etc.) ─
    public bool HasVisualInstance3D { get; init; }
    public string? VisualInstance3DPath { get; init; }
    public int VisualInstanceLayersBits { get; init; }
    public string[] RenderLayerNames3D { get; init; } = new string[RenderLayerSlotCount.Value];

    // ── Editor snap toggles (toolbar state read in Godot; see MXEditorSnapStateHelper) ─
    public bool CanvasSmartSnapActive { get; init; }
    public bool CanvasGridSnapActive { get; init; }
    public bool SpatialSnapActive { get; init; }

    // ── Debug ────────────────────────────────────────────────────────────────
    public bool IsDebugging { get; init; }

    // ── Animation editor (AnimationPlayer + Animation panel) ─────────────────
    /// <summary>True when an AnimationPlayer is selected and the Animation panel is active.</summary>
    public bool   HasAnimation          { get; init; }
    public string AnimationName         { get; init; } = "";
    public double AnimationPosition     { get; init; }
    public double AnimationLength       { get; init; } = 1.0;
    public bool   AnimationPlaying      { get; init; }
    public bool   AnimationPaused       { get; init; }
    /// <summary>Legacy: true when <see cref="AnimationLoopMode"/> is not <c>LOOP_NONE</c> (linear or ping-pong).</summary>
    public bool   AnimationLoop         { get; init; }
    /// <summary>Godot <c>Animation.LoopMode</c>: 0 = none, 1 = linear, 2 = ping-pong.</summary>
    public int    AnimationLoopMode     { get; init; }
    public int    AnimationTrackCount   { get; init; }
    public int    AnimationSelectedTrack { get; init; } = -1;
    public string[] AnimationTrackNames { get; init; } = [];
    /// <summary>Animation names from <c>AnimationPlayer.get_animation_list()</c> (same order as Godot).</summary>
    public string[] AnimationClipNames { get; init; } = [];
}
