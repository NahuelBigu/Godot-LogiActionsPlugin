namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Shared transform encoder (Node2D / Node3D): context updates from <see cref="GodotContextBroadcastService.DispatchSnapshot"/>.
/// Presentation identity uses <see cref="BridgePresentation.TransformPresentationIdentityChanged"/>.
/// </summary>
public abstract class NodeTransformAxisAdjustmentBase : PluginDynamicAdjustment, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private readonly String _axisKey;
    private String _presentationTargetHint = "";
    private Boolean? _presentationHasTransform;
    private String? _presentationTransformPathKey;

    private String _lastPaintedHint = "\u0001";
    private Boolean _lastPaintedApplicable;
    private Double _lastPaintedScalar;
    private Boolean _hasLastPaintedSurface;

    protected String AxisKey => _axisKey;

    public String PresentationTargetHint => _presentationTargetHint;

    protected NodeTransformAxisAdjustmentBase(String displayName, String description, String axisKey)
        : base(displayName, description, "Transform", hasReset: true)
    {
        _axisKey = axisKey;
        this.DisableLoupedeckLocalization();
    }

    protected abstract void SendAxisReset(IBridgeTransport bridge);

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snapshot) =>
        OnBroadcastContextSnapshot(snapshot);

    protected override bool OnLoad()
    {
        NodeTransformAdjustmentTracker.AxisReset += OnAxisReset;
        if (Bridge != null && Bridge.TryReadSnapshot(out var s))
        {
            HydratePresentation(s);
            _presentationHasTransform     = s.HasTransformNode;
            _presentationTransformPathKey = BridgePresentation.TransformPresentationPathKey(s);
        }

        GodotContextBroadcastService.Subscribe(this);
        return base.OnLoad();
    }

    protected override bool OnUnload()
    {
        GodotContextBroadcastService.Unsubscribe(this);

        NodeTransformAdjustmentTracker.AxisReset -= OnAxisReset;
        return base.OnUnload();
    }

    protected void HydratePresentation(ContextSnapshot snap) =>
        _presentationTargetHint = BridgePresentation.FormatTransformTargetHint(snap);

    private void OnBroadcastContextSnapshot(ContextSnapshot snapshot)
    {
        if (BridgePresentation.TransformPresentationIdentityChanged(
                snapshot, _presentationHasTransform, _presentationTransformPathKey))
        {
            NodeTransformAdjustmentTracker.Clear();
            _presentationHasTransform     = snapshot.HasTransformNode;
            _presentationTransformPathKey = BridgePresentation.TransformPresentationPathKey(snapshot);
        }

        NodeTransformAdjustmentTracker.ReconcilePendingWithSnapshot(snapshot);
        HydratePresentation(snapshot);

        var applicable = snapshot.HasTransformNode && NodeTransformHelper.AxisApplies(_axisKey, snapshot);
        var hint       = _presentationTargetHint ?? "";
        var scalar     = applicable
            ? NodeTransformAdjustmentTracker.GetAxisScalarForNotification(_axisKey, snapshot)
            : 0.0;

        if (_hasLastPaintedSurface
            && hint == _lastPaintedHint
            && applicable == _lastPaintedApplicable
            && (!applicable || NotificationScalarsEqual(_axisKey, scalar, _lastPaintedScalar)))
            return;

        _hasLastPaintedSurface   = true;
        _lastPaintedHint       = hint;
        _lastPaintedApplicable = applicable;
        _lastPaintedScalar     = scalar;

        ActionImageChanged();
        if (applicable)
            AdjustmentValueChanged(scalar);
        else
            AdjustmentValueChanged();
    }

    private static Boolean NotificationScalarsEqual(String axisKey, Double a, Double b)
    {
        if (axisKey is ActionKeys.TfRotX or ActionKeys.TfRotY or ActionKeys.TfRotZ)
        {
            var d = a - b;
            if (Math.Abs(d) < 0.05) return true;
            if (Math.Abs(d - 360.0) < 0.05) return true;
            if (Math.Abs(d + 360.0) < 0.05) return true;
            return false;
        }

        return Math.Abs(a - b) < 0.0001;
    }

    private void OnAxisReset(String key)
    {
        if (key != _axisKey) return;
        _hasLastPaintedSurface = false;
        if (Bridge.TryReadSnapshot(out var snap) && snap.HasTransformNode && NodeTransformHelper.AxisApplies(_axisKey, snap))
            AdjustmentValueChanged(NodeTransformAdjustmentTracker.GetAxisScalarForNotification(_axisKey, snap));
        else
            AdjustmentValueChanged();
    }

    protected override void RunCommand(String actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                   || !NodeTransformHelper.AxisApplies(_axisKey, snap)) return;
        SendAxisReset(Bridge);
        NodeTransformAdjustmentTracker.NotifyResetApplied(_axisKey);
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                   || !NodeTransformHelper.AxisApplies(_axisKey, snap)) return;
        if (diff != 0)
            NodeTransformAdjustmentTracker.SetActive(_axisKey);
        NodeTransformHelper.ApplyEncoderDelta(_axisKey, diff, Bridge, snap);
        AdjustmentValueChanged();
    }

    protected override Boolean ProcessTouchEvent(String actionParameter, DeviceTouchEvent touchEvent)
    {
        if (touchEvent.EventType is DeviceTouchEventType.TouchUp or DeviceTouchEventType.LongRelease
            && String.Equals(NodeTransformAdjustmentTracker.ActiveKey, _axisKey, StringComparison.Ordinal))
            NodeTransformAdjustmentTracker.Clear();
        return base.ProcessTouchEvent(actionParameter, touchEvent);
    }

    protected override String GetAdjustmentValue(String actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                   || !NodeTransformHelper.AxisApplies(_axisKey, snap)) return "—";
        if (NodeTransformAdjustmentTracker.TryGetPendingResetDisplay(_axisKey, snap, out var pending))
            return pending;
        return NodeTransformHelper.GetDisplayValue(_axisKey, snap) ?? "—";
    }

    protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        var axis = NodeTransformHelper.GetDisplayName(_axisKey) ?? _axisKey;
        return String.IsNullOrEmpty(_presentationTargetHint)
            ? axis
            : $"{axis} - {_presentationTargetHint}";
    }

    protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(_axisKey);
}
