using System;
using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for the Godot AnimationPlayer / Animation panel.
///
/// Touch grid layout:
///   [Back]     [New Anim]  [New Track]  [Loop ↺]
///   [|◄ Start] [◄ Step]   [Play/Pause] [Step ►]  [End ▶|]
///   [■ Stop]   [◄ Reverse]
///
/// Encoders:
///   Dial – focused Inspector float/int first, else scrub animation time (touch = insert key).
///   Track selection: <c>Anim - Tracks</c>. Animation list: <c>Anim - Animation list</c>.
/// </summary>
public sealed class AnimationDynamicFolder : BridgeDynamicFolder
{

    public AnimationDynamicFolder()
    {
        DisplayName = "Animation";
        GroupName   = "Animation";
    }

    protected override void OnContextChanged()
    {
        foreach (var p in new[] { ActionKeys.AnimPlayPause, ActionKeys.AnimPlayReverse, ActionKeys.AnimLoop, ActionKeys.AnimGotoStart, ActionKeys.AnimGotoEnd, ActionKeys.AnimStop })
            CommandImageChanged(p);
        AdjustmentValueChanged(ActionKeys.DialAnimTime);
    }

    // ── Touch buttons ─────────────────────────────────────────────────────────

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            CreateCommandName(ActionKeys.AnimNewAnim),
            CreateCommandName(ActionKeys.AnimNewTrack),
            CreateCommandName(ActionKeys.AnimLoop),
            CreateCommandName(ActionKeys.AnimGotoStart),
            CreateCommandName(ActionKeys.AnimBackward),
            CreateCommandName(ActionKeys.AnimPlayPause),
            CreateCommandName(ActionKeys.AnimForward),
            CreateCommandName(ActionKeys.AnimGotoEnd),
            CreateCommandName(ActionKeys.AnimStop),
            CreateCommandName(ActionKeys.AnimPlayReverse),
        };

    // ── Encoder dials ─────────────────────────────────────────────────────────

    public override IEnumerable<string> GetEncoderRotateActionNames(DeviceType _) =>
        new[] { CreateAdjustmentName(ActionKeys.DialAnimTime) };

    // ── Commands ──────────────────────────────────────────────────────────────

    public override void RunCommand(string actionParameter)
    {
        Bridge.TryReadSnapshot(out var snap);
        switch (actionParameter)
        {
            case ActionKeys.AnimNewAnim:
                Bridge.SendTrigger(EventIds.AnimNewAnim);
                break;
            case ActionKeys.AnimNewTrack:
                if (snap.HasAnimation) Bridge.SendTrigger(EventIds.AnimNewTrack);
                break;
            case ActionKeys.AnimLoop:
                if (snap.HasAnimation) Bridge.SendTrigger(EventIds.AnimToggleLoop);
                CommandImageChanged(ActionKeys.AnimLoop);
                break;
            case ActionKeys.AnimPlayPause:
                if (snap.HasAnimation)
                    Bridge.SendTrigger(snap.AnimationPlaying ? EventIds.AnimPause : EventIds.AnimPlay);
                CommandImageChanged(ActionKeys.AnimPlayPause);
                break;
            case ActionKeys.AnimPlayReverse:
                Bridge.SendTrigger(EventIds.AnimPlayReverse);
                break;
            case ActionKeys.AnimGotoStart:
                Bridge.SendTrigger(EventIds.AnimGoToStart);
                break;
            case ActionKeys.AnimGotoEnd:
                Bridge.SendTrigger(EventIds.AnimGoToEnd);
                break;
            case ActionKeys.AnimStop:
                if (snap.HasAnimation) Bridge.SendTrigger(EventIds.AnimStop);
                CommandImageChanged(ActionKeys.AnimPlayPause);
                break;
            case ActionKeys.AnimBackward:
                Bridge.SendTrigger(EventIds.AnimStepBackward);
                break;
            case ActionKeys.AnimForward:
                Bridge.SendTrigger(EventIds.AnimStepForward);
                break;
            // Encoder touch: insert key (anim_time) — no-op for anim_track
            case ActionKeys.DialAnimTime:
                if (snap.HasAnimation) Bridge.SendTrigger(EventIds.AnimInsertKey);
                break;
        }
    }

    // ── Adjustments ───────────────────────────────────────────────────────────

    public override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0) return;
        switch (actionParameter)
        {
            case ActionKeys.DialAnimTime:
                if (Bridge.TryReadFocusedProp(out _))
                {
                    Bridge.SendInspectorPropStepDelta(NodeTransformHelper.VelocityTicks(diff));
                }
                else if (Bridge.TryReadSnapshot(out var snap) && snap.HasAnimation)
                {
                    Bridge.SendFloat(EventIds.AnimScrub, diff);
                }
                break;
        }
        AdjustmentValueChanged(actionParameter);
    }

    public override string? GetAdjustmentValue(string actionParameter)
    {
        if (actionParameter != ActionKeys.DialAnimTime)
            return null;
        if (Bridge.TryReadFocusedProp(out var fp))
            return $"{fp.Label}: {fp.Value:G}";
        if (Bridge.TryReadSnapshot(out var snap) && snap.HasAnimation)
            return $"{snap.AnimationPosition:F3}s";
        return null;
    }

    // ── Images ────────────────────────────────────────────────────────────────

    public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_folder");

    public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        if (actionParameter == PluginDynamicFolder.NavigateUpActionName)
            return base.GetCommandImage(actionParameter, imageSize);

        Bridge.TryReadSnapshot(out var snap);
        return actionParameter switch
        {
            ActionKeys.AnimNewAnim     => SvgIcons.GetAnimIcon("anim_new"),
            ActionKeys.AnimNewTrack    => SvgIcons.GetAnimIcon("anim_new_track"),
            ActionKeys.AnimLoop        => SvgIcons.GetAnimationLoopIcon(snap.AnimationLoopMode),
            ActionKeys.AnimPlayPause   => SvgIcons.GetAnimIcon(snap.AnimationPlaying ? "anim_pause" : "anim_play"),
            ActionKeys.AnimPlayReverse => SvgIcons.GetAnimIcon("anim_play_reverse"),
            ActionKeys.AnimGotoStart   => SvgIcons.GetAnimIcon("anim_to_start"),
            ActionKeys.AnimGotoEnd     => SvgIcons.GetAnimIcon("anim_to_end"),
            ActionKeys.AnimStop        => SvgIcons.GetAnimIcon("anim_stop"),
            ActionKeys.AnimBackward    => SvgIcons.GetAnimIcon("anim_backward"),
            ActionKeys.AnimForward     => SvgIcons.GetAnimIcon("anim_forward"),
            _                          => SvgIcons.GetAnimIcon("anim_folder"),
        };
    }

    public override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.DialAnimTime => SvgIcons.GetAnimIcon("anim_time"),
            _                       => base.GetAdjustmentImage(actionParameter, imageSize),
        };

    // ── Display names ─────────────────────────────────────────────────────────

    public override string? GetCommandDisplayName(string actionParameter, PluginImageSize _)
    {
        if (actionParameter == PluginDynamicFolder.NavigateUpActionName) return "Back";
        Bridge.TryReadSnapshot(out var snap);
        return actionParameter switch
        {
            ActionKeys.AnimNewAnim     => "New Anim",
            ActionKeys.AnimNewTrack    => "New Track",
            ActionKeys.AnimLoop        => snap.AnimationLoopMode switch
            {
                1 => "Loop ON",
                2 => "Ping-Pong",
                _ => "Loop OFF",
            },
            ActionKeys.AnimPlayPause   => snap.AnimationPlaying ? "Pause" : snap.AnimationPaused ? "Resume" : "Play",
            ActionKeys.AnimPlayReverse => "Reverse",
            ActionKeys.AnimGotoStart   => "|◄ Start",
            ActionKeys.AnimGotoEnd     => "End ▶|",
            ActionKeys.AnimStop        => "Stop",
            ActionKeys.AnimBackward    => "◄ Step",
            ActionKeys.AnimForward     => "Step ►",
            _                          => actionParameter,
        };
    }

    public override string? GetAdjustmentDisplayName(string actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            ActionKeys.DialAnimTime => "Scrub time",
            _                       => null,
        };

    public override string GetButtonDisplayName(PluginImageSize _)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation)
            return "Animation";
        var name = snap.AnimationName.Length > 12 ? snap.AnimationName[..12] + "…" : snap.AnimationName;
        var state = snap.AnimationPlaying ? "▶" : snap.AnimationPaused ? "⏸" : "■";
            return $"{state} {name}";
    }
}
