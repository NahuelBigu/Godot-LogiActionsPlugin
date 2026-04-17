namespace Loupedeck.GodotMxBridge;

/// <summary>Event kind strings used in the bridge JSON protocol.</summary>
public static class EventKind
{
    public const string Trigger         = "trigger";
    public const string SetBool         = "set_bool";
    public const string SetFloat        = "set_float";
    public const string SetInt          = "set_int";
    /// <summary>Invokes an <see cref="EditorSettings"/> shortcut path in Godot (e.g. <c>spatial_editor/top_view</c>).</summary>
    public const string EditorShortcut = "editor_shortcut";
}
