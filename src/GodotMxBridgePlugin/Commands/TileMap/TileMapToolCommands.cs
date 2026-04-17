namespace Loupedeck.GodotMxBridge;

public sealed class TileMapSelectCommand : TileMapToolReactiveCommandBase
{
    public TileMapSelectCommand()
        : base(
            "TileMap - Select",
            "TileMap editor: selection tool (tiles_editor/selection_tool)",
            "select",
            "Select")
    {
    }
}

public sealed class TileMapPaintCommand : TileMapToolReactiveCommandBase
{
    public TileMapPaintCommand()
        : base(
            "TileMap - Paint",
            "TileMap editor: paint tool (Editor Shortcuts → tiles_editor/paint_tool)",
            "paint",
            "Paint")
    {
    }
}

public sealed class TileMapLineCommand : TileMapToolReactiveCommandBase
{
    public TileMapLineCommand()
        : base("TileMap - Line", "TileMap editor: line tool (tiles_editor/line_tool)", "line", "Line")
    {
    }
}

public sealed class TileMapRectCommand : TileMapToolReactiveCommandBase
{
    public TileMapRectCommand()
        : base(
            "TileMap - Rectangle",
            "TileMap editor: rectangle tool (tiles_editor/rect_tool)",
            "rect",
            "Rectangle")
    {
    }
}

public sealed class TileMapBucketCommand : TileMapToolReactiveCommandBase
{
    public TileMapBucketCommand()
        : base(
            "TileMap - Bucket",
            "TileMap editor: bucket / fill tool (tiles_editor/bucket_tool)",
            "bucket",
            "Bucket")
    {
    }
}

public sealed class TileMapPickerCommand : TileMapToolReactiveCommandBase
{
    public TileMapPickerCommand()
        : base("TileMap - Picker", "TileMap editor: pick tile (tiles_editor/picker)", "picker", "Picker")
    {
    }
}

public sealed class TileMapEraserCommand : TileMapToolReactiveCommandBase
{
    public TileMapEraserCommand()
        : base("TileMap - Eraser", "TileMap editor: eraser (tiles_editor/eraser)", "eraser", "Eraser")
    {
    }
}

public sealed class TileMapRandomTileCommand : TileMapToolReactiveCommandBase
{
    public TileMapRandomTileCommand()
        : base(
            "TileMap - Random tile",
            "Toggle «Place random tile» in the TileMap toolbar (no default editor shortcut; uses the editor dice control).",
            "random_tile",
            "Random tile")
    {
    }
}

public sealed class TileMapRotateLeftCommand : TileMapToolReactiveCommandBase
{
    public TileMapRotateLeftCommand()
        : base(
            "TileMap - Rotate left",
            "TileMap editor: rotate tile left (tiles_editor/rotate_tile_left)",
            "rotate_left",
            "Rotate left")
    {
    }
}

public sealed class TileMapRotateRightCommand : TileMapToolReactiveCommandBase
{
    public TileMapRotateRightCommand()
        : base(
            "TileMap - Rotate right",
            "TileMap editor: rotate tile right (tiles_editor/rotate_tile_right)",
            "rotate_right",
            "Rotate right")
    {
    }
}

public sealed class TileMapFlipHorizontalCommand : TileMapToolReactiveCommandBase
{
    public TileMapFlipHorizontalCommand()
        : base(
            "TileMap - Flip horizontal",
            "TileMap editor: flip tile horizontally (tiles_editor/flip_tile_horizontal)",
            "flip_h",
            "Flip H")
    {
    }
}

public sealed class TileMapFlipVerticalCommand : TileMapToolReactiveCommandBase
{
    public TileMapFlipVerticalCommand()
        : base(
            "TileMap - Flip vertical",
            "TileMap editor: flip tile vertically (tiles_editor/flip_tile_vertical)",
            "flip_v",
            "Flip V")
    {
    }
}

public sealed class TileMapPrevLayerCommand : TileMapToolReactiveCommandBase
{
    public TileMapPrevLayerCommand()
        : base(
            "TileMap - Previous layer",
            "TileMap editor: previous layer (tiles_editor/select_previous_layer)",
            "prev_layer",
            "Prev layer")
    {
    }
}

public sealed class TileMapNextLayerCommand : TileMapToolReactiveCommandBase
{
    public TileMapNextLayerCommand()
        : base(
            "TileMap - Next layer",
            "TileMap editor: next layer (tiles_editor/select_next_layer)",
            "next_layer",
            "Next layer")
    {
    }
}
