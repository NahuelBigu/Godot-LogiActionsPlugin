namespace Loupedeck.GodotMxBridge;

public sealed class SceneTreeAddChildNodeCommand : GodotEditorShortcutCommandBase
{
    public SceneTreeAddChildNodeCommand()
        : base(
            "Scene - Add child node",
            "Scene dock: Add Child Node (+)",
            "Scene",
            "scene_tree/add_child_node")
    {
    }

    protected override BitmapImage GetShortcutCommandIcon(PluginImageSize imageSize) =>
        SvgIcons.GetSceneTreeToolbarIcon(ActionKeys.SceneAddChild, imageSize);
}
