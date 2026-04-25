namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Optional contract for plugin parts that receive context snapshots from the HTTP bridge when live UI state changes
/// (see <see cref="GodotContextBroadcastService.DispatchSnapshot"/>).
/// </summary>
public interface IGodotContextSubscriber
{
    /// <summary>
    /// Called when <see cref="HttpBridgeTransport"/> detects new logical context: after a poll that changed raw JSON
    /// or <see cref="LiveContextFingerprint"/>, or after the label nudge timer only if the fingerprint changed since
    /// the last poll. Not called on every nudge tick. Deliveries are staggered across subscribers
    /// (<see cref="BridgePollSchedule.ContextSubscriberStaggerMs"/>). Implementations should still diff locally before repainting.
    /// </summary>
    void OnGodotContextSnapshot(ContextSnapshot snapshot);
}
