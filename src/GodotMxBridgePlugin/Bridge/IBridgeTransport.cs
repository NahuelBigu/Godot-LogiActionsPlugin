namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Abstraction over the transport mechanism between the Logitech plugin and Godot.
/// <para>
/// Implementation: <see cref="HttpBridgeTransport"/> — Godot serves GET /context and POST /events on loopback.
/// </para>
/// </summary>
public interface IBridgeTransport
{
    /// <summary>Read the latest context snapshot from Godot.</summary>
    bool TryReadSnapshot(out ContextSnapshot snapshot);

    /// <summary>Drop any short-lived HTTP snapshot cache so the next read hits Godot (diagnostics / forced UI refresh).</summary>
    void RequestFreshSnapshot();

    /// <summary>Read the focused Inspector property from the current context.</summary>
    bool TryReadFocusedProp(out FocusedPropSnapshot prop);

    /// <summary>Send a trigger event (no value) to Godot.</summary>
    void SendTrigger(string eventId);

    /// <summary>Send a boolean value to Godot.</summary>
    void SendBool(string eventId, bool value);

    /// <summary>Send a float value to Godot.</summary>
    void SendFloat(string eventId, double value);
    void SendRelativeFloat(string eventId, double deltaValue);

    /// <summary>
    /// Inspector: move focused RANGE property by <paramref name="deltaSteps"/> Godot <c>step</c> units (live value on apply).
    /// </summary>
    void SendInspectorPropStepDelta(int deltaSteps);

    /// <summary>Send an integer value to Godot.</summary>
    void SendInt(string eventId, int value);

    /// <summary>Run a Godot editor shortcut by <see cref="EditorSettings"/> action id (e.g. <c>spatial_editor/top_view</c>).</summary>
    void SendEditorShortcut(string editorShortcutPath);

    /// <summary>Raised when the context snapshot changes (new data from Godot).</summary>
    event Action? ContextChanged;

    /// <summary>
    /// Raised when the user’s primary editable target identity changes (e.g. selected Node3D path),
    /// not on every numeric tweak. Use for dial titles / icons; pair with <see cref="ContextChanged"/> for values.
    /// </summary>
    event Action<ContextSnapshot>? PresentationTargetChanged;

    /// <summary>Start listening/polling for context changes.</summary>
    void Start();

    /// <summary>Stop listening/polling.</summary>
    void Stop();
}
