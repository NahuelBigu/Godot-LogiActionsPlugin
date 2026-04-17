using System;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dial adjustment for the currently focused Inspector property.
/// Context-aware behavior:
/// - When a float/int range property is focused in the Inspector: adjust that property.
/// - Else when an AnimationPlayer context is active: scrub the animation timeline.
/// Each tick sends a relative step delta; Godot applies against the live property value (avoids stale HTTP value).
/// </summary>
public class InspectorPropAdjustment : PluginDynamicAdjustment
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public InspectorPropAdjustment()
        : base("Inspector Property", "Adjust focused inspector property", "Inspector", hasReset: false)
    {
        this.DisableLoupedeckLocalization();
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetInspectorPropIcon(imageSize);

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0) return;

        if (Bridge.TryReadFocusedProp(out _))
        {
            Bridge.SendInspectorPropStepDelta(NodeTransformHelper.VelocityTicks(diff));
            return;
        }

        if (Bridge.TryReadSnapshot(out var snap) && snap.HasAnimation)
            Bridge.SendFloat(EventIds.AnimScrub, diff);
    }

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (Bridge.TryReadFocusedProp(out var prop))
            return $"{prop.Label}: {prop.Value:G}";

        if (Bridge.TryReadSnapshot(out var snap) && snap.HasAnimation)
            return $"{snap.AnimationPosition:F3}s / {snap.AnimationLength:F2}s";

        return "\u2014";
    }
}
