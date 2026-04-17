namespace Loupedeck.GodotMxBridge;

public sealed class SpatialUseSnapCommand : EditorSnapReactiveCommandBase
{
    public SpatialUseSnapCommand()
        : base(
            "Use Snap",
            "Toggle 3D editor transform snap",
            "3D View",
            "spatial_editor/snap",
            "Use Snap",
            static s => s.SpatialSnapActive)
    {
    }

    protected override BitmapImage GetSnapIcon(Boolean active) => SvgIcons.GetEditorSnapSmartIcon(active);
}
