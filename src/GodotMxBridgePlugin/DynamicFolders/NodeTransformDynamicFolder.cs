namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for Node2D/Node3D transform: toggle visible, latch axis, single encoder row.
/// </summary>
public class NodeTransformDynamicFolder : BridgeDynamicFolder
{
    private const String SelPrefix = "sel_";

    private String? _latchedAxis;

    public NodeTransformDynamicFolder()
    {
        DisplayName = "Transform";
        GroupName   = "Transform";
    }

    public override Boolean Load()
    {
        if (Bridge != null) Bridge.PresentationTargetChanged += OnPresentationTargetChanged;
        NodeTransformAdjustmentTracker.AxisReset += OnTransformAxisReset;
        NodeTransformAdjustmentTracker.ActiveKeyChanged += OnActiveKeyChanged;
        return base.Load(); // subscribes ContextChanged
    }

    public override Boolean Unload()
    {
        NodeTransformAdjustmentTracker.ActiveKeyChanged -= OnActiveKeyChanged;
        NodeTransformAdjustmentTracker.AxisReset -= OnTransformAxisReset;
        if (Bridge != null) Bridge.PresentationTargetChanged -= OnPresentationTargetChanged;
        return base.Unload(); // unsubscribes ContextChanged
    }

    private void OnPresentationTargetChanged(ContextSnapshot _)
    {
        if (_latchedAxis == null) return;
        _latchedAxis = null;
        NotifyLayoutIfChanged();
        CommandImageChanged(ActionKeys.TfVis);
        CommandImageChanged(ActionKeys.TfResetActive);
    }

    private void OnTransformAxisReset(String key) => AdjustmentValueChanged(key);

    private void OnActiveKeyChanged() => CommandImageChanged(ActionKeys.TfResetActive);

    protected override void OnContextChanged()
    {
        if (Bridge.TryReadSnapshot(out var snap))
        {
            NodeTransformAdjustmentTracker.ReconcilePendingWithSnapshot(snap);
            if (_latchedAxis != null
                && (!snap.HasTransformNode || !NodeTransformHelper.AxisApplies(_latchedAxis, snap)))
                _latchedAxis = null;
        }

        // NotifyLayoutIfChanged() is called by base after this method returns.
        CommandImageChanged(ActionKeys.TfVis);
        foreach (var k in NodeTransformHelper.AllKeys)
            AdjustmentValueChanged(k);
        if (_latchedAxis != null)
            AdjustmentValueChanged(_latchedAxis);
    }

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
    {
        yield return CreateCommandName(ActionKeys.TfVis);
        yield return CreateCommandName(ActionKeys.TfResetActive);
        if (Bridge.TryReadSnapshot(out var snap) && snap.HasTransformNode)
        {
            foreach (var k in NodeTransformHelper.AxisKeysFor(snap))
                yield return CreateCommandName($"{SelPrefix}{k}");
        }
        else
        {
            foreach (var k in NodeTransformHelper.AllKeys)
                yield return CreateCommandName($"{SelPrefix}{k}");
        }
    }

    public override void RunCommand(String actionParameter)
    {
        if (actionParameter == ActionKeys.TfResetActive)
        {
            if (Bridge.TryReadSnapshot(out var s0) && !s0.HasTransformNode)
                NodeTransformAdjustmentTracker.Clear();

            var akey = NodeTransformAdjustmentTracker.ActiveKey;
            if (akey == null || !Bridge.TryReadSnapshot(out var sr) || !sr.HasTransformNode
                             || !NodeTransformHelper.AxisApplies(akey, sr)) return;

            switch (akey)
            {
                case ActionKeys.TfPosX:    Bridge.SendFloat(EventIds.TfPosX,  0.0); break;
                case ActionKeys.TfPosY:    Bridge.SendFloat(EventIds.TfPosY,  0.0); break;
                case ActionKeys.TfPosZ:    Bridge.SendFloat(EventIds.TfPosZ,  0.0); break;
                case ActionKeys.TfRotX:    Bridge.SendFloat(EventIds.TfRotX,  0.0); break;
                case ActionKeys.TfRotY:    Bridge.SendFloat(EventIds.TfRotY,  0.0); break;
                case ActionKeys.TfRotZ:    Bridge.SendFloat(EventIds.TfRotZ,  0.0); break;
                case ActionKeys.TfScale: Bridge.SendFloat(EventIds.TfScale, 1.0); break;
            }
            NodeTransformAdjustmentTracker.NotifyResetApplied(akey);
            return;
        }

        if (actionParameter == ActionKeys.TfVis)
        {
            if (Bridge.TryReadSnapshot(out var s) && s.HasTransformNode)
                Bridge.SendBool(EventIds.TfVisible, !s.Visible);
            return;
        }

        if (!actionParameter.StartsWith(SelPrefix, StringComparison.Ordinal)) return;
        var key = actionParameter[SelPrefix.Length..];
        if (!NodeTransformHelper.IsAxisKey(key) || !Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                   || !NodeTransformHelper.AxisApplies(key, snap))
            return;

        if (String.Equals(_latchedAxis, key, StringComparison.Ordinal))
            _latchedAxis = null;
        else
            _latchedAxis = key;

        NotifyLayoutIfChanged();
        if (_latchedAxis != null)
            AdjustmentValueChanged(_latchedAxis);
    }

    public override String? GetCommandDisplayName(String actionParameter, PluginImageSize _) =>
        actionParameter switch
        {
            ActionKeys.TfVis => "Toggle visible",
            ActionKeys.TfResetActive => GetResetActiveLabel(),
            _ when actionParameter.StartsWith(SelPrefix, StringComparison.Ordinal) =>
                AxisSelectLabel(actionParameter[SelPrefix.Length..]),
            _ => null,
        };

    private String GetResetActiveLabel()
    {
        var key = NodeTransformAdjustmentTracker.ActiveKey;
        if (key == null || !Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                        || !NodeTransformHelper.AxisApplies(key, snap))
            return "Reset active dial";
        return $"Reset {NodeTransformHelper.GetDisplayName(key) ?? key}";
    }

    private String AxisSelectLabel(String axisKey)
    {
        var name = NodeTransformHelper.GetDisplayName(axisKey) ?? axisKey;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                   || !NodeTransformHelper.AxisApplies(axisKey, snap))
            return $"{name} (no transform node)";
        return String.Equals(_latchedAxis, axisKey, StringComparison.Ordinal)
            ? $"{name} - dial"
            : $"{name}";
    }

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        if (actionParameter == ActionKeys.TfResetActive)
        {
            bool active = NodeTransformAdjustmentTracker.ActiveKey != null
                          && Bridge.TryReadSnapshot(out var snap)
                          && snap.HasTransformNode
                          && NodeTransformHelper.AxisApplies(NodeTransformAdjustmentTracker.ActiveKey!, snap);
            return SvgIcons.GetTransformResetIcon(active);
        }

        if (actionParameter == ActionKeys.TfVis)
        {
            Bridge.TryReadSnapshot(out var s);
            return SvgIcons.GetReactiveIcon(ActionKeys.TfVis, s);
        }

        if (actionParameter.StartsWith(SelPrefix, StringComparison.Ordinal))
        {
            var k = actionParameter[SelPrefix.Length..];
            if (NodeTransformHelper.IsAxisKey(k))
                return SvgIcons.GetDialIcon(k);
        }

        return base.GetCommandImage(actionParameter, imageSize);
    }

    public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType _)
    {
        if (_latchedAxis == null)
            yield break;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasTransformNode
                                                    || !NodeTransformHelper.AxisApplies(_latchedAxis, snap))
            yield break;
        yield return CreateAdjustmentName(_latchedAxis);
    }

    public override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (_latchedAxis == null
            || !String.Equals(actionParameter, _latchedAxis, StringComparison.Ordinal)
            || !Bridge.TryReadSnapshot(out var s)
            || !s.HasTransformNode
            || !NodeTransformHelper.AxisApplies(actionParameter, s)) return;
        if (diff != 0)
            NodeTransformAdjustmentTracker.SetActive(actionParameter);
        NodeTransformHelper.ApplyDelta(actionParameter, diff, Bridge, s);
        AdjustmentValueChanged(actionParameter);
    }

    public override Boolean ProcessTouchEvent(String actionParameter, DeviceTouchEvent touchEvent)
    {
        if (touchEvent.EventType is DeviceTouchEventType.TouchUp or DeviceTouchEventType.LongRelease
            && NodeTransformHelper.IsAxisKey(actionParameter)
            && String.Equals(NodeTransformAdjustmentTracker.ActiveKey, actionParameter, StringComparison.Ordinal))
            NodeTransformAdjustmentTracker.Clear();
        return base.ProcessTouchEvent(actionParameter, touchEvent);
    }

    public override String? GetAdjustmentDisplayName(String actionParameter, PluginImageSize _) =>
        NodeTransformHelper.GetDisplayName(actionParameter);

    public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetDialIcon(actionParameter);

    public override String? GetAdjustmentValue(String actionParameter)
    {
        if (_latchedAxis == null
            || !String.Equals(actionParameter, _latchedAxis, StringComparison.Ordinal)
            || !Bridge.TryReadSnapshot(out var s)
            || !s.HasTransformNode
            || !NodeTransformHelper.AxisApplies(actionParameter, s)) return null;
        if (NodeTransformAdjustmentTracker.TryGetPendingResetDisplay(actionParameter, s, out var pending))
            return pending;
        return NodeTransformHelper.GetDisplayValue(actionParameter, s);
    }
}
