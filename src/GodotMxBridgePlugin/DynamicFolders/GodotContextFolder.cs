namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Master auto-switching dynamic folder. Detects the current Godot context
/// (Runtime / Particles / Transform / Script) and dynamically changes its
/// touch buttons and encoder dials to match.
///
/// Context priority (highest wins):
///   1. Game running  → Runtime controls + time scale dial
///   2. Particle node selected (GPU/CPU 2D/3D) → Particles controls + dials (ratio only on GPU)
///   3. Node2D/Node3D selected → Transform controls + axis dials
///   4. Script tab → Script editor shortcuts
///   5. Default → Play/Focus buttons (idle)
/// </summary>
public class GodotContextFolder : BridgeDynamicFolder
{
    private enum ContextMode { Idle, Runtime, Particles, Transform, Script }

    public GodotContextFolder()
    {
        DisplayName = "Auto";
        GroupName   = "Auto";
    }

    public override Boolean Load()
    {
        NodeTransformAdjustmentTracker.AxisReset += OnTransformAxisReset;
        return base.Load(); // subscribes ContextChanged
    }

    public override Boolean Unload()
    {
        NodeTransformAdjustmentTracker.AxisReset -= OnTransformAxisReset;
        return base.Unload(); // unsubscribes ContextChanged
    }

    private void OnTransformAxisReset(String key) => AdjustmentValueChanged(key);

    /// <summary>Refreshes only the UI elements relevant to the current context mode.</summary>
    protected override void OnContextChanged()
    {
        if (Bridge.TryReadSnapshot(out var snap))
            NodeTransformAdjustmentTracker.ReconcilePendingWithSnapshot(snap);

        // NotifyLayoutIfChanged() is called by base after this method returns.
        var mode = Detect(out _);
        switch (mode)
        {
            case ContextMode.Runtime:
                CommandImageChanged(ActionKeys.RtPause);
                AdjustmentValueChanged(ActionKeys.DialTimeScale);
                break;

            case ContextMode.Particles:
                CommandImageChanged(ActionKeys.PtEmitting);
                CommandImageChanged(ActionKeys.PtOneShot);
                AdjustmentValueChanged(ActionKeys.DialAmount);
                AdjustmentValueChanged(ActionKeys.DialLifetime);
                AdjustmentValueChanged(ActionKeys.DialAmountRatio);
                AdjustmentValueChanged(ActionKeys.DialSpeedScale);
                AdjustmentValueChanged(ActionKeys.DialExplosiveness);
                AdjustmentValueChanged(ActionKeys.DialRandomness);
                break;

            case ContextMode.Transform:
                CommandImageChanged(ActionKeys.TfVis);
                foreach (var k in NodeTransformHelper.AllKeys)
                    AdjustmentValueChanged(k);
                break;
        }
    }

    private ContextMode Detect(out ContextSnapshot snap)
    {
        snap = new ContextSnapshot();
        if (!Bridge.TryReadSnapshot(out snap))
            return ContextMode.Idle;

        if (snap.IsPlaying)         return ContextMode.Runtime;
        if (snap.HasParticles)      return ContextMode.Particles;
        if (snap.HasTransformNode) return ContextMode.Transform;
        if (snap.IsScriptTab)       return ContextMode.Script;
        return ContextMode.Idle;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  TOUCH BUTTONS
    // ═══════════════════════════════════════════════════════════════════════════

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
    {
        var mode = Detect(out var s);
        switch (mode)
        {
            case ContextMode.Runtime:
                yield return CreateCommandName(ActionKeys.RtPause);
                yield return CreateCommandName(ActionKeys.RtStop);
                yield return CreateCommandName(ActionKeys.RtRestart);
                yield return CreateCommandName(ActionKeys.RtResetTs);
                yield return CreateCommandName(ActionKeys.RtFocusGame);
                yield return CreateCommandName(ActionKeys.RtFocus2D);
                yield return CreateCommandName(ActionKeys.RtFocus3D);
                yield return CreateCommandName(ActionKeys.RtFocusScript);
                break;

            case ContextMode.Particles:
                yield return CreateCommandName(ActionKeys.PtEmitting);
                yield return CreateCommandName(ActionKeys.PtOneShot);
                yield return CreateCommandName(ActionKeys.PtRestart);
                yield return CreateCommandName(ActionKeys.PtSpeedScale);
                yield return CreateCommandName(ActionKeys.PtExplosiveness);
                yield return CreateCommandName(ActionKeys.PtRandomness);
                break;

            case ContextMode.Transform:
                yield return CreateCommandName(ActionKeys.TfVis);
                break;

            case ContextMode.Script:
                yield return CreateCommandName(ActionKeys.ScNew);
                yield return CreateCommandName(ActionKeys.ScSave);
                yield return CreateCommandName(ActionKeys.ScRun);
                yield return CreateCommandName(ActionKeys.ScCut);
                yield return CreateCommandName(ActionKeys.ScCopy);
                yield return CreateCommandName(ActionKeys.ScPaste);
                yield return CreateCommandName(ActionKeys.ScFind);
                yield return CreateCommandName(ActionKeys.ScHelp);
                break;

            default: // Idle
                yield return CreateCommandName(ActionKeys.IdlePlayCur);
                yield return CreateCommandName(ActionKeys.IdlePlayMain);
                yield return CreateCommandName(ActionKeys.IdleFocusGame);
                yield return CreateCommandName(ActionKeys.IdleFocus2D);
                yield return CreateCommandName(ActionKeys.IdleFocus3D);
                yield return CreateCommandName(ActionKeys.IdleFocusScript);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ENCODER DIALS
    // ═══════════════════════════════════════════════════════════════════════════

    public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType deviceType)
    {
        var mode = Detect(out var snapEnc);
        switch (mode)
        {
            case ContextMode.Runtime:
                yield return CreateAdjustmentName(ActionKeys.DialTimeScale);
                break;
            case ContextMode.Particles:
                yield return CreateAdjustmentName(ActionKeys.DialAmount);
                yield return CreateAdjustmentName(ActionKeys.DialLifetime);
                if (snapEnc.ParticlesSupportsAmountRatio)
                    yield return CreateAdjustmentName(ActionKeys.DialAmountRatio);
                yield return CreateAdjustmentName(ActionKeys.DialSpeedScale);
                yield return CreateAdjustmentName(ActionKeys.DialExplosiveness);
                yield return CreateAdjustmentName(ActionKeys.DialRandomness);
                break;
            case ContextMode.Transform:
                foreach (var k in NodeTransformHelper.AxisKeysFor(snapEnc))
                    yield return CreateAdjustmentName(k);
                break;
            // Script & Idle: no dials
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  COMMAND EXECUTION
    // ═══════════════════════════════════════════════════════════════════════════

    public override void RunCommand(String actionParameter)
    {
        switch (actionParameter)
        {
            // ── Runtime ─────────────────────────────────────────────────
            case ActionKeys.RtPause:
                if (Bridge.TryReadSnapshot(out var sp))
                    Bridge.SendBool(EventIds.Pause, !sp.RuntimePaused);
                break;
            case ActionKeys.RtStop:       Bridge.SendTrigger(EventIds.Stop);          break;
            case ActionKeys.RtRestart:    Bridge.SendTrigger(EventIds.Restart);       break;
            case ActionKeys.RtResetTs:   Bridge.SendTrigger(EventIds.ResetTimeScale); break;
            case ActionKeys.RtFocusGame:   Bridge.SendTrigger(EventIds.FocusGame);      break;
            case ActionKeys.RtFocus2D:     Bridge.SendTrigger(EventIds.Focus2DTab);      break;
            case ActionKeys.RtFocus3D:     Bridge.SendTrigger(EventIds.Focus3DTab);      break;
            case ActionKeys.RtFocusScript: Bridge.SendTrigger(EventIds.FocusScriptTab);  break;

            // ── Particles ───────────────────────────────────────────────
            case ActionKeys.PtEmitting:
                if (Bridge.TryReadSnapshot(out var spt))
                    Bridge.SendBool(EventIds.PtEmitting, !spt.ParticlesEmitting);
                break;
            case ActionKeys.PtOneShot:
                if (Bridge.TryReadSnapshot(out var spo))
                    Bridge.SendBool(EventIds.PtOneShot, !spo.ParticlesOneShot);
                break;
            case ActionKeys.PtRestart: Bridge.SendTrigger(EventIds.PtRestart); break;
            case ActionKeys.PtSpeedScale:
            case ActionKeys.PtExplosiveness:
            case ActionKeys.PtRandomness:
                break;

            // ── Transform (Node2D / Node3D) ─────────────────────────────
            case ActionKeys.TfVis:
                if (Bridge.TryReadSnapshot(out var sn) && sn.HasTransformNode)
                    Bridge.SendBool(EventIds.TfVisible, !sn.Visible);
                break;

            // ── Script ──────────────────────────────────────────────────
            case ActionKeys.ScNew:    Bridge.SendTrigger(EventIds.ScNew);        break;
            case ActionKeys.ScSave:   Bridge.SendTrigger(EventIds.ScSave);       break;
            case ActionKeys.ScRun:    Bridge.SendTrigger(EventIds.ScRun);        break;
            case ActionKeys.ScCut:    Bridge.SendTrigger(EventIds.ScCut);        break;
            case ActionKeys.ScCopy:   Bridge.SendTrigger(EventIds.ScCopy);       break;
            case ActionKeys.ScPaste:  Bridge.SendTrigger(EventIds.ScPaste);      break;
            case ActionKeys.ScFind:   Bridge.SendTrigger(EventIds.ScFind);       break;
            case ActionKeys.ScHelp:   Bridge.SendTrigger(EventIds.ScSearchHelp); break;

            // ── Idle ────────────────────────────────────────────────────
            case ActionKeys.IdlePlayCur:   Bridge.SendTrigger(EventIds.PlayCurrent); break;
            case ActionKeys.IdlePlayMain:  Bridge.SendTrigger(EventIds.PlayMain);    break;
            case ActionKeys.IdleFocusGame:   Bridge.SendTrigger(EventIds.FocusGame);     break;
            case ActionKeys.IdleFocus2D:     Bridge.SendTrigger(EventIds.Focus2DTab);     break;
            case ActionKeys.IdleFocus3D:     Bridge.SendTrigger(EventIds.Focus3DTab);     break;
            case ActionKeys.IdleFocusScript: Bridge.SendTrigger(EventIds.FocusScriptTab); break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ADJUSTMENT (DIAL) LOGIC
    // ═══════════════════════════════════════════════════════════════════════════

    public override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (!Bridge.TryReadSnapshot(out var snap)) return;

        switch (actionParameter)
        {
            // Time scale (preset stepping)
            case ActionKeys.DialTimeScale:
                var idx = TimeScalePresetHelper.FindClosestPresetIndex(snap.EngineTimeScale);
                idx = Math.Clamp(idx + diff, 0, TimeScalePresetHelper.Presets.Length - 1);
                Bridge.SendFloat(EventIds.TimeScale, TimeScalePresetHelper.Presets[idx]);
                break;

            // Particles dials
            case ActionKeys.DialAmount:
                if (snap.HasParticles)
                {
                    var deltaAmt = ParticleAmountDialHelper.DeltaFromEncoderDiff(diff);
                    if (deltaAmt != 0)
                    {
                        var nextAmt = ParticleAmountDialHelper.ClampMin1(snap.ParticlesAmount + deltaAmt);
                        Bridge.SendInt(EventIds.PtAmount, nextAmt);
                    }
                }
                break;
            case ActionKeys.DialLifetime:
                if (snap.HasParticles && diff != 0)
                    Bridge.SendFloat(EventIds.PtLifetime,
                        ParticleLifetimeDialHelper.ApplyEncoderDiff(snap.ParticlesLifetime, diff));
                break;
            case ActionKeys.DialAmountRatio:
                if (snap.HasParticles && snap.ParticlesSupportsAmountRatio && diff != 0)
                    Bridge.SendFloat(EventIds.PtAmountRatio,
                        ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesAmountRatio, diff));
                break;
            case ActionKeys.DialSpeedScale:
                if (snap.HasParticles && diff != 0)
                    Bridge.SendFloat(EventIds.PtSpeedScale,
                        ParticleSpeedScaleDialHelper.ApplyEncoderDiff(snap.ParticlesSpeedScale, diff));
                break;
            case ActionKeys.DialExplosiveness:
                if (snap.HasParticles && diff != 0)
                    Bridge.SendFloat(EventIds.PtExplosiveness,
                        ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesExplosiveness, diff));
                break;
            case ActionKeys.DialRandomness:
                if (snap.HasParticles && diff != 0)
                    Bridge.SendFloat(EventIds.PtRandomness,
                        ParticleAmountRatioHelper.ApplyEncoderDiff(snap.ParticlesRandomness, diff));
                break;

            // Transform axes
            default:
                if (snap.HasTransformNode && NodeTransformHelper.IsAxisKey(actionParameter)
                                          && NodeTransformHelper.AxisApplies(actionParameter, snap))
                {
                    if (diff != 0)
                        NodeTransformAdjustmentTracker.SetActive(actionParameter);
                    NodeTransformHelper.ApplyDelta(actionParameter, diff, Bridge, snap);
                }

                break;
        }
        AdjustmentValueChanged(actionParameter);
    }

    public override Boolean ProcessTouchEvent(String actionParameter, DeviceTouchEvent touchEvent)
    {
        if (touchEvent.EventType is DeviceTouchEventType.TouchUp or DeviceTouchEventType.LongRelease
            && NodeTransformHelper.IsAxisKey(actionParameter)
            && String.Equals(NodeTransformAdjustmentTracker.ActiveKey, actionParameter, StringComparison.Ordinal))
            NodeTransformAdjustmentTracker.Clear();
        return base.ProcessTouchEvent(actionParameter, touchEvent);
    }

    public override String? GetAdjustmentValue(String actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap)) return null;
        return actionParameter switch
        {
            ActionKeys.DialTimeScale => TimeScalePresetHelper.BuildEncoderReadoutForSnapshot(snap),
            ActionKeys.DialAmount => snap.HasParticles ? $"{snap.ParticlesAmount}" : null,
            ActionKeys.DialLifetime => snap.HasParticles ? $"{snap.ParticlesLifetime:F2}s" : null,
            ActionKeys.DialAmountRatio => snap.HasParticles && snap.ParticlesSupportsAmountRatio ? $"{snap.ParticlesAmountRatio:P0}" : null,
            ActionKeys.DialSpeedScale => snap.HasParticles ? $"{snap.ParticlesSpeedScale:F2}×" : null,
            ActionKeys.DialExplosiveness => snap.HasParticles ? $"{snap.ParticlesExplosiveness:P0}" : null,
            ActionKeys.DialRandomness => snap.HasParticles ? $"{snap.ParticlesRandomness:P0}" : null,
            _ when snap.HasTransformNode && NodeTransformHelper.AxisApplies(actionParameter, snap)
                                         && NodeTransformAdjustmentTracker.TryGetPendingResetDisplay(actionParameter, snap, out var pending) =>
                pending,
            _ => snap.HasTransformNode && NodeTransformHelper.AxisApplies(actionParameter, snap)
                ? NodeTransformHelper.GetDisplayValue(actionParameter, snap)
                : null,
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  REACTIVE ICONS (SVG)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a state-sensitive SVG icon for each command button.
    /// Icons react to context: pause↔play, eye open↔closed, emitting on↔off, etc.
    /// </summary>
    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon(actionParameter, snap);
    }

    /// <summary>
    /// Returns a descriptive SVG icon for each encoder dial.
    /// </summary>
    public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
    {
        return SvgIcons.GetDialIcon(actionParameter);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  DISPLAY NAMES
    // ═══════════════════════════════════════════════════════════════════════════

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            // Runtime
            ActionKeys.RtPause      => Bridge.TryReadSnapshot(out var spp) && spp.RuntimePaused ? "Resume" : "Pause",
            ActionKeys.RtStop       => "Stop",
            ActionKeys.RtRestart    => "Restart",
            ActionKeys.RtResetTs   => "Reset TS",
            ActionKeys.RtFocusGame   => "Game Tab",
            ActionKeys.RtFocus2D     => "2D Tab",
            ActionKeys.RtFocus3D     => "3D Tab",
            ActionKeys.RtFocusScript => "Script Tab",
            // Particles
            ActionKeys.PtEmitting   => Bridge.TryReadSnapshot(out var spe) && spe.ParticlesEmitting ? "Emitting" : "Stopped",
            ActionKeys.PtOneShot   => Bridge.TryReadSnapshot(out var spo) && spo.ParticlesOneShot ? "One Shot" : "Loop",
            ActionKeys.PtRestart    => "Restart",
            ActionKeys.PtSpeedScale => Bridge.TryReadSnapshot(out var sps) && sps.HasParticles
                ? $"Speed {sps.ParticlesSpeedScale:F2}×"
                : "Speed",
            ActionKeys.PtExplosiveness => Bridge.TryReadSnapshot(out var spex) && spex.HasParticles
                ? $"Expl. {spex.ParticlesExplosiveness:P0}"
                : "Explosive",
            ActionKeys.PtRandomness => Bridge.TryReadSnapshot(out var spr) && spr.HasParticles
                ? $"Rand {spr.ParticlesRandomness:P0}"
                : "Random",
            // Transform
            ActionKeys.TfVis       => Bridge.TryReadSnapshot(out var snv) && snv.Visible ? "Visible" : "Hidden",
            // Script
            ActionKeys.ScNew        => "New",
            ActionKeys.ScSave       => "Save",
            ActionKeys.ScRun        => "Run",
            ActionKeys.ScCut        => "Cut",
            ActionKeys.ScCopy       => "Copy",
            ActionKeys.ScPaste      => "Paste",
            ActionKeys.ScFind       => "Find",
            ActionKeys.ScHelp       => "Help",
            // Idle
            ActionKeys.IdlePlayCur   => "Play Scene",
            ActionKeys.IdlePlayMain  => "Play Main",
            ActionKeys.IdleFocusGame   => "Game Tab",
            ActionKeys.IdleFocus2D     => "2D Tab",
            ActionKeys.IdleFocus3D     => "3D Tab",
            ActionKeys.IdleFocusScript => "Script Tab",
            _ => actionParameter,
        };

    public override String? GetAdjustmentDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            ActionKeys.DialTimeScale                => "Time Scale",
            ActionKeys.DialAmount       => "Amount",
            ActionKeys.DialLifetime     => "Lifetime",
            ActionKeys.DialAmountRatio => "Am. Ratio",
            ActionKeys.DialSpeedScale    => "Speed",
            ActionKeys.DialExplosiveness => "Explosive",
            ActionKeys.DialRandomness    => "Random",
            _ => NodeTransformHelper.GetDisplayName(actionParameter),
        };

    public override String GetButtonDisplayName(PluginImageSize imageSize)
    {
        var mode = Detect(out var snap);
        return mode switch
        {
            ContextMode.Runtime   => $"▶ Runtime ({TimeScalePresetHelper.FormatLabel(snap.EngineTimeScale)})",
            ContextMode.Particles => "✦ Particles",
            ContextMode.Transform => TruncateName(
                snap.TransformPath,
                snap.TransformKind == NodeTransformKind.Node2D ? "Node2D" : "Node3D"),
            ContextMode.Script    => TruncateName(snap.ScriptFileName, "Script"),
            _                     => "Godot",
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static string TruncateName(string? path, string fallback)
    {
        if (string.IsNullOrEmpty(path)) return fallback;
        var name = path.Contains('/') ? path.Split('/').Last() : Path.GetFileName(path);
        return name.Length > 12 ? name[..12] + "…" : name;
    }
}
