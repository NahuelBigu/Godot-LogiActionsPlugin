namespace Loupedeck.GodotMxBridge;

/// <summary>Which node type supplies the unified transform snapshot (position / rotation_deg / scale_uniform).</summary>
public enum NodeTransformKind
{
    None  = 0,
    Node2D = 1,
    Node3D = 2,
}
