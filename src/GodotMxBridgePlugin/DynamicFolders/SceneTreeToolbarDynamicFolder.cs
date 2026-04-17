namespace Loupedeck.GodotMxBridge;

/// <summary>Scene dock: add child node (+), instantiate child scene (link), attach script.</summary>
public sealed class SceneTreeToolbarDynamicFolder : PluginDynamicFolder
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public SceneTreeToolbarDynamicFolder()
    {
        DisplayName = "Scene tree";
        GroupName   = "Scene";
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            CreateCommandName(ActionKeys.SceneAddChild),
            CreateCommandName(ActionKeys.SceneInstantiate),
            CreateCommandName(ActionKeys.SceneAttachScript),
        };

    public override void RunCommand(string actionParameter)
    {
        switch (actionParameter)
        {
            case ActionKeys.SceneAddChild:
                Bridge.SendEditorShortcut("scene_tree/add_child_node");
                break;
            case ActionKeys.SceneInstantiate:
                Bridge.SendEditorShortcut("scene_tree/instantiate_scene");
                break;
            case ActionKeys.SceneAttachScript:
                Bridge.SendEditorShortcut("scene_tree/attach_script");
                break;
        }
    }

    public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
        SvgIcons.GetSceneTreeFolderIcon(imageSize);

    public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        actionParameter == PluginDynamicFolder.NavigateUpActionName
            ? base.GetCommandImage(actionParameter, imageSize)
            : SvgIcons.GetSceneTreeToolbarIcon(actionParameter, imageSize);

    public override string? GetCommandDisplayName(string actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            _ when actionParameter == PluginDynamicFolder.NavigateUpActionName => "Back",
            ActionKeys.SceneAddChild    => "Add child node",
            ActionKeys.SceneInstantiate => "Instance child scene",
            ActionKeys.SceneAttachScript => "Attach script",
            _                   => actionParameter,
        };

    public override string GetButtonDisplayName(PluginImageSize imageSize) => "Scene tree";
}
