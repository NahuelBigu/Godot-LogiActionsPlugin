using System.Threading;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Fan-out for <see cref="IGodotContextSubscriber"/>: <see cref="HttpBridgeTransport"/> calls
/// <see cref="DispatchSnapshot"/> only when a poll detects new context (same coalescing as
/// <see cref="IBridgeTransport.ContextChanged"/> from the poll, not from <c>LabelNudge</c>).
/// Subscribers are notified one per timer tick, spaced by <see cref="BridgePollSchedule.ContextSubscriberStaggerMs"/>,
/// so a single snapshot does not synchronously repaint every dial/command in one burst.
/// </summary>
internal static class GodotContextBroadcastService
{
    private static readonly object Gate = new();
    private static readonly List<IGodotContextSubscriber> Subscribers = [];

    private static Timer? _staggerTimer;
    private static Int32 _dispatchWave;
    private static ContextSnapshot? _pendingSnap;
    private static IGodotContextSubscriber[]? _pendingSubs;
    private static Int32 _pendingIdx;

    public static void Subscribe(IGodotContextSubscriber subscriber)
    {
        lock (Gate)
        {
            if (Subscribers.Contains(subscriber)) return;
            Subscribers.Add(subscriber);
        }
    }

    public static void Unsubscribe(IGodotContextSubscriber subscriber)
    {
        lock (Gate)
            Subscribers.Remove(subscriber);
    }

    /// <summary>Invoked from <see cref="HttpBridgeTransport.PollConnection"/> when live UI state changed.</summary>
    public static void DispatchSnapshot(ContextSnapshot snapshot)
    {
        lock (Gate)
        {
            if (Subscribers.Count == 0)
                return;

            _dispatchWave++;
            _pendingSnap = snapshot;
            _pendingSubs = Subscribers.ToArray();
            _pendingIdx = 0;

            if (_staggerTimer == null)
                _staggerTimer = new Timer(StaggerTick, null, 0, Timeout.Infinite);
            else
                _staggerTimer.Change(0, Timeout.Infinite);
        }
    }

    private static void StaggerTick(object? _)
    {
        Int32 capturedWave;
        IGodotContextSubscriber sub;
        ContextSnapshot snap;

        lock (Gate)
        {
            if (_pendingSubs == null || _pendingSnap == null || _pendingIdx >= _pendingSubs.Length)
            {
                _staggerTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            capturedWave = _dispatchWave;
            sub = _pendingSubs[_pendingIdx];
            snap = _pendingSnap;
        }

        lock (Gate)
        {
            if (capturedWave != _dispatchWave)
                return;
        }

        try
        {
            sub.OnGodotContextSnapshot(snap);
        }
        catch
        {
            /* subscriber must not take down the hub */
        }

        lock (Gate)
        {
            if (capturedWave != _dispatchWave)
                return;

            _pendingIdx++;
            if (_pendingIdx >= _pendingSubs.Length)
            {
                _staggerTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            _staggerTimer!.Change(BridgePollSchedule.ContextSubscriberStaggerMs, Timeout.Infinite);
        }
    }
}
