namespace Loupedeck.GodotMxBridge;

public sealed class CanvasGridSnapCommand : EditorSnapReactiveCommandBase
{
    public CanvasGridSnapCommand()
        : base(
            "Grid Snap",
            "Toggle canvas grid snap (2D editor)",
            "2D Canvas",
            "canvas_item_editor/use_grid_snap",
            "Grid Snap",
            static s => s.CanvasGridSnapActive)
    {
    }

    protected override BitmapImage GetSnapIcon(Boolean active) => SvgIcons.GetEditorSnapGridIcon(active);
}
