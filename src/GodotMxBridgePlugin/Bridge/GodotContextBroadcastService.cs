namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Fan-out for <see cref="IGodotContextSubscriber"/>: <see cref="HttpBridgeTransport"/> calls
/// <see cref="DispatchSnapshot"/> only when a poll detects new context (same coalescing as
/// <see cref="IBridgeTransport.ContextChanged"/> from the poll, not from <c>LabelNudge</c>).
/// No extra timer and no cache invalidation here.
/// </summary>
internal static class GodotContextBroadcastService
{
    private static readonly object Gate = new();
    private static readonly List<IGodotContextSubscriber> Subscribers = [];

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
        IGodotContextSubscriber[] copy;
        lock (Gate)
        {
            if (Subscribers.Count == 0) return;
            copy = Subscribers.ToArray();
        }

        foreach (var sub in copy)
        {
            try
            {
                sub.OnGodotContextSnapshot(snapshot);
            }
            catch
            {
                /* subscriber must not take down the hub */
            }
        }
    }
}
