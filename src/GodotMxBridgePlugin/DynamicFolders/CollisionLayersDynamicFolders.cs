namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder: one button per physics layer bit on <see cref="ContextSnapshot.CollisionLayerBits"/>.
/// </summary>
public sealed class CollisionLayersDynamicFolder : CollisionBitmaskDynamicFolderBase
{
    public CollisionLayersDynamicFolder() : base(maskMode: false, "Collision layers") { }
}

/// <summary>
/// Dynamic folder: one button per bit on <see cref="ContextSnapshot.CollisionMaskBits"/>.
/// </summary>
public sealed class CollisionMaskDynamicFolder : CollisionBitmaskDynamicFolderBase
{
    public CollisionMaskDynamicFolder() : base(maskMode: true, "Collision mask") { }
}

/// <summary>Shared implementation (protected API of <see cref="PluginDynamicFolder"/>).</summary>
public abstract class CollisionBitmaskDynamicFolderBase : PluginDynamicFolder, IGodotContextSubscriber
{
    private const Int32 SlotCount = 32;
    /// <summary>Throttle <see cref="IBridgeTransport.RequestFreshSnapshot"/> during batched <see cref="GetCommandImage"/> calls.</summary>
    private static Int64 s_lastCollisionContextRefreshTicks;
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly Boolean _maskMode;

    protected CollisionBitmaskDynamicFolderBase(Boolean maskMode, String displayTitle)
    {
        _maskMode = maskMode;
        DisplayName = displayTitle;
        GroupName   = "Inspector";
    }

    public override Boolean Load()
    {
        if (Bridge != null) Bridge.ContextChanged += OnContextChanged;
        GodotContextBroadcastService.Subscribe(this);
        return base.Load();
    }

    public override Boolean Unload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        if (Bridge != null) Bridge.ContextChanged -= OnContextChanged;
        return base.Unload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot) =>
        RefreshCollisionFolderSurface();

    private void OnContextChanged()
    {
        Bridge?.RequestFreshSnapshot();
        RefreshCollisionFolderSurface();
    }

    private void RefreshCollisionFolderSurface()
    {
        // Do not call ButtonActionNamesChanged here — slot list is fixed; notifying falsely resets touch pagination to page 1.
        for (var i = 1; i <= SlotCount; i++)
            CommandImageChanged(SlotKey(i));
    }

    private static String SlotKey(Int32 layer1Based) => $"s{layer1Based:00}";

    /// <summary>Bypass short-lived HTTP snapshot cache so layer/mask tiles match Godot when the folder opens or repaints.</summary>
    private static void ThrottledRequestFreshContext()
    {
        var bridge = Bridge;
        if (bridge == null) return;
        var n = Environment.TickCount64;
        var d = unchecked(n - s_lastCollisionContextRefreshTicks);
        if (d is < 0 or > 100)
        {
            s_lastCollisionContextRefreshTicks = n;
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
        for (var i = 1; i <= SlotCount; i++)
            yield return CreateCommandName(SlotKey(i));
    }

    public override void RunCommand(String actionParameter)
    {
        if (!TryParseSlot(actionParameter, out var layer1Based)) return;
        Bridge.RequestFreshSnapshot();
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasCollisionObject) return;
        var id = _maskMode ? EventIds.CollisionToggleMask : EventIds.CollisionToggleLayer;
        Bridge.SendInt(id, layer1Based);
        Bridge.RequestFreshSnapshot();
        CommandImageChanged(SlotKey(layer1Based));
    }

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _)
    {
        if (!TryParseSlot(actionParameter, out var layer1Based)) return null;
        TryReadLiveSnapshot(out var snap);
        if (!snap.HasCollisionObject)
            return $"{FormatLayerTitle(snap, layer1Based)} - select body";
        return FormatLayerTitle(snap, layer1Based);
    }

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
        TryGetSlotIcon(actionParameter, out var img)
            ? img
            : base.GetCommandImage(actionParameter, imageSize);

    public override String GetButtonDisplayName(PluginImageSize _)
    {
        TryReadLiveSnapshot(out var snap);
        if (!snap.HasCollisionObject)
            return _maskMode ? "Collision mask" : "Collision layers";
        var dim = snap.CollisionDimension == CollisionPhysicsDimension.Physics3D ? "3D" : "2D";
        return _maskMode ? $"Mask ({dim})" : $"Layers ({dim})";
    }

    private Boolean TryGetSlotIcon(String actionParameter, out BitmapImage image)
    {
        image = null!;
        if (!TryParseSlot(actionParameter, out var layer1Based)) return false;
        TryReadLiveSnapshot(out var snap);
        var on = snap.HasCollisionObject && IsPhysicsLayerBitSet(
            _maskMode ? snap.CollisionMaskBits : snap.CollisionLayerBits,
            layer1Based);
        image = SvgIcons.GetCollisionBitIcon(on);
        return true;
    }

    /// <summary>Godot layer <paramref name="layer1Based"/> uses bit <c>(layer - 1)</c>; bitmask is uint32.</summary>
    private static Boolean IsPhysicsLayerBitSet(Int32 bitmask, Int32 layer1Based)
    {
        if (layer1Based is < 1 or > SlotCount) return false;
        unchecked
        {
            var u = (UInt32)bitmask;
            return (u & (1u << (layer1Based - 1))) != 0;
        }
    }

    private static Boolean TryParseSlot(String actionParameter, out Int32 layer1Based)
    {
        layer1Based = 0;
        if (String.IsNullOrEmpty(actionParameter)) return false;
        var token = actionParameter;
        var dot = actionParameter.LastIndexOf('.');
        if (dot >= 0 && dot < actionParameter.Length - 1)
            token = actionParameter[(dot + 1)..];
        if (token.Length < 2 || token[0] != 's' && token[0] != 'S') return false;
        if (!Int32.TryParse(token.AsSpan(1), out var n) || n is < 1 or > SlotCount) return false;
        layer1Based = n;
        return true;
    }

    private static String FormatLayerTitle(ContextSnapshot snap, Int32 layer1Based)
    {
        if (layer1Based is < 1 or > SlotCount) return "?";
        var names = snap.CollisionPhysicsLayerNames;
        if (names.Length >= layer1Based)
        {
            var nm = names[layer1Based - 1];
            if (!String.IsNullOrWhiteSpace(nm))
                return nm;
        }

        return layer1Based.ToString();
    }
}
