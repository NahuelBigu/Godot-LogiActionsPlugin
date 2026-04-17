namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Touch: <c>spatial_editor/focus_selection</c>. Dial: horizontal orbit around the current 3D view pivot
/// (simulated middle-mouse drag on the editor viewport).
/// </summary>
public sealed class FocusOrbitSelectionAdjustment : PluginDynamicAdjustment
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public FocusOrbitSelectionAdjustment()
        : base(
            "Focus & Orbit",
            "Touch: focus selection - Dial: orbit view (3D)",
            "3D View",
            hasReset: false)
    {
        this.DisableLoupedeckLocalization();
    }

    protected override bool OnLoad()
    {
        if (Bridge != null)
            Bridge.ContextChanged += OnContextChanged;
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        if (Bridge != null)
            Bridge.ContextChanged -= OnContextChanged;
        return base.OnUnload();
    }

    private void OnContextChanged() => AdjustmentValueChanged();

    protected override void ApplyAdjustment(string actionParameter, int diff)
    {
        if (diff == 0 || !Bridge.TryReadSnapshot(out var snap) || !EditorMainScreenGuards.Is3DView(snap.MainScreen))
            return;
        Bridge.SendInt(EventIds.View3dOrbitYaw, diff);
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !EditorMainScreenGuards.Is3DView(snap.MainScreen))
            return;
        Bridge.SendEditorShortcut("spatial_editor/focus_selection");
    }

    protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(ActionKeys.TfFocusOrbit);

    protected override string GetAdjustmentValue(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap))
            return "…";
        return EditorMainScreenGuards.Is3DView(snap.MainScreen) ? "3D" : "—";
    }
}
