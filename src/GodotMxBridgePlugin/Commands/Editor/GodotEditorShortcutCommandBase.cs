namespace Loupedeck.GodotMxBridge;

/// <summary>Sends <see cref="IBridgeTransport.SendEditorShortcut"/> for a fixed Godot <c>EditorSettings</c> action path.</summary>
public abstract class GodotEditorShortcutCommandBase : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly string _shortcutPath;

    protected GodotEditorShortcutCommandBase(string displayName, string description, string group, string shortcutPath)
        : base(displayName, description, group)
    {
        _shortcutPath = shortcutPath;
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter) => Bridge.SendEditorShortcut(_shortcutPath);

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        GetShortcutCommandIcon(imageSize);

    /// <summary>Override in subclasses that ship a dedicated tile SVG.</summary>
    protected virtual BitmapImage GetShortcutCommandIcon(PluginImageSize imageSize) =>
        SvgIcons.GetGodotBrandingIcon(imageSize);
}
