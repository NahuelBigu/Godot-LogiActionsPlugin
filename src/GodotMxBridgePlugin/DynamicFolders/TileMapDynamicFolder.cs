using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for TileMap: Paint/Line/Rect/Bucket/Rotate/Flip/Layer nav.
/// Dials: palette scroll and random-tile scattering.
/// </summary>
public class TileMapDynamicFolder : PluginDynamicFolder, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public TileMapDynamicFolder()
    {
        this.DisplayName = "TileMap";
        this.GroupName = "TileMap";
    }

    public override Boolean Load()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null)
            Bridge.ContextChanged += OnContextChanged;
        return base.Load();
    }

    public override Boolean Unload()
    {
        if (Bridge != null)
            Bridge.ContextChanged -= OnContextChanged;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.Unload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot _) => RefreshToolbarImages();

    /// <summary>Same pattern as <see cref="ToggleEmittingCommand.OnBridgeContextChanged"/> (no <c>RequestFreshSnapshot</c>).</summary>
    private void OnContextChanged() => RefreshToolbarImages();

    private void RefreshToolbarImages()
    {
        foreach (var p in new[]
                 {
                     ActionKeys.TmSelect,
                     ActionKeys.TmPaint, ActionKeys.TmLine, ActionKeys.TmRect, ActionKeys.TmBucket, ActionKeys.TmPicker, ActionKeys.TmEraser, ActionKeys.TmRandomTile,
                     ActionKeys.TmRotateLeft, ActionKeys.TmRotateRight, ActionKeys.TmPrevLayer, ActionKeys.TmFlipH, ActionKeys.TmFlipV, ActionKeys.TmNextLayer,
                 })
            CommandImageChanged(p);
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            this.CreateCommandName(ActionKeys.TmSelect),
            this.CreateCommandName(ActionKeys.TmPaint),
            this.CreateCommandName(ActionKeys.TmLine),
            this.CreateCommandName(ActionKeys.TmRect),
            this.CreateCommandName(ActionKeys.TmBucket),
            this.CreateCommandName(ActionKeys.TmPicker),
            this.CreateCommandName(ActionKeys.TmEraser),
            this.CreateCommandName(ActionKeys.TmRandomTile),
            this.CreateCommandName(ActionKeys.TmRotateLeft),
            this.CreateCommandName(ActionKeys.TmRotateRight),
            this.CreateCommandName(ActionKeys.TmPrevLayer),
            this.CreateCommandName(ActionKeys.TmFlipH),
            this.CreateCommandName(ActionKeys.TmFlipV),
            this.CreateCommandName(ActionKeys.TmNextLayer),
        };

    public override IEnumerable<string> GetEncoderRotateActionNames(DeviceType _) =>
        new[]
        {
            this.CreateAdjustmentName(ActionKeys.TmScroll),
            this.CreateAdjustmentName(ActionKeys.TmRandomScatter),
        };

    // ── Commands ─────────────────────────────────────────────────────────────

    public override void RunCommand(string actionParameter)
    {
        TileMapBridgeCommands.SendToolTrigger(actionParameter);
        Bridge?.RequestFreshSnapshot();
        CommandImageChanged(actionParameter);
    }

    // ── Adjustments ──────────────────────────────────────────────────────────

    public override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0) return;
        if (!Bridge.TryReadSnapshot(out _))
            PluginLog.Warning("TileMap dial: bridge unreachable (GET /context failed).");
        switch (actionParameter)
        {
            case ActionKeys.TmScroll:
                Bridge.SendFloat(EventIds.TmTileScroll, diff);
                break;
            case ActionKeys.TmRandomScatter:
                Bridge.SendFloat(EventIds.TmRandomScatter, diff);
                break;
        }
    }

    public override string GetAdjustmentValue(string actionParameter) =>
        actionParameter switch
        {
            ActionKeys.TmScroll    => "Scroll",
            ActionKeys.TmRandomScatter => Bridge.TryReadSnapshot(out var s) && s.HasTileMap
                ? s.TileMapRandomScatter.ToString("0.###", CultureInfo.InvariantCulture)
                : "…",
            _ => "—",
        };

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetTileMapCommandImage(actionParameter, snap, imageSize);
    }

    public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.TmScroll    => SvgIcons.GetDialIcon(ActionKeys.TmScroll),
            ActionKeys.TmRandomScatter => SvgIcons.GetDialIcon(ActionKeys.TmRandomScatter),
            _                => base.GetAdjustmentImage(actionParameter, imageSize),
        };

    // ── Display ──────────────────────────────────────────────────────────────

    public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        if (actionParameter == PluginDynamicFolder.NavigateUpActionName)
            return "Back";

        return actionParameter switch
        {
            ActionKeys.TmSelect       => "Select",
            ActionKeys.TmPaint        => "Paint",
            ActionKeys.TmLine         => "Line",
            ActionKeys.TmRect         => "Rect",
            ActionKeys.TmBucket       => "Bucket",
            ActionKeys.TmPicker       => "Picker",
            ActionKeys.TmEraser       => "Eraser",
            ActionKeys.TmRandomTile   => "Random tile",
            ActionKeys.TmRotateLeft   => "Rotate left",
            ActionKeys.TmRotateRight  => "Rotate right",
            ActionKeys.TmFlipH        => "Flip H",
            ActionKeys.TmFlipV        => "Flip V",
            ActionKeys.TmPrevLayer    => "Prev layer",
            ActionKeys.TmNextLayer    => "Next layer",
            _                         => base.GetCommandDisplayName(actionParameter, imageSize),
        };
    }

    public override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.TmScroll    => "Tile scroll",
            ActionKeys.TmRandomScatter => "Scattering",
            _                => base.GetAdjustmentDisplayName(actionParameter, imageSize),
        };

    public override string GetButtonDisplayName(PluginImageSize imageSize)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTileMap) return "TileMap";
        var tool = String.IsNullOrEmpty(snap.TileMapActiveTool) ? "?" : snap.TileMapActiveTool;
        return $"TileMap L{snap.TileMapCurrentLayer + 1}/{snap.TileMapLayerCount}, {tool}";
    }
}
