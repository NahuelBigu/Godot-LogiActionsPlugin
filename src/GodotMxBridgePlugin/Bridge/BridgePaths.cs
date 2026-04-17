namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Bridge file paths used by the C# plugin.
/// Communication with Godot is exclusively via HTTP loopback; this directory is only
/// used for the local audit log written when events are sent.
/// </summary>
internal static class BridgePaths
{
    public const string SubFolder = "GodotMXCreativeConsole";

    public static string BridgeDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            SubFolder);

    public static string AuditLogPath => Path.Combine(BridgeDirectory, "bridge_audit.log");
}
