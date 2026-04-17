namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for runtime controls: Play/Pause/Stop/Restart + Time Scale dial.
/// Subscribes to <see cref="IBridgeTransport.ContextChanged"/> to refresh the console UI
/// when Godot state changes.
/// </summary>
public class GameDynamicFolder : BridgeDynamicFolder
{
    public GameDynamicFolder()
    {
        DisplayName = "Game";
        GroupName   = "Playback";
    }

    protected override void OnContextChanged()
    {
        CommandImageChanged("pause");
        AdjustmentValueChanged("ts");
    }

    // ── Touch grid ──────────────────────────────────────────────────────────

    static Boolean IsGameTab(String? ms) =>
        !String.IsNullOrEmpty(ms)
        && (ms.Equals("Game", StringComparison.OrdinalIgnoreCase)
            || ms.Equals("Juego", StringComparison.OrdinalIgnoreCase));

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
    {
        if (!Bridge.TryReadSnapshot(out var s))
        {
            foreach (var x in TabFocusCommands)
                yield return CreateCommandName(x);
            yield break;
        }

        if (s.IsPlaying)
        {
            yield return CreateCommandName("pause");
            yield return CreateCommandName("stop");
            yield return CreateCommandName("restart");
            yield return CreateCommandName("reset_ts");
            foreach (var x in TabFocusCommands)
                yield return CreateCommandName(x);
        }
        else if (IsGameTab(s.MainScreen))
        {
            yield return CreateCommandName("play_cur");
            yield return CreateCommandName("play_main");
            foreach (var x in TabFocusCommands)
                yield return CreateCommandName(x);
        }
        else
        {
            foreach (var x in TabFocusCommands)
                yield return CreateCommandName(x);
        }
    }

    private static readonly String[] TabFocusCommands = ["focus_game", "focus_2d", "focus_3d", "focus_script"];

    public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType _) =>
        Bridge.TryReadSnapshot(out var s) && s.IsPlaying
            ? new[] { CreateAdjustmentName("ts") }
            : Array.Empty<String>();

    // ── Commands ─────────────────────────────────────────────────────────────

    public override void RunCommand(String actionParameter)
    {
        switch (actionParameter)
        {
            case "pause":
                if (Bridge.TryReadSnapshot(out var s))
                    Bridge.SendBool(EventIds.Pause, !s.RuntimePaused);
                break;
            case "stop":       Bridge.SendTrigger(EventIds.Stop);          break;
            case "restart":    Bridge.SendTrigger(EventIds.Restart);       break;
            case "reset_ts":   Bridge.SendTrigger(EventIds.ResetTimeScale); break;
            case "play_cur":   Bridge.SendTrigger(EventIds.PlayCurrent);   break;
            case "play_main":  Bridge.SendTrigger(EventIds.PlayMain);      break;
            case "focus_game":   Bridge.SendTrigger(EventIds.FocusGame);      break;
            case "focus_2d":     Bridge.SendTrigger(EventIds.Focus2DTab);      break;
            case "focus_3d":     Bridge.SendTrigger(EventIds.Focus3DTab);      break;
            case "focus_script": Bridge.SendTrigger(EventIds.FocusScriptTab);  break;
        }
    }

    // ── Adjustments (Time Scale — steps through Godot presets) ───────────────

    public override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (actionParameter != "ts") return;
        if (!Bridge.TryReadSnapshot(out var snap)) return;

        var idx = TimeScalePresetHelper.FindClosestPresetIndex(snap.EngineTimeScale);
        idx = Math.Clamp(idx + diff, 0, TimeScalePresetHelper.Presets.Length - 1);
        Bridge.SendFloat(EventIds.TimeScale, TimeScalePresetHelper.Presets[idx]);
        AdjustmentValueChanged(actionParameter);
    }

    public override String? GetAdjustmentValue(String actionParameter)
    {
        if (actionParameter != "ts") return null;
        if (!Bridge.TryReadSnapshot(out var snap)) return null;
        return TimeScalePresetHelper.BuildEncoderReadoutForSnapshot(snap);
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            "pause"      => "Pause / resume",
            "stop"       => "Stop game",
            "restart"    => "Restart scene",
            "reset_ts"   => "Reset time scale (1×)",
            "play_cur"   => "Run current scene",
            "play_main"  => "Run main scene",
            "focus_game"   => "Open Game tab",
            "focus_2d"     => "Open 2D tab",
            "focus_3d"     => "Open 3D tab",
            "focus_script" => "Open Script tab",
            _              => actionParameter,
        };

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        var reactiveKey = actionParameter switch
        {
            "pause"     => "rt_pause",
            "stop"      => "rt_stop",
            "restart"   => "rt_restart",
            "reset_ts"  => "rt_reset_ts",
            "play_cur"  => "idle_play_cur",
            "play_main" => "idle_play_main",
            _           => actionParameter,
        };
        return SvgIcons.GetReactiveIcon(reactiveKey, snap);
    }

    public override String? GetAdjustmentDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter == "ts" ? "Time scale" : actionParameter;

}
