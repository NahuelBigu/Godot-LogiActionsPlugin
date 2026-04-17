$root = $PSScriptRoot
$srcIcons = Join-Path $root 'src\GodotMxBridgePlugin\Icons\svg'
$dstIcons = Join-Path $root 'src\GodotMxBridgePlugin\package\actionicons'
$dstSymbols = Join-Path $root 'src\GodotMxBridgePlugin\package\actionsymbols'
$ns = 'Loupedeck.GodotMxBridge'

function Save-Icon($fileName, $className) {
    Copy-Item "$srcIcons\$fileName.svg" "$dstIcons\$ns.$className.svg" -Force
    $content = Get-Content "$srcIcons\$fileName.svg" -Raw
    $content = $content -replace '#FFF', '#000'
    $content = $content -replace '#f25260', '#000'
    $content = $content -replace '#82d026', '#000'
    $content = $content -replace '#2b82d9', '#000'
    Set-Content "$dstSymbols\$ns.$className.svg" -Value $content -NoNewline
}

## TileMap (and any command with tm_* + tm_*_on): catalog/symbols get both off + lit assets.
## Runtime still loads lit tiles from embedded resources (SvgIcons.IsTileMapToolbarLitIcon).
function Save-IconLitPair([string] $baseName, [string] $className) {
    Save-Icon $baseName $className
    $onSrc = Join-Path $srcIcons "${baseName}_on.svg"
    if (-not (Test-Path $onSrc)) {
        Write-Warning "Save-IconLitPair: missing lit icon ${baseName}_on.svg → $className"
        return
    }
    Copy-Item $onSrc (Join-Path $dstIcons "$ns.${className}_on.svg") -Force
    $content = Get-Content $onSrc -Raw
    $content = $content -replace '#FFF', '#000'
    $content = $content -replace '#f25260', '#000'
    $content = $content -replace '#82d026', '#000'
    $content = $content -replace '#2b82d9', '#000'
    Set-Content (Join-Path $dstSymbols "$ns.${className}_on.svg") -Value $content -NoNewline
}

## Editor snap toggles: inactive = *off.svg → ClassName.svg; active → ClassName_on.svg (matches SvgIcons snap_* keys).
function Save-EditorSnapReactivePair([string] $inactiveBase, [string] $activeBase, [string] $className) {
    Save-Icon $inactiveBase $className
    $onSrc = Join-Path $srcIcons "${activeBase}.svg"
    if (-not (Test-Path $onSrc)) {
        Write-Warning "Save-EditorSnapReactivePair: missing active icon ${activeBase}.svg → ${className}_on"
        return
    }
    Copy-Item $onSrc (Join-Path $dstIcons "$ns.${className}_on.svg") -Force
    $content = Get-Content $onSrc -Raw
    $content = $content -replace '#FFF', '#000'
    $content = $content -replace '#f25260', '#000'
    $content = $content -replace '#82d026', '#000'
    $content = $content -replace '#2b82d9', '#000'
    Set-Content (Join-Path $dstSymbols "$ns.${className}_on.svg") -Value $content -NoNewline
}

# Node transform (2D / 3D)
Save-Icon 'eye_open' 'ToggleTransformVisibleCommand'
Save-Icon 'reset_n3d' 'ResetNodeTransformAdjustmentCommand'
# X (Red)
Save-Icon 'pos_x' 'NodeTransformPosXAdjustment'
Save-Icon 'rot_x' 'NodeTransformRotXAdjustment'
# Y (Green)
Save-Icon 'pos_y' 'NodeTransformPosYAdjustment'
Save-Icon 'rot_y' 'NodeTransformRotYAdjustment'
# Z (Blue)
Save-Icon 'pos_z' 'NodeTransformPosZAdjustment'
Save-Icon 'rot_z' 'NodeTransformRotZAdjustment'
# Scale
Save-Icon 'scale' 'NodeTransformScaleAdjustment'
Save-Icon 'eye_closed' 'ToggleTransformVisibleCommand_off'
Save-Icon 'reset_n3d_off' 'ResetNodeTransformAdjustmentCommand_off'
# Toggle commands: catalog/static tile only (runtime uses embedded particles_on/off & oneshot_on/off — see SvgIcons.Load).
Save-Icon 'particles_on' 'ToggleEmittingCommand'
Save-Icon 'oneshot_off' 'ToggleOneShotCommand'
Save-Icon 'restart' 'RestartParticlesCommand'
Save-Icon 'restart' 'RestartSceneCommand'
# Dial / adjustment icons — use dedicated art, not particles_on (old mistake made every dial look like "emitting").
Save-Icon 'particles_amount' 'ParticlesAmountAdjustment'
Save-Icon 'particles_lifetime' 'ParticlesLifetimeAdjustment'
Save-Icon 'particles_amount_ratio' 'ParticlesAmountRatioAdjustment'
Save-Icon 'particles_speed_scale' 'ParticlesSpeedScaleAdjustment'
Save-Icon 'particles_explosiveness' 'ParticlesExplosivenessAdjustment'
Save-Icon 'particles_randomness' 'ParticlesRandomnessAdjustment'

# Runtime / playback (same assets as SvgIcons GetReactiveIcon short names → actionicons)
Save-Icon 'pause' 'TogglePauseCommand'
Save-Icon 'stop' 'StopGameCommand'
Save-Icon 'play_scene' 'PlayCurrentCommand'
Save-Icon 'play_main' 'PlayMainSceneCommand'
Save-Icon 'game_tab' 'OpenGameTabCommand'

Save-Icon 'reset_ts' 'ResetTimeScaleCommand'
Save-Icon 'reset_ts' 'TimeScaleAdjustment'
Save-Icon 'godot' 'GodotContextFolder'
Save-Icon 'inspector_prop' 'InspectorPropAdjustment'
Save-Icon 'inspector_step' 'InspectorStepAdjustment'

Save-Icon 'editor_tab_2d' 'Open2DTabCommand'
Save-Icon 'editor_tab_3d' 'Open3DTabCommand'
Save-Icon 'editor_tab_script' 'OpenScriptTabCommand'
Save-Icon 'focus_orbit' 'FocusOrbitSelectionAdjustment'
Save-Icon 'spatial_views_3d' 'SpatialViewsDynamicFolder'
Save-Icon 'scene_tree_folder' 'SceneTreeToolbarDynamicFolder'
Save-Icon 'scene_tree_add_child' 'SceneTreeAddChildNodeCommand'
Save-Icon 'scene_tree_instantiate' 'SceneTreeInstantiateChildSceneCommand'
Save-Icon 'scene_tree_attach_script' 'SceneTreeAttachScriptCommand'

# 2D smart/grid snap + 3D use snap (Spatial shares smart-snap art with Canvas — same pixels on device and in list).
Save-EditorSnapReactivePair 'snap_smart_off' 'snap_smart' 'CanvasSmartSnapCommand'
Save-EditorSnapReactivePair 'snap_grid_off' 'snap_grid' 'CanvasGridSnapCommand'
Save-EditorSnapReactivePair 'snap_smart_off' 'snap_smart' 'SpatialUseSnapCommand'

Save-Icon 'file_new' 'ScriptNewCommand'
Save-Icon 'save' 'ScriptSaveCommand'
Save-Icon 'play' 'ScriptRunCommand'
Save-Icon 'cut' 'ScriptCutCommand'
Save-Icon 'copy' 'ScriptCopyCommand'
Save-Icon 'paste' 'ScriptPasteCommand'
Save-Icon 'find' 'ScriptFindCommand'
Save-Icon 'help' 'ScriptHelpCommand'

# TileMap — off + _on pairs (Godot toolbar mirror; see SvgIcons.TileMapToolbarKeysWithLitIcons)
Save-IconLitPair 'tm_select' 'TileMapSelectCommand'
Save-IconLitPair 'tm_paint' 'TileMapPaintCommand'
Save-IconLitPair 'tm_line' 'TileMapLineCommand'
Save-IconLitPair 'tm_rect' 'TileMapRectCommand'
Save-IconLitPair 'tm_bucket' 'TileMapBucketCommand'
Save-IconLitPair 'tm_picker' 'TileMapPickerCommand'
Save-IconLitPair 'tm_eraser' 'TileMapEraserCommand'
Save-IconLitPair 'tm_random_tile' 'TileMapRandomTileCommand'
# Single-state tiles (no *_on in Icons/svg)
Save-Icon 'tm_rotate_left' 'TileMapRotateLeftCommand'
Save-Icon 'tm_rotate_right' 'TileMapRotateRightCommand'
Save-Icon 'tm_flip_h' 'TileMapFlipHorizontalCommand'
Save-Icon 'tm_flip_v' 'TileMapFlipVerticalCommand'
Save-Icon 'tm_prev_layer' 'TileMapPrevLayerCommand'
Save-Icon 'tm_next_layer' 'TileMapNextLayerCommand'
Save-Icon 'tm_scroll' 'TileMapPaletteScrollAdjustment'
Save-Icon 'tm_random_tile' 'TileMapRandomScatterAdjustment'

# Animation editor
Save-Icon 'anim_play'         'AnimationPlayPauseCommand'
Save-Icon 'anim_pause'        'AnimationPlayPauseCommand_pause'
Save-Icon 'anim_stop'         'AnimationStopCommand'
Save-Icon 'anim_forward'      'AnimationForwardCommand'
Save-Icon 'anim_backward'     'AnimationBackwardCommand'
Save-Icon 'anim_to_start'     'AnimationGoToStartCommand'
Save-Icon 'anim_to_end'       'AnimationGoToEndCommand'
Save-Icon 'anim_play_reverse' 'AnimationPlayReverseCommand'
Save-Icon 'anim_insert_key'   'AnimationInsertKeyCommand'
Save-Icon 'anim_new'          'AnimationNewCommand'
Save-Icon 'anim_new_track'    'AnimationNewTrackCommand'
Save-Icon 'anim_loop_on'      'AnimationLoopToggleCommand'
Save-Icon 'anim_loop_off'     'AnimationLoopToggleCommand_off'
Save-Icon 'anim_loop_pingpong' 'AnimationLoopToggleCommand_pingpong'
Save-Icon 'anim_folder'       'AnimationDynamicFolder'
Save-Icon 'anim_time'         'AnimationTimeAdjustment'
Save-Icon 'anim_track_scroll' 'AnimationTracksDynamicFolder'
Save-Icon 'anim_clip_scroll' 'AnimationClipsDynamicFolder'

Write-Host "Copied missing icons"
