namespace Loupedeck.GodotMxBridge;

/// <summary>
/// English strings used as XLIFF source IDs for plugin status only (<see cref="Loupedeck.Plugin.OnPluginStatusChanged"/>).
/// Action names stay English via <see cref="PluginLocalizationBehavior.DisableLoupedeckLocalization"/>.
/// </summary>
internal static class GodotBridgeStatusMessages
{
    /// <summary>Matches <c>_get_plugin_name()</c> in the Godot editor addon.</summary>
    public const string GodotAddonDisplayName = "MX Console";

    public const string SetupHelpUrl = "https://youtu.be/u0L-FMGce5o";

    /// <summary>Hyperlink label in Options+ / Loupedeck plugin error status.</summary>
    public const string SetupHelpLinkTitle = "How to install and enable the addon in Godot";

    /// <summary>Status until the HTTP bridge responds.</summary>
    public const string DefaultNotConnected =
        "No connection to the Godot editor. Install and enable the \"MX Console\" addon, open a project, and keep the editor running.";

    public const string BridgeDisconnected =
        "Lost connection to the editor bridge. Make sure Godot is still running and the \"MX Console\" addon is enabled under Project → Project Settings → Plugins.";
}
