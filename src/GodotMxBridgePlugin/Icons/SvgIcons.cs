using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Loads SVG icons for action tiles. Prefers <c>actionicons/*.svg</c> next to the plugin
/// (same files as the action list / catalog) so <c>copy-icons.ps1</c> updates both list and
/// hardware without relying only on embedded resources from the last build. Falls back to
/// embedded resources for reactive pairs (<c>particles_on</c>/<c>particles_off</c>, <c>oneshot_on</c>/<c>oneshot_off</c>) and icons not shipped
/// in <c>actionicons</c>.
///
/// Embedded resource names: <c>Loupedeck.GodotMxBridge.Icons.svg.{filename}.svg</c>
/// </summary>
internal static class SvgIcons
{
    private static readonly Assembly Asm = typeof(SvgIcons).Assembly;
    private const string Prefix = "Loupedeck.GodotMxBridge.Icons.svg.";
    private const string Ns = "Loupedeck.GodotMxBridge";

    /// <summary>
    /// Maps <c>Icons/svg/{name}.svg</c> basename to packaged <c>actionicons/{Ns}.{ClassName}.svg</c>.
    /// Keep in sync with <c>copy-icons.ps1</c> <c>Save-Icon</c> lines.
    /// </summary>
    private static readonly FrozenDictionary<string, string> PackagedActionIconByShortName =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["eye_open"] = $"{Ns}.ToggleTransformVisibleCommand.svg",
            ["reset_n3d"] = $"{Ns}.ResetNodeTransformAdjustmentCommand.svg",
            ["pos_x"] = $"{Ns}.NodeTransformPosXAdjustment.svg",
            ["rot_x"] = $"{Ns}.NodeTransformRotXAdjustment.svg",
            ["pos_y"] = $"{Ns}.NodeTransformPosYAdjustment.svg",
            ["rot_y"] = $"{Ns}.NodeTransformRotYAdjustment.svg",
            ["focus_orbit"] = $"{Ns}.FocusOrbitSelectionAdjustment.svg",
            ["pos_z"] = $"{Ns}.NodeTransformPosZAdjustment.svg",
            ["rot_z"] = $"{Ns}.NodeTransformRotZAdjustment.svg",
            ["scale"] = $"{Ns}.NodeTransformScaleAdjustment.svg",
            // emitting / one_shot reactive states use embedded on+off SVGs only — avoids stale disk icons and ensures host repaints both variants.
            ["restart"] = $"{Ns}.RestartParticlesCommand.svg",
            ["particles_amount"] = $"{Ns}.ParticlesAmountAdjustment.svg",
            ["particles_lifetime"] = $"{Ns}.ParticlesLifetimeAdjustment.svg",
            ["particles_amount_ratio"] = $"{Ns}.ParticlesAmountRatioAdjustment.svg",
            ["particles_speed_scale"] = $"{Ns}.ParticlesSpeedScaleAdjustment.svg",
            ["particles_explosiveness"] = $"{Ns}.ParticlesExplosivenessAdjustment.svg",
            ["particles_randomness"] = $"{Ns}.ParticlesRandomnessAdjustment.svg",
            ["reset_ts"] = $"{Ns}.ResetTimeScaleCommand.svg",
            ["file_new"] = $"{Ns}.ScriptNewCommand.svg",
            ["save"] = $"{Ns}.ScriptSaveCommand.svg",
            ["play"] = $"{Ns}.ScriptRunCommand.svg",
            ["cut"] = $"{Ns}.ScriptCutCommand.svg",
            ["copy"] = $"{Ns}.ScriptCopyCommand.svg",
            ["paste"] = $"{Ns}.ScriptPasteCommand.svg",
            ["find"] = $"{Ns}.ScriptFindCommand.svg",
            ["help"] = $"{Ns}.ScriptHelpCommand.svg",
            ["editor_tab_2d"] = $"{Ns}.Open2DTabCommand.svg",
            ["editor_tab_3d"] = $"{Ns}.Open3DTabCommand.svg",
            ["editor_tab_script"] = $"{Ns}.OpenScriptTabCommand.svg",
            ["spatial_views_3d"] = $"{Ns}.SpatialViewsDynamicFolder.svg",
            ["scene_tree_add_child"] = $"{Ns}.SceneTreeAddChildNodeCommand.svg",
            ["scene_tree_instantiate"] = $"{Ns}.SceneTreeInstantiateChildSceneCommand.svg",
            ["scene_tree_attach_script"] = $"{Ns}.SceneTreeAttachScriptCommand.svg",
            ["scene_tree_folder"] = $"{Ns}.SceneTreeToolbarDynamicFolder.svg",
            ["tm_select"] = $"{Ns}.TileMapSelectCommand.svg",

            // Animation
            ["anim_play"]         = $"{Ns}.AnimationPlayPauseCommand.svg",
            ["anim_pause"]        = $"{Ns}.AnimationPlayPauseCommand_pause.svg",
            ["anim_stop"]         = $"{Ns}.AnimationStopCommand.svg",
            ["anim_forward"]      = $"{Ns}.AnimationForwardCommand.svg",
            ["anim_backward"]     = $"{Ns}.AnimationBackwardCommand.svg",
            ["anim_to_start"]     = $"{Ns}.AnimationGoToStartCommand.svg",
            ["anim_to_end"]       = $"{Ns}.AnimationGoToEndCommand.svg",
            ["anim_play_reverse"] = $"{Ns}.AnimationPlayReverseCommand.svg",
            ["anim_insert_key"]   = $"{Ns}.AnimationInsertKeyCommand.svg",
            ["anim_new"]          = $"{Ns}.AnimationNewCommand.svg",
            ["anim_new_track"]    = $"{Ns}.AnimationNewTrackCommand.svg",
            ["anim_loop_on"]      = $"{Ns}.AnimationLoopToggleCommand.svg",
            ["anim_loop_pingpong"] = $"{Ns}.AnimationLoopToggleCommand_pingpong.svg",
            ["anim_folder"]       = $"{Ns}.AnimationDynamicFolder.svg",
            ["anim_tracks_folder"] = $"{Ns}.AnimationTracksDynamicFolder.svg",
            ["anim_time"]          = $"{Ns}.AnimationTimeAdjustment.svg",
            ["anim_track_scroll"]  = $"{Ns}.AnimationTracksDynamicFolder.svg",
            ["anim_clip_scroll"]   = $"{Ns}.AnimationClipsDynamicFolder.svg",

            // Runtime / navigation (GetReactiveIcon short names → same actionicons as catalog)
            ["play"]       = $"{Ns}.ScriptRunCommand.svg",
            ["pause"]      = $"{Ns}.TogglePauseCommand.svg",
            ["stop"]       = $"{Ns}.StopGameCommand.svg",
            ["play_scene"] = $"{Ns}.PlayCurrentCommand.svg",
            ["play_main"]  = $"{Ns}.PlayMainSceneCommand.svg",
            ["game_tab"]   = $"{Ns}.OpenGameTabCommand.svg",

            ["eye_closed"] = $"{Ns}.ToggleTransformVisibleCommand_off.svg",
            ["reset_n3d_off"] = $"{Ns}.ResetNodeTransformAdjustmentCommand_off.svg",
            ["anim_loop_off"] = $"{Ns}.AnimationLoopToggleCommand_off.svg",

            // Editor snap toggles (active / inactive art packaged as Class + Class_on)
            ["snap_smart"]     = $"{Ns}.CanvasSmartSnapCommand_on.svg",
            ["snap_smart_off"] = $"{Ns}.CanvasSmartSnapCommand.svg",
            ["snap_grid"]      = $"{Ns}.CanvasGridSnapCommand_on.svg",
            ["snap_grid_off"]  = $"{Ns}.CanvasGridSnapCommand.svg",

            // Time scale dial uses its own catalog asset (not ResetTimeScaleCommand).
            ["ts"] = $"{Ns}.TimeScaleAdjustment.svg",

            // Generic fallback tile (particles w/o selection, unknown reactive keys, …)
            ["godot"] = $"{Ns}.GodotContextFolder.svg",

            // Inspector dials — catalog filenames must match runtime Load short names.
            ["inspector_prop"] = $"{Ns}.InspectorPropAdjustment.svg",
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // ═════════════════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns the appropriate icon for a command button based on action
    /// parameter and current context state. Icons toggle reactively.
    /// </summary>
    public static BitmapImage GetReactiveIcon(string actionParameter, ContextSnapshot snap) =>
        actionParameter switch
        {
            // ── Runtime ──────────────────────────────────────────────
            ActionKeys.RtPause      => snap.RuntimePaused ? Load("play") : Load("pause"),
            ActionKeys.RtStop       => Load("stop"),
            ActionKeys.RtRestart    => Load("restart"),
            ActionKeys.RtResetTs   => Load("reset_ts"),
            ActionKeys.RtFocusGame   => Load("game_tab"),
            ActionKeys.RtFocus2D     => Load("editor_tab_2d"),
            ActionKeys.RtFocus3D     => Load("editor_tab_3d"),
            ActionKeys.RtFocusScript => Load("editor_tab_script"),

            // ── Particles ────────────────────────────────────────────
            ActionKeys.PtEmitting   => snap.ParticlesEmitting ? Load("particles_on") : Load("particles_off"),
            ActionKeys.PtOneShot   => snap.ParticlesOneShot  ? Load("oneshot_on")   : Load("oneshot_off"),
            ActionKeys.PtRestart    => Load("restart"),
            ActionKeys.PtSpeedScale   => snap.HasParticles ? Load("particles_speed_scale") : Load("godot"),
            ActionKeys.PtExplosiveness => snap.HasParticles ? Load("particles_explosiveness") : Load("godot"),
            ActionKeys.PtRandomness    => snap.HasParticles ? Load("particles_randomness") : Load("godot"),

            // ── Transform (visibility) ───────────────────────────────
            ActionKeys.TfVis       => snap.HasTransformNode && snap.Visible ? Load("eye_open") : Load("eye_closed"),

            // ── Script ───────────────────────────────────────────────
            ActionKeys.ScNew   => Load("file_new"),
            ActionKeys.ScSave  => Load("save"),
            ActionKeys.ScRun   => Load("play"),
            ActionKeys.ScCut   => Load("cut"),
            ActionKeys.ScCopy  => Load("copy"),
            ActionKeys.ScPaste => Load("paste"),
            ActionKeys.ScFind  => Load("find"),
            ActionKeys.ScHelp  => Load("help"),

            // ── Idle ─────────────────────────────────────────────────
            ActionKeys.IdlePlayCur   => Load("play_scene"),
            ActionKeys.IdlePlayMain  => Load("play_main"),
            ActionKeys.IdleFocusGame   => Load("game_tab"),
            ActionKeys.IdleFocus2D     => Load("editor_tab_2d"),
            ActionKeys.IdleFocus3D     => Load("editor_tab_3d"),
            ActionKeys.IdleFocusScript => Load("editor_tab_script"),

            ActionKeys.FocusGame   => Load("game_tab"),
            ActionKeys.Focus2D     => Load("editor_tab_2d"),
            ActionKeys.Focus3D     => Load("editor_tab_3d"),
            ActionKeys.FocusScript => Load("editor_tab_script"),

            _ => Load("godot"),
        };

    /// <summary>Layer or mask slot: on vs off (collision_layer / collision_mask bits).</summary>
    public static BitmapImage GetCollisionBitIcon(bool bitEnabled) =>
        bitEnabled ? Load("collision_bit_on") : Load("collision_bit_off");

    /// <summary>Generic Godot logo tile for editor-shortcut commands without a dedicated asset.</summary>
    public static BitmapImage GetGodotBrandingIcon(PluginImageSize _) => Load("godot");

    /// <summary>Inspector Property dial — same asset as <c>InspectorPropAdjustment.svg</c> in <c>actionicons</c>.</summary>
    public static BitmapImage GetInspectorPropIcon(PluginImageSize _) => Load("inspector_prop");

    /// <summary>3D spatial views folder + ortho/persp commands (see <see cref="SpatialViewsDynamicFolder"/>).</summary>
    public static BitmapImage GetSpatialViewsFolderIcon(PluginImageSize _) => Load("spatial_views_3d");

    /// <summary>Scene dock toolbar: <paramref name="actionParameter"/> is <c>add_child</c>, <c>instantiate_scene</c>, or <c>attach_script</c>.</summary>
    public static BitmapImage GetSceneTreeToolbarIcon(string actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            ActionKeys.SceneAddChild         => Load("scene_tree_add_child"),
            ActionKeys.SceneInstantiate      => Load("scene_tree_instantiate"),
            ActionKeys.SceneAttachScript     => Load("scene_tree_attach_script"),
            _                                => Load("godot"),
        };

    /// <summary>Parent tile for <see cref="SceneTreeToolbarDynamicFolder"/>.</summary>
    public static BitmapImage GetSceneTreeFolderIcon(PluginImageSize _) => Load("scene_tree_folder");

    /// <summary>Parent tile for <see cref="AnimationTracksDynamicFolder"/> (uses embedded <c>anim_track_scroll.svg</c>).</summary>
    public static BitmapImage GetAnimationTracksFolderIcon(PluginImageSize _) => Load("anim_track_scroll");

    /// <summary>Parent tile for <see cref="AnimationClipsDynamicFolder"/> (uses embedded <c>anim_clip_scroll.svg</c>).</summary>
    public static BitmapImage GetAnimationClipsFolderIcon(PluginImageSize _) => Load("anim_clip_scroll");

    /// <summary>
    /// Invisible tile surface for label-only dynamic-folder commands. Logi Plugin Service must not receive
    /// <c>null</c> from <see cref="Loupedeck.PluginDynamicFolder.GetCommandImage"/> — that can crash native UI and disable the plugin.
    /// </summary>
    public static BitmapImage GetTransparentCommandTile(PluginImageSize _) => Load("transparent_tile");

    /// <summary>Smart snap + 3D use snap (magnet + blocks motif), on/off from Godot toolbar.</summary>
    public static BitmapImage GetEditorSnapSmartIcon(bool active) =>
        active ? Load("snap_smart") : Load("snap_smart_off");

    /// <summary>Grid snap (grid + magnet motif), on/off from Godot toolbar.</summary>
    public static BitmapImage GetEditorSnapGridIcon(bool active) =>
        active ? Load("snap_grid") : Load("snap_grid_off");

    /// <summary>
    /// Reset icon for the transform active-dial reset button (Node2D / Node3D).
    /// </summary>
    public static BitmapImage GetTransformResetIcon(bool active) =>
        active ? Load("reset_n3d") : Load("reset_n3d_off");

    /// <summary>
    /// Returns the icon for an encoder dial action.
    /// </summary>
    public static BitmapImage GetDialIcon(string actionParameter) =>
        actionParameter switch
        {
            ActionKeys.DialTimeScale                => Load("ts"),
            ActionKeys.DialAmount       => Load("particles_amount"),
            ActionKeys.DialLifetime     => Load("particles_lifetime"),
            ActionKeys.DialAmountRatio => Load("particles_amount_ratio"),
            ActionKeys.DialSpeedScale    => Load("particles_speed_scale"),
            ActionKeys.DialExplosiveness => Load("particles_explosiveness"),
            ActionKeys.DialRandomness   => Load("particles_randomness"),
            ActionKeys.TmScroll       => Load("tm_scroll"),
            ActionKeys.TmRandomScatter => Load("tm_random_tile"),
            ActionKeys.TfPosX                => Load("pos_x"),
            ActionKeys.TfPosY                => Load("pos_y"),
            ActionKeys.TfPosZ                => Load("pos_z"),
            ActionKeys.TfRotX                => Load("rot_x"),
            ActionKeys.TfRotY                => Load("rot_y"),
            ActionKeys.TfRotZ                => Load("rot_z"),
            ActionKeys.TfFocusOrbit       => Load("focus_orbit"),
            ActionKeys.TfScale             => Load("scale"),
            _ => Load("godot"),
        };

    /// <summary>Returns an animation icon by its short SVG name (e.g. "anim_play", "anim_pause", "anim_loop_on").</summary>
    public static BitmapImage GetAnimIcon(string name) => Load(name);

    /// <summary>Loop tile: Godot <c>Animation.LoopMode</c> 0 / 1 / 2.</summary>
    public static BitmapImage GetAnimationLoopIcon(int loopMode) =>
        loopMode switch
        {
            1 => Load("anim_loop_on"),
            2 => Load("anim_loop_pingpong"),
            _ => Load("anim_loop_off"),
        };

    /// <summary>TileMap tools (no live Godot state — use <see cref="GetTileMapCommandImage(string,ContextSnapshot,PluginImageSize)"/>).</summary>
    public static BitmapImage GetTileMapCommandImage(String actionParameter, PluginImageSize sz) =>
        GetTileMapCommandImage(actionParameter, new ContextSnapshot(), sz);

    /// <summary>TileMap command icon from snapshot (folder + standalone commands).</summary>
    public static BitmapImage GetTileMapCommandImage(String actionParameter, ContextSnapshot snap, PluginImageSize sz) =>
        GetTileMapToolSurfaceIcon(actionParameter, IsTileMapToolLit(actionParameter, snap), sz);

    /// <summary>
    /// Tool keys that ship embedded <c>tm_*</c> + <c>tm_*_on</c> pairs (toolbar mirror). Others use a single static tile.
    /// </summary>
    public static Boolean TileMapToolSupportsLitPair(String toolKey) =>
        TileMapToolbarKeysWithLitIcons.Contains(toolKey);

    /// <summary>
    /// Whether Godot reports this tool as active (toggle pressed and/or exclusive <see cref="ContextSnapshot.TileMapActiveTool"/>).
    /// </summary>
    public static Boolean IsTileMapToolLit(String toolKey, ContextSnapshot snap)
    {
        if (!snap.HasTileMap || !TileMapToolbarKeysWithLitIcons.Contains(toolKey))
            return false;
        if (snap.TileMapToolbar.TryGetValue(toolKey, out var tb) && tb)
            return true;
        var active = (snap.TileMapActiveTool ?? "").Trim();
        if (active.Length > 0
            && active.Equals(toolKey, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    /// <summary>
    /// Picks <c>tm_*</c> vs <c>tm_*_on</c> when <paramref name="lit"/> and the tool has an on pair; otherwise base asset.
    /// </summary>
    public static BitmapImage GetTileMapToolSurfaceIcon(String toolKey, Boolean lit, PluginImageSize _)
    {
        var baseName = TileMapToolResourceBaseName(toolKey);
        if (baseName == "godot")
            return Load("godot");
        if (lit && TileMapToolbarKeysWithLitIcons.Contains(toolKey))
            return Load($"{baseName}_on");
        return Load(baseName);
    }

    private static String TileMapToolResourceBaseName(String toolKey) =>
        toolKey switch
        {
            ActionKeys.TmSelect       => "tm_select",
            ActionKeys.TmPaint        => "tm_paint",
            ActionKeys.TmLine         => "tm_line",
            ActionKeys.TmRect         => "tm_rect",
            ActionKeys.TmBucket       => "tm_bucket",
            ActionKeys.TmPicker       => "tm_picker",
            ActionKeys.TmEraser       => "tm_eraser",
            ActionKeys.TmRandomTile  => "tm_random_tile",
            ActionKeys.TmRotateLeft  => "tm_rotate_left",
            ActionKeys.TmRotateRight => "tm_rotate_right",
            ActionKeys.TmFlipH       => "tm_flip_h",
            ActionKeys.TmFlipV       => "tm_flip_v",
            ActionKeys.TmPrevLayer   => "tm_prev_layer",
            ActionKeys.TmNextLayer   => "tm_next_layer",
            _                        => "godot",
        };

    /// <summary>Keys that mirror Godot TileMap toolbar toggles (shortcut + random tile).</summary>
    private static readonly HashSet<string> TileMapToolbarKeysWithLitIcons = new(StringComparer.Ordinal)
    {
        ActionKeys.TmSelect, ActionKeys.TmPaint, ActionKeys.TmLine, ActionKeys.TmRect, ActionKeys.TmBucket, ActionKeys.TmPicker, ActionKeys.TmEraser, ActionKeys.TmRandomTile,
    };

    // ═════════════════════════════════════════════════════════════════════════
    //  INTERNAL
    // ═════════════════════════════════════════════════════════════════════════

    private static BitmapImage Load(string name)
    {
        // Reactive emitting / one-shot pairs must never load from actionicons: packaged tiles are often a
        // single static SVG (e.g. ToggleEmittingCommand.svg copied from particles_on) and would shadow the off state.
        if (IsReactiveParticleStateIcon(name))
            return BitmapImage.FromResource(Asm, $"{Prefix}{name}.svg");

        // Lit TileMap tiles are embedded only (no actionicons shadow).
        if (IsTileMapToolbarLitIcon(name))
            return BitmapImage.FromResource(Asm, $"{Prefix}{name}.svg");

        if (TryLoadFromPackagedActionIcons(name, out var disk))
            return disk;
        return BitmapImage.FromResource(Asm, $"{Prefix}{name}.svg");
    }

    private static bool IsReactiveParticleStateIcon(string name) =>
        name is "particles_on" or "particles_off" or "oneshot_on" or "oneshot_off";

    private static bool IsTileMapToolbarLitIcon(string name) =>
        name.StartsWith("tm_", StringComparison.Ordinal) && name.EndsWith("_on", StringComparison.Ordinal);

    /// <summary>
    /// <c>…/bin/GodotMxBridgePlugin.dll</c> → package root is <c>…/</c> (sibling of inner <c>bin</c>).
    /// </summary>
    private static bool TryLoadFromPackagedActionIcons(string shortName, out BitmapImage image)
    {
        image = null!;
        if (!PackagedActionIconByShortName.TryGetValue(shortName, out var fileName))
            return false;

        try
        {
            var asmPath = Asm.Location;
            if (string.IsNullOrEmpty(asmPath))
                return false;
            var innerBin = Path.GetDirectoryName(asmPath);
            if (string.IsNullOrEmpty(innerBin))
                return false;
            var packageRoot = Path.GetFullPath(Path.Combine(innerBin, ".."));
            var path = Path.Combine(packageRoot, "actionicons", fileName);
            if (!File.Exists(path))
                return false;
            image = BitmapImage.FromArray(File.ReadAllBytes(path));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
