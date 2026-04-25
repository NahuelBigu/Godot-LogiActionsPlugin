using System;
using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>Dial: TileMap «Scattering» (random tile — chance of painting nothing). Same as second encoder in <see cref="TileMapDynamicFolder"/>.</summary>
public sealed class TileMapRandomScatterAdjustment : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;
    private Boolean? _lastHas;
    private Double? _lastScatter;

    public TileMapRandomScatterAdjustment()
        : base(
            "TileMap - Scattering",
            "Adjust random-tile scattering in the TileMap toolbar (SpinBox next to Place Random Tile).",
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
        var sc  = has ? Math.Round(snap.TileMapRandomScatter, 5) : (Double?)null;
        if (_lastHas == has && _lastScatter == sc) return;
        _lastHas     = has;
        _lastScatter = sc;
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
            PluginLog.Warning("TileMap scattering dial: bridge unreachable (GET /context failed).");
        Bridge.SendFloat(EventIds.TmRandomScatter, diff);
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(ActionKeys.TmRandomScatter);

    protected override string GetAdjustmentValue(string actionParameter) =>
        Bridge.TryReadSnapshot(out var s) && s.HasTileMap
            ? s.TileMapRandomScatter.ToString("0.###", CultureInfo.InvariantCulture)
            : "…";
}
