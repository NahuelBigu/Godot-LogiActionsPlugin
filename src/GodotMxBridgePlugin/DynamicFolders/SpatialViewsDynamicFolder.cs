namespace Loupedeck.GodotMxBridge;

/// <summary>Orthographic / perspective views and standard axis views for the 3D editor.</summary>
public sealed class SpatialViewsDynamicFolder : PluginDynamicFolder
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public SpatialViewsDynamicFolder()
    {
        DisplayName = "3D Views";
        GroupName   = "3D View";
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            CreateCommandName("top"),
            CreateCommandName("bottom"),
            CreateCommandName("front"),
            CreateCommandName("rear"),
            CreateCommandName("right"),
            CreateCommandName("left"),
            CreateCommandName("persp_ortho"),
        };

    public override void RunCommand(string actionParameter)
    {
        switch (actionParameter)
        {
            case "top":
                Bridge.SendEditorShortcut("spatial_editor/top_view");
                break;
            case "bottom":
                Bridge.SendEditorShortcut("spatial_editor/bottom_view");
                break;
            case "front":
                Bridge.SendEditorShortcut("spatial_editor/front_view");
                break;
            case "rear":
                Bridge.SendEditorShortcut("spatial_editor/rear_view");
                break;
            case "right":
                Bridge.SendEditorShortcut("spatial_editor/right_view");
                break;
            case "left":
                Bridge.SendEditorShortcut("spatial_editor/left_view");
                break;
            case "persp_ortho":
                Bridge.SendEditorShortcut("spatial_editor/switch_perspective_orthogonal");
                break;
        }
    }

    /// <summary>Tile on the parent page (“3D Views” folder entry).</summary>
    public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
        SvgIcons.GetSpatialViewsFolderIcon(imageSize);

    public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        actionParameter == PluginDynamicFolder.NavigateUpActionName
            ? base.GetCommandImage(actionParameter, imageSize)
            : SvgIcons.GetSpatialViewsFolderIcon(imageSize);

    public override string? GetCommandDisplayName(string actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            _ when actionParameter == PluginDynamicFolder.NavigateUpActionName => "Back",
            "top"        => "Top",
            "bottom"     => "Bottom",
            "front"      => "Front",
            "rear"       => "Rear",
            "right"      => "Right",
            "left"       => "Left",
            "persp_ortho" => "Persp / Ortho",
            _            => actionParameter,
        };

    public override string GetButtonDisplayName(PluginImageSize imageSize) => "3D Views";
}
