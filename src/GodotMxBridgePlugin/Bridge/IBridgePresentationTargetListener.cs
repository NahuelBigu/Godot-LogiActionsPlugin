namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Optional contract for plugin pieces that refresh labels/icons when Godot’s primary editor target changes
/// (e.g. selected <c>Node3D</c> path). Subscribe to <see cref="IBridgeTransport.PresentationTargetChanged"/> or implement
/// this type for documentation / future DI-style registration.
/// </summary>
public interface IBridgePresentationTargetListener
{
    /// <summary>Invoked when the bridge detects a new presentation target (e.g. selection path).</summary>
    void OnPresentationTargetChanged(ContextSnapshot snapshot);
}
