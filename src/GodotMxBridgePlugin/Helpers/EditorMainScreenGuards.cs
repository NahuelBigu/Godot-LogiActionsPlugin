namespace Loupedeck.GodotMxBridge;

internal static class EditorMainScreenGuards
{
    /// <summary>Matches Godot <c>EditorPlugin.main_screen_changed</c> tab id (EN <c>3D</c> or localized labels containing <c>3D</c>).</summary>
    public static bool Is3DView(string? mainScreen)
    {
        if (string.IsNullOrWhiteSpace(mainScreen))
            return false;
        var t = mainScreen.Trim();
        if (t.Equals("3D", StringComparison.OrdinalIgnoreCase))
            return true;
        var u = t.ToUpperInvariant();
        return u.Contains(" 3D", StringComparison.Ordinal)
               || u.EndsWith("3D", StringComparison.Ordinal)
               || u.StartsWith("3D", StringComparison.Ordinal);
    }
}
