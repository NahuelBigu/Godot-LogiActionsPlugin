namespace Loupedeck.GodotMxBridge;

public static class ActionKeys
{
    // Runtime
    public const string RtPause = "rt_pause";
    public const string RtStop = "rt_stop";
    public const string RtRestart = "rt_restart";
    public const string RtResetTs = "rt_reset_ts";
    public const string RtFocusGame = "rt_focus_game";
    public const string RtFocus2D = "rt_focus_2d";
    public const string RtFocus3D = "rt_focus_3d";
    public const string RtFocusScript = "rt_focus_script";

    public const string FocusGame = "focus_game";
    public const string Focus2D = "focus_2d";
    public const string Focus3D = "focus_3d";
    public const string FocusScript = "focus_script";

    // Particles
    public const string PtEmitting = "pt_emitting";
    public const string PtOneShot = "pt_one_shot";
    public const string PtRestart = "pt_restart";
    public const string PtSpeedScale = "pt_speed_scale";
    public const string PtExplosiveness = "pt_explosiveness";
    public const string PtRandomness = "pt_randomness";
    public const string PtAmount = "amount";
    public const string PtAmountRatio = "amount_ratio";
    public const string PtLifetime = "lifetime";

    public const string DialAmount = "amount_dial";
    public const string DialLifetime = "lifetime_dial";
    public const string DialAmountRatio = "amount_ratio_dial";
    public const string DialSpeedScale = "speed_scale_dial";
    public const string DialExplosiveness = "explosiveness_dial";
    public const string DialRandomness = "randomness_dial";
    public const string DialTimeScale = "ts";

    // Script
    public const string ScNew = "sc_new";
    public const string ScSave = "sc_save";
    public const string ScRun = "sc_run";
    public const string ScCut = "sc_cut";
    public const string ScCopy = "sc_copy";
    public const string ScPaste = "sc_paste";
    public const string ScFind = "sc_find";
    public const string ScHelp = "sc_help";

    // Debug
    public const string DbgStepInto = "step_into";
    public const string DbgStepOver = "step_over";
    public const string DbgBreakpoint = "breakpoint";
    public const string DbgContinue = "continue";

    // Idle
    public const string IdlePlayCur = "idle_play_cur";
    public const string IdlePlayMain = "idle_play_main";
    public const string IdleFocusGame = "idle_focus_game";
    public const string IdleFocus2D = "idle_focus_2d";
    public const string IdleFocus3D = "idle_focus_3d";
    public const string IdleFocusScript = "idle_focus_script";

    // Transform
    public const string TfVis = "n3d_vis";
    public const string TfResetActive = "reset_active";

    // Animations
    public const string AnimNewAnim = "new_anim";
    public const string AnimNewTrack = "new_track";
    public const string AnimLoop = "loop";
    public const string AnimPlayPause = "play_pause";
    public const string AnimPlayReverse = "play_reverse";
    public const string AnimGotoStart = "goto_start";
    public const string AnimGotoEnd = "goto_end";
    public const string AnimStop = "stop";
    public const string AnimBackward = "backward";
    public const string AnimForward = "forward";
    public const string DialAnimTime = "anim_time";

    // Scene dock tools
    public const string SceneAddChild = "add_child";
    public const string SceneInstantiate = "instantiate_scene";
    public const string SceneAttachScript = "attach_script";

    // Transform components
    public const string TfPosX = "px";
    public const string TfPosY = "py";
    public const string TfPosZ = "pz";
    public const string TfRotX = "rx";
    public const string TfRotY = "ry";
    public const string TfRotZ = "rz";
    public const string TfScale = "scale";
    public const string TfFocusOrbit = "focus_orbit";

    // Editor snaps
    public const string SnapSmart = "snap_smart";
    public const string SnapGrid = "snap_grid";

    // TileMap tools
    public const string TmSelect = "select";
    public const string TmPaint = "paint";
    public const string TmLine = "line";
    public const string TmRect = "rect";
    public const string TmBucket = "bucket";
    public const string TmPicker = "picker";
    public const string TmEraser = "eraser";
    public const string TmRandomTile = "random_tile";
    public const string TmRotateLeft = "rotate_left";
    public const string TmRotateRight = "rotate_right";
    public const string TmFlipH = "flip_h";
    public const string TmFlipV = "flip_v";
    public const string TmPrevLayer = "prev_layer";
    public const string TmNextLayer = "next_layer";

    // TileMap dials
    public const string TmScroll = "tile_scroll";
    public const string TmRandomScatter = "random_scatter";
}
