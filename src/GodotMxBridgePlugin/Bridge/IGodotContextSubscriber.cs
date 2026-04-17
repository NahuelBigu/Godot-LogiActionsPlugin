namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Optional contract for plugin parts that receive context snapshots from the HTTP bridge poll path only
/// (see <see cref="GodotContextBroadcastService.DispatchSnapshot"/>).
/// </summary>
public interface IGodotContextSubscriber
{
    /// <summary>Called when <see cref="HttpBridgeTransport"/> finishes a poll that changed live UI state (same gating as poll-driven <see cref="IBridgeTransport.ContextChanged"/>).</summary>
    void OnGodotContextSnapshot(ContextSnapshot snapshot);
}
