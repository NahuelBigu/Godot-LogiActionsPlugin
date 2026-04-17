namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Compares materialized action-name lists so we only call <see cref="PluginDynamicFolder.ButtonActionNamesChanged"/>
/// / <see cref="PluginDynamicFolder.EncoderActionNamesChanged"/> when the layout actually changes — otherwise the host
/// resets multi-page touch folders to page 1 (Logi Actions SDK: those methods mean “the list of actions changed”).
/// </summary>
internal static class DynamicFolderLayoutCache
{
    public static Boolean AreEqual(String[]? previous, String[] current)
    {
        if (previous is null) return false;
        if (previous.Length != current.Length) return false;
        for (var i = 0; i < previous.Length; i++)
        {
            if (!String.Equals(previous[i], current[i], StringComparison.Ordinal)) return false;
        }

        return true;
    }
}
