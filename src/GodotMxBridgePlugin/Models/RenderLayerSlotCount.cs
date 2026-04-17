namespace Loupedeck.GodotMxBridge;

/// <summary>20 render-layer slots per Godot 4.6 VisualInstance3D (set_layer_mask_value / get_layer_mask_value: layer 1–20; see docs.godotengine.org/en/4.6/classes/class_visualinstance3d.html). MX uses the lower 20 bits of the layers mask; CanvasItem visibility uses the same 20 UI slots (API bit index 0–19).</summary>
internal static class RenderLayerSlotCount
{
    public const int Value = 20;
}
