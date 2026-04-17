namespace Loupedeck.GodotMxBridge;

public sealed class SceneTreeAttachScriptCommand : GodotEditorShortcutCommandBase
{
    public SceneTreeAttachScriptCommand()
        : base(
            "Scene - Attach script",
            "Scene dock: Attach Script (works even if the shortcut has no key bound)",
            "Scene",
            "scene_tree/attach_script")
    {
    }

    protected override BitmapImage GetShortcutCommandIcon(PluginImageSize imageSize) =>
        SvgIcons.GetSceneTreeToolbarIcon(ActionKeys.SceneAttachScript, imageSize);
}
