namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Background polling for the Godot HTTP bridge. The first poll is delayed so Logi Plugin Service
/// can finish loading without blocking on loopback HTTP; after that, interval is aligned with the
/// editor addon’s context refresh so selection / tab / transform changes reach the console quickly.
/// </summary>
internal static class BridgePollSchedule
{
    /// <summary>
    /// Time before the first HTTP poll once <see cref="HttpBridgeTransport.Start"/> runs.
    /// Kept non-zero so the first callback is not in the same burst as transport setup; paired with
    /// <c>GodotMxBridgePlugin</c> deferral, total time-to-first-poll stays ~≤4s.
    /// </summary>
    public const int InitialDelayMs = 2000;

    /// <summary>
    /// Interval between background context / reachability polls. Matches the Godot MX addon
    /// context timer (~350 ms) so switching Node3D / main screen updates dials within ~0.5 s.
    /// </summary>
    public const int PeriodMs = 350;

    /// <summary>
    /// Delay between each <see cref="IGodotContextSubscriber.OnGodotContextSnapshot"/> when a context
    /// snapshot is broadcast, so many Loupedeck surfaces are not repainted in the same timer tick.
    /// </summary>
    public const int ContextSubscriberStaggerMs = 8;
}
