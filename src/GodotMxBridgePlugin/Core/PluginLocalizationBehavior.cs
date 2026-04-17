namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Catalog / action metadata stays English-only; only plugin status (bridge errors) uses <see cref="Loupedeck.Plugin.Localization"/>.
/// </summary>
internal static class PluginLocalizationBehavior
{
    public static void DisableLoupedeckLocalization(this PluginDynamicCommand command)
    {
        command.SetLocalize(false);
        command.SetLocalizeParameters(false);
    }

    public static void DisableLoupedeckLocalization(this PluginDynamicAdjustment adjustment) =>
        adjustment.SetLocalize(false);
}
