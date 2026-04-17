namespace Loupedeck.GodotMxBridge;

/// <summary>Schema version constants for bridge JSON files.</summary>
internal static class BridgeSchema
{
    /// <summary>Schema version of <c>context.json</c> written by the Godot addon.</summary>
    public const int Context = 2;

    /// <summary>Schema version of <c>events.json</c> exchanged between both sides.</summary>
    public const int Events = 1;
}
