namespace Loupedeck.GodotMxBridge;

/// <summary>
/// All bridge event IDs shared between the Logitech plugin and the Godot addon.
/// Mirror of <c>MXEventIds</c> on the GDScript side.
/// </summary>
public static class EventIds
{
    // ── Runtime ──────────────────────────────────────────────────────────────
    public const string Pause          = "mx.runtime.pause";
    public const string TimeScale      = "mx.runtime.time_scale";
    public const string Stop           = "mx.runtime.stop";
    public const string Restart        = "mx.runtime.restart";
    public const string PlayCurrent    = "mx.runtime.play_current";
    public const string PlayMain       = "mx.runtime.play_main";
    public const string FocusGame      = "mx.runtime.focus_game_tab";
    public const string Focus2DTab     = "mx.runtime.focus_2d_tab";
    public const string Focus3DTab     = "mx.runtime.focus_3d_tab";
    public const string FocusScriptTab = "mx.runtime.focus_script_tab";
    public const string ResetTimeScale = "mx.runtime.reset_time_scale";

    // ── Node transform (Node2D / Node3D) ─────────────────────────────────────
    public const string TfVisible = "mx.transform.visible";
    public const string TfScale   = "mx.transform.scale_uniform";
    public const string TfPosX    = "mx.transform.position_x";
    public const string TfPosY    = "mx.transform.position_y";
    public const string TfPosZ    = "mx.transform.position_z";
    public const string TfRotX    = "mx.transform.rotation_deg_x";
    public const string TfRotY    = "mx.transform.rotation_deg_y";
    public const string TfRotZ    = "mx.transform.rotation_deg_z";

    // ── Collision layers / mask (CollisionObject2D / CollisionObject3D) ───────
    public const string CollisionToggleLayer = "mx.collision.toggle_layer";
    public const string CollisionToggleMask  = "mx.collision.toggle_mask";

    // ── Render / visibility (20 layers — unified 2D visibility + 3D visual layers; light mask separate) ───
    public const string RenderLayersToggle    = "mx.render_layers.toggle";
    public const string CanvasLightMaskToggle = "mx.canvas.light_mask.toggle";

    // ── Inspector ────────────────────────────────────────────────────────────
    public const string InspectorProp = "mx.inspector.focused_prop";

    // ── Particles ────────────────────────────────────────────────────────────
    public const string PtEmitting   = "mx.particles.emitting";
    public const string PtAmount     = "mx.particles.amount";
    public const string PtAmountRatio = "mx.particles.amount_ratio";
    public const string PtOneShot    = "mx.particles.one_shot";
    public const string PtLifetime   = "mx.particles.lifetime";
    public const string PtSpeedScale = "mx.particles.speed_scale";
    public const string PtExplosiveness = "mx.particles.explosiveness";
    public const string PtRandomness = "mx.particles.randomness";
    public const string PtRestart    = "mx.particles.restart";

    // ── TileMap ──────────────────────────────────────────────────────────────
    public const string TmSelect    = "mx.tilemap.tool_select";
    public const string TmPaint     = "mx.tilemap.tool_paint";
    public const string TmLine      = "mx.tilemap.tool_line";
    public const string TmRect      = "mx.tilemap.tool_rect";
    public const string TmBucket    = "mx.tilemap.tool_bucket";
    public const string TmPicker    = "mx.tilemap.tool_picker";
    public const string TmEraser    = "mx.tilemap.tool_eraser";
    public const string TmToggleRandomTile = "mx.tilemap.toggle_random_tile";
    public const string TmRotate    = "mx.tilemap.rotate";
    public const string TmRotateLeft  = "mx.tilemap.rotate_left";
    public const string TmRotateRight = "mx.tilemap.rotate_right";
    public const string TmFlipH     = "mx.tilemap.flip_h";
    public const string TmFlipV     = "mx.tilemap.flip_v";
    public const string TmPrevLayer = "mx.tilemap.prev_layer";
    public const string TmNextLayer = "mx.tilemap.next_layer";
    public const string TmTileScroll = "mx.tilemap.tile_scroll";
    public const string TmRandomScatter = "mx.tilemap.random_scatter";

    // ── Editor shortcuts (Godot EditorSettings path in <c>path</c> on the event) ─
    public const string EditorShortcut = "mx.editor.shortcut";
    /// <summary>Encoder steps → horizontal orbit of the 3D editor camera (middle-drag simulation).</summary>
    public const string View3dOrbitYaw = "mx.view3d.orbit_yaw";

    // ── Script ───────────────────────────────────────────────────────────────
    public const string ScNew        = "mx.script.new";
    public const string ScSave       = "mx.script.save";
    public const string ScRun        = "mx.script.run";
    public const string ScCut        = "mx.script.cut";
    public const string ScCopy       = "mx.script.copy";
    public const string ScPaste      = "mx.script.paste";
    public const string ScFind       = "mx.script.find";
    public const string ScSearchHelp = "mx.script.search_help";

    // ── Debug ────────────────────────────────────────────────────────────────
    public const string DbgStepInto   = "mx.debug.step_into";
    public const string DbgStepOver   = "mx.debug.step_over";
    public const string DbgBreakpoint = "mx.debug.breakpoint";
    public const string DbgContinue   = "mx.debug.continue";

    // ── Animation editor ─────────────────────────────────────────────────────
    public const string AnimPlay        = "mx.anim.play";
    public const string AnimPause       = "mx.anim.pause";
    public const string AnimStop        = "mx.anim.stop";
    public const string AnimGoToStart   = "mx.anim.goto_start";
    public const string AnimGoToEnd     = "mx.anim.goto_end";
    public const string AnimPlayReverse = "mx.anim.play_reverse";
    public const string AnimInsertKey   = "mx.anim.insert_key";
    public const string AnimNewAnim     = "mx.anim.new_animation";
    public const string AnimNewTrack    = "mx.anim.new_track";
    public const string AnimToggleLoop  = "mx.anim.toggle_loop";
    /// <summary>Float delta: steps in the animation timeline (encoder ticks → Godot scrub).</summary>
    public const string AnimScrub       = "mx.anim.scrub";
    /// <summary>Float delta: scroll the track list to change the selected track (MX console / legacy).</summary>
    public const string AnimTrackScroll  = "mx.anim.track_scroll";
    /// <summary>Absolute track index (0-based) for Loupedeck track picker folder.</summary>
    public const string AnimTrackSelect   = "mx.anim.track_select";
    /// <summary>Absolute animation index (0-based) for Loupedeck animation list folder.</summary>
    public const string AnimClipSelect    = "mx.anim.clip_select";
    /// <summary>Advance one frame forward (uses Animation.step; works even when stopped).</summary>
    public const string AnimStepForward  = "mx.anim.step_forward";
    /// <summary>Go back one frame (uses Animation.step; works even when stopped).</summary>
    public const string AnimStepBackward = "mx.anim.step_backward";
}
