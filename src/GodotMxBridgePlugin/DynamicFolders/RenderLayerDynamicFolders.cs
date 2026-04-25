namespace Loupedeck.GodotMxBridge;

/// <summary>Per-snapshot binding for one render-layer folder (bitmask, names, bridge event, UI hint).</summary>
internal readonly record struct RenderLayerView(
    Boolean HasTarget,
    Int32 Bits,
    String[] LayerNames,
    String ToggleEventId,
    String ModeLabel);

internal static class RenderLayerViewNone
{
    public static RenderLayerView Value { get; } = new(false, 0, Array.Empty<String>(), "", "");
}

/// <summary>
/// <see cref="CanvasItem.visibility_layer"/> (2D) and <see cref="VisualInstance3D.layers"/> (3D, layers 1–20 per Godot 4.6). Display: “Visibility layer”.
/// Target: first <see cref="VisualInstance3D"/> in selection, else first <see cref="CanvasItem"/> — same order as the Godot provider.
/// </summary>
public sealed class UnifiedRenderLayersDynamicFolder : RenderLayerBitmaskFolderBase
{
    public UnifiedRenderLayersDynamicFolder() : base(
        slotLetter: 'l',
        displayTitle: "Visibility layer",
        idleLabel: "Visibility layer",
        resolveView: static s =>
        {
            if (s.HasVisualInstance3D)
                return new RenderLayerView(
                    true,
                    s.VisualInstanceLayersBits,
                    s.RenderLayerNames3D,
                    EventIds.RenderLayersToggle,
                    "3D");
            if (s.HasCanvasItem)
                return new RenderLayerView(
                    true,
                    s.CanvasVisibilityLayerBits,
                    s.RenderLayerNames2D,
                    EventIds.RenderLayersToggle,
                    "2D");
            return RenderLayerViewNone.Value;
        },
        showDimensionInFolderButton: false)
    {
    }
}

/// <summary>2D <see cref="CanvasItem.light_mask"/> (which <c>Light2D</c> layers affect this item).</summary>
public sealed class CanvasLightMaskDynamicFolder : RenderLayerBitmaskFolderBase
{
    public CanvasLightMaskDynamicFolder() : base(
        slotLetter: 'm',
        displayTitle: "2D light mask",
        idleLabel: "2D light mask",
        resolveView: static s =>
            s.HasCanvasItem
                ? new RenderLayerView(
                    true,
                    s.CanvasLightMaskBits,
                    s.RenderLayerNames2D,
                    EventIds.CanvasLightMaskToggle,
                    "2D")
                : RenderLayerViewNone.Value,
        showDimensionInFolderButton: false)
    {
    }
}

/// <summary>Shared 20-slot render layer UI (bit 0 = layer 1).</summary>
public abstract class RenderLayerBitmaskFolderBase : PluginDynamicFolder, IGodotContextSubscriber
{
    private static Int64 s_lastRenderContextRefreshTicks;
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly Char _slotLetter;
    private readonly String _idleLabel;
    private readonly Boolean _showDimensionInFolderButton;
    private readonly Func<ContextSnapshot, RenderLayerView> _resolveView;
    private Int32 _lastRenderViewSig = Int32.MinValue;

    private protected RenderLayerBitmaskFolderBase(
        Char slotLetter,
        String displayTitle,
        String idleLabel,
        Func<ContextSnapshot, RenderLayerView> resolveView,
        Boolean showDimensionInFolderButton = true)
    {
        _slotLetter                   = slotLetter;
        _idleLabel                    = idleLabel;
        _showDimensionInFolderButton  = showDimensionInFolderButton;
        _resolveView                  = resolveView;
        DisplayName                   = displayTitle;
        GroupName                     = "Inspector";
    }

    public override Boolean Load()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.Load();
    }

    public override Boolean Unload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.Unload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snap)
    {
        var sig = RenderLayerViewSignature(_resolveView(snap));
        if (sig == _lastRenderViewSig) return;
        _lastRenderViewSig = sig;
        RefreshSurface();
    }

    private static Int32 RenderLayerViewSignature(RenderLayerView v)
    {
        var hc = new HashCode();
        hc.Add(v.HasTarget);
        hc.Add(v.Bits);
        hc.Add(v.ModeLabel ?? "");
        hc.Add(v.ToggleEventId ?? "");
        foreach (var n in v.LayerNames)
            hc.Add(n ?? "");
        return hc.ToHashCode();
    }

    private void RefreshSurface()
    {
        // Fixed slot list — only refresh tile images so the user stays on the same touch page (see SDK: ButtonActionNamesChanged = layout changed).
        for (var i = 1; i <= RenderLayerSlotCount.Value; i++)
            CommandImageChanged(SlotKey(i));
    }

    private String SlotKey(Int32 layer1Based) => $"{_slotLetter}{layer1Based:00}";

    private static void ThrottledRequestFreshContext()
    {
        var bridge = Bridge;
        if (bridge == null) return;
        var n = Environment.TickCount64;
        var d = unchecked(n - s_lastRenderContextRefreshTicks);
        if (d is < 0 or > 100)
        {
            s_lastRenderContextRefreshTicks = n;
            bridge.RequestFreshSnapshot();
        }
    }

    private static Boolean TryReadLiveSnapshot(out ContextSnapshot snap)
    {
        ThrottledRequestFreshContext();
        return Bridge.TryReadSnapshot(out snap);
    }

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
    {
        yield return NavigateUpActionName;
        for (var i = 1; i <= RenderLayerSlotCount.Value; i++)
            yield return CreateCommandName(SlotKey(i));
    }

    public override void RunCommand(String actionParameter)
    {
        if (!TryParseSlot(actionParameter, out var layer1Based)) return;
        Bridge.RequestFreshSnapshot();
        if (!Bridge.TryReadSnapshot(out var snap)) return;
        var v = _resolveView(snap);
        if (!v.HasTarget || String.IsNullOrEmpty(v.ToggleEventId)) return;
        Bridge.SendInt(v.ToggleEventId, layer1Based);
        Bridge.RequestFreshSnapshot();
        CommandImageChanged(SlotKey(layer1Based));
    }

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _)
    {
        if (!TryParseSlot(actionParameter, out var layer1Based)) return null;
        TryReadLiveSnapshot(out var snap);
        var v = _resolveView(snap);
        if (!v.HasTarget)
            return $"{FormatLayerTitle(v.LayerNames, layer1Based)} - select node";
        return FormatLayerTitle(v.LayerNames, layer1Based);
    }

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
        TryGetSlotIcon(actionParameter, out var img)
            ? img
            : base.GetCommandImage(actionParameter, imageSize);

    public override String GetButtonDisplayName(PluginImageSize _)
    {
        TryReadLiveSnapshot(out var snap);
        var v = _resolveView(snap);
        if (!_showDimensionInFolderButton || !v.HasTarget)
            return _idleLabel;
        return $"{_idleLabel} ({v.ModeLabel})";
    }

    private Boolean TryGetSlotIcon(String actionParameter, out BitmapImage image)
    {
        image = null!;
        if (!TryParseSlot(actionParameter, out var layer1Based)) return false;
        TryReadLiveSnapshot(out var snap);
        var v = _resolveView(snap);
        var on = v.HasTarget && IsRenderLayerBitSet(v.Bits, layer1Based);
        image = SvgIcons.GetCollisionBitIcon(on);
        return true;
    }

    private static Boolean IsRenderLayerBitSet(Int32 bitmask, Int32 layer1Based)
    {
        if (layer1Based is < 1 or > RenderLayerSlotCount.Value) return false;
        unchecked
        {
            var u = (UInt32)bitmask;
            return (u & (1u << (layer1Based - 1))) != 0;
        }
    }

    private Boolean TryParseSlot(String actionParameter, out Int32 layer1Based)
    {
        layer1Based = 0;
        if (String.IsNullOrEmpty(actionParameter)) return false;
        var token = actionParameter;
        var dot = actionParameter.LastIndexOf('.');
        if (dot >= 0 && dot < actionParameter.Length - 1)
            token = actionParameter[(dot + 1)..];
        if (token.Length != 3 || token[0] != _slotLetter) return false;
        if (!Int32.TryParse(token.AsSpan(1), out var n)
            || n is < 1 or > RenderLayerSlotCount.Value) return false;
        layer1Based = n;
        return true;
    }

    private static String FormatLayerTitle(String[] names, Int32 layer1Based)
    {
        if (layer1Based is < 1 or > RenderLayerSlotCount.Value) return "?";
        if (names.Length >= layer1Based)
        {
            var nm = names[layer1Based - 1];
            if (!String.IsNullOrWhiteSpace(nm))
                return nm;
        }

        return layer1Based.ToString();
    }
}
