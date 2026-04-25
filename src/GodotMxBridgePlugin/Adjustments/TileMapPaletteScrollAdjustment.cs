using System;

namespace Loupedeck.GodotMxBridge;

/// <summary>Standalone encoder: scroll tile/terrain palette (same as the TileMap dynamic folder dial).</summary>
public sealed class TileMapPaletteScrollAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHasTileMap;

    public TileMapPaletteScrollAdjustment()
        : base(
            "TileMap - Palette scroll",
            "Scroll the TileMap tile or terrain palette with the dial",
            "TileMap",
            hasReset: false)
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.OnUnload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snap)
    {
        var has = snap.HasTileMap;
        if (_lastHasTileMap == has) return;
        _lastHasTileMap = has;
        RefreshDialSurface();
    }

    private void RefreshDialSurface()
    {
        ActionImageChanged();
        AdjustmentValueChanged();
    }

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0) return;
        if (!Bridge.TryReadSnapshot(out _))
            PluginLog.Warning("TileMap dial: bridge unreachable (GET /context failed).");
        Bridge.SendFloat(EventIds.TmTileScroll, diff);
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(ActionKeys.TmScroll);

    protected override string GetAdjustmentValue(string actionParameter) => "Scroll";
}
