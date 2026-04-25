using System;
using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dial: scrub animation time, unless a float/int Inspector property is focused — then adjusts that first.
/// Encoder steps → Godot <c>mx.anim.scrub</c> or <c>mx.inspector.focused_prop</c>.
/// Touch/press the encoder button = insert a keyframe at the current time (animation context).
/// </summary>
public sealed class AnimationTimeAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private String? _lastPaintKey;

    public AnimationTimeAdjustment()
        : base("Anim - Time Scrub", "Dial: scrub animation time - Press: insert key", "Animation", hasReset: false)
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snap)
    {
        String key;
        if (Bridge.TryReadFocusedProp(out var prop))
        {
            key = "F:" + prop.Label + ":" + prop.Value.ToString("G9", CultureInfo.InvariantCulture);
        }
        else if (!snap.HasAnimation)
            key = "A:0";
        else
            key = "A:1:" + snap.AnimationPosition.ToString("F3", CultureInfo.InvariantCulture) + ":" +
                  snap.AnimationLength.ToString("F2", CultureInfo.InvariantCulture);

        if (key == _lastPaintKey) return;
        _lastPaintKey = key;
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0) return;

        if (Bridge.TryReadFocusedProp(out _))
        {
            Bridge.SendInspectorPropStepDelta(NodeTransformHelper.VelocityTicks(diff));
            AdjustmentValueChanged();
            return;
        }

        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendFloat(EventIds.AnimScrub, diff);
        AdjustmentValueChanged();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimInsertKey);
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_time");

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (Bridge.TryReadFocusedProp(out var prop))
            return $"{prop.Label}: {prop.Value:G}";

        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return "—";
        return $"{snap.AnimationPosition:F3}s / {snap.AnimationLength:F2}s";
    }
}
