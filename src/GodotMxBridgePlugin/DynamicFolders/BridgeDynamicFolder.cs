namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Base class for dynamic folders that react to Godot context changes.
/// Handles <see cref="IBridgeTransport.ContextChanged"/> subscription/unsubscription
/// and the layout-cache "did the action list change?" pattern shared by all context folders.
/// </summary>
public abstract class BridgeDynamicFolder : PluginDynamicFolder
{
    protected static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private String[]? _lastTouchActions;
    private String[]? _lastEncoderActions;

    public override Boolean Load()
    {
        if (Bridge != null) Bridge.ContextChanged += HandleContextChanged;
        return base.Load();
    }

    public override Boolean Unload()
    {
        if (Bridge != null) Bridge.ContextChanged -= HandleContextChanged;
        return base.Unload();
    }

    // Private — wired to the bridge event. Always updates layout after the subclass hook.
    private void HandleContextChanged()
    {
        OnContextChanged();
        NotifyLayoutIfChanged();
    }

    /// <summary>
    /// Override to refresh command images and adjustment values when the Godot context changes.
    /// <see cref="NotifyLayoutIfChanged"/> is called automatically after this method returns.
    /// </summary>
    protected virtual void OnContextChanged() { }

    /// <summary>
    /// Compares current action-name lists against the cache and notifies the SDK only when
    /// they differ — avoids resetting the touch page on every poll cycle.
    /// </summary>
    protected void NotifyLayoutIfChanged()
    {
        var touch = GetButtonPressActionNames(default(DeviceType)).ToArray();
        if (!DynamicFolderLayoutCache.AreEqual(_lastTouchActions, touch))
        {
            _lastTouchActions = touch;
            ButtonActionNamesChanged();
        }

        var enc = GetEncoderRotateActionNames(default(DeviceType)).ToArray();
        if (!DynamicFolderLayoutCache.AreEqual(_lastEncoderActions, enc))
        {
            _lastEncoderActions = enc;
            EncoderActionNamesChanged();
        }
    }
}
