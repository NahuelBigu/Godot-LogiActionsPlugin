namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dial adjustment for engine time scale.
/// Steps through Godot's preset values: 1/16×, 1/8×, 1/4×, 1/2×, 3/4×, 1.0×, 1.25× ... 16.0×
/// </summary>
public class TimeScaleAdjustment : PluginDynamicAdjustment
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public TimeScaleAdjustment()
        : base("Time Scale", "Adjust engine time scale with the dial", "Playback", hasReset: true)
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        if (Bridge != null)
            Bridge.ContextChanged += OnBridgeContextChanged;
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        if (Bridge != null)
            Bridge.ContextChanged -= OnBridgeContextChanged;
        return base.OnUnload();
    }

    private void OnBridgeContextChanged() => AdjustmentValueChanged();

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (!Bridge.TryReadSnapshot(out var snap)) return;
        if (!snap.IsPlaying) return;
        var idx = TimeScalePresetHelper.FindClosestPresetIndex(snap.EngineTimeScale);
        idx = Math.Clamp(idx + diff, 0, TimeScalePresetHelper.Presets.Length - 1);
        Bridge.SendFloat(EventIds.TimeScale, TimeScalePresetHelper.Presets[idx]);
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.IsPlaying) return;
        Bridge.SendTrigger(EventIds.ResetTimeScale);
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon("ts");

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap)) return "…";
        return TimeScalePresetHelper.BuildEncoderReadoutForSnapshot(snap);
    }
}
