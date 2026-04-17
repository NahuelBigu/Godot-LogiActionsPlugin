namespace Loupedeck.GodotMxBridge;

public sealed class SceneTreeInstantiateChildSceneCommand : GodotEditorShortcutCommandBase
{
    public SceneTreeInstantiateChildSceneCommand()
        : base(
            "Scene - Instantiate child scene",
            "Scene dock: Instantiate Child Scene",
            "Scene",
            "scene_tree/instantiate_scene")
    {
    }

    protected override BitmapImage GetShortcutCommandIcon(PluginImageSize imageSize) =>
        SvgIcons.GetSceneTreeToolbarIcon(ActionKeys.SceneInstantiate, imageSize);
}
