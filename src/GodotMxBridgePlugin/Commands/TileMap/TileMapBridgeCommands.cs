namespace Loupedeck.GodotMxBridge;

/// <summary>Shared bridge send logic for <see cref="TileMapDynamicFolder"/> and standalone TileMap commands.</summary>
internal static class TileMapBridgeCommands
{
    public static void SendToolTrigger(string actionParameter)
    {
        var eventId = ResolveToolEventId(actionParameter);
        if (eventId == null)
            return;

        var bridge = GodotMxBridgePlugin.Bridge;
        if (bridge == null)
            return;

        if (!bridge.TryReadSnapshot(out var snap))
        {
            PluginLog.Warning(
                "TileMap: bridge unreachable (GET /context failed). Is Godot open with the MX addon enabled?");
            return;
        }

        if (!snap.HasTileMap)
            PluginLog.Info(
                "TileMap: snapshot has no TileMap layer; sending command anyway so Godot can apply it on 2D.");

        bridge.SendTrigger(eventId);
    }

    public static string? ResolveToolEventId(string actionParameter) =>
        actionParameter switch
        {
            "select"       => EventIds.TmSelect,
            "paint"        => EventIds.TmPaint,
            "line"         => EventIds.TmLine,
            "rect"         => EventIds.TmRect,
            "bucket"       => EventIds.TmBucket,
            "picker"       => EventIds.TmPicker,
            "eraser"       => EventIds.TmEraser,
            "random_tile"  => EventIds.TmToggleRandomTile,
            "rotate_left"  => EventIds.TmRotateLeft,
            "rotate_right" => EventIds.TmRotateRight,
            "flip_h"       => EventIds.TmFlipH,
            "flip_v"       => EventIds.TmFlipV,
            "prev_layer"   => EventIds.TmPrevLayer,
            "next_layer"   => EventIds.TmNextLayer,
            _              => null,
        };
}
