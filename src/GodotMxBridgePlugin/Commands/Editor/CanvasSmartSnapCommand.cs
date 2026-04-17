namespace Loupedeck.GodotMxBridge;

public sealed class CanvasSmartSnapCommand : EditorSnapReactiveCommandBase
{
    public CanvasSmartSnapCommand()
        : base(
            "Smart Snap",
            "Toggle canvas smart snap (2D editor)",
            "2D Canvas",
            "canvas_item_editor/use_smart_snap",
            "Smart Snap",
            static s => s.CanvasSmartSnapActive)
    {
    }

    protected override BitmapImage GetSnapIcon(Boolean active) => SvgIcons.GetEditorSnapSmartIcon(active);
}
