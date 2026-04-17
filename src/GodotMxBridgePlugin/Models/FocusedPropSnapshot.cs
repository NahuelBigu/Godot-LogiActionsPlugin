namespace Loupedeck.GodotMxBridge;

/// <summary>Describes a float/int range property currently focused in the Godot Inspector.</summary>
public sealed class FocusedPropSnapshot
{
    public string Label { get; init; } = "";
    public double Min   { get; init; }
    public double Max   { get; init; } = 1.0;
    public double Step  { get; init; } = 0.001;
    public double Value { get; init; }
}
