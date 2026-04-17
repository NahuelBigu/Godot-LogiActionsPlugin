using System.Collections.Generic;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Tracks which transform dial was last used, and optional optimistic UI after reset (0 / 1)
/// until the HTTP snapshot catches up or the user moves that dial again.
/// </summary>
internal static class NodeTransformAdjustmentTracker
{
    private static readonly object PendingLock = new();
    private static readonly Dictionary<String, Double> PendingResetDisplay = new(StringComparer.OrdinalIgnoreCase);

    private static String? _activeKey;

    public static String? ActiveKey => _activeKey;

    public static event Action? ActiveKeyChanged;

    public static event Action<String>? AxisReset;

    public static void SetActive(String key)
    {
        if (_activeKey == key) return;
        _activeKey = key;
        ActiveKeyChanged?.Invoke();
    }

    public static void Clear()
    {
        if (_activeKey == null) return;
        _activeKey = null;
        ActiveKeyChanged?.Invoke();
    }

    public static void NotifyResetApplied(String key)
    {
        var value = key == ActionKeys.TfScale ? 1.0 : 0.0;
        lock (PendingLock)
            PendingResetDisplay[key] = value;
        AxisReset?.Invoke(key);
    }

    public static void ClearPendingResetForKey(String key)
    {
        lock (PendingLock)
            PendingResetDisplay.Remove(key);
    }

    public static void ReconcilePendingWithSnapshot(ContextSnapshot snap)
    {
        if (!snap.HasTransformNode) return;
        lock (PendingLock)
        {
            var keys = new List<String>(PendingResetDisplay.Keys);
            foreach (var key in keys)
            {
                if (!PendingResetDisplay.TryGetValue(key, out var expected)) continue;
                if (SnapshotMatchesReset(key, snap, expected))
                    PendingResetDisplay.Remove(key);
            }
        }
    }

    public static Double GetAxisScalarForNotification(String key, ContextSnapshot snap)
    {
        lock (PendingLock)
        {
            if (PendingResetDisplay.TryGetValue(key, out var expected))
            {
                if (!snap.HasTransformNode)
                {
                    PendingResetDisplay.Remove(key);
                }
                else if (SnapshotMatchesReset(key, snap, expected))
                {
                    PendingResetDisplay.Remove(key);
                }
                else
                    return expected;
            }
        }

        return NodeTransformHelper.GetScalar(key, snap);
    }

    public static Boolean TryGetPendingResetDisplay(String key, ContextSnapshot snap, out String display)
    {
        display = "";
        Double expected;
        lock (PendingLock)
        {
            if (!PendingResetDisplay.TryGetValue(key, out expected))
                return false;
            if (!snap.HasTransformNode)
            {
                PendingResetDisplay.Remove(key);
                return false;
            }

            if (SnapshotMatchesReset(key, snap, expected))
            {
                PendingResetDisplay.Remove(key);
                return false;
            }
        }

        display = NodeTransformHelper.FormatScalarDisplay(key, expected);
        return true;
    }

    private static Boolean SnapshotMatchesReset(String key, ContextSnapshot snap, Double expected)
    {
        var current = NodeTransformHelper.GetScalar(key, snap);
        return key == ActionKeys.TfRotX || key == ActionKeys.TfRotY || key == ActionKeys.TfRotZ
            ? DegreesMatch(current, expected)
            : Math.Abs(current - expected) < 0.001;
    }

    private static Boolean DegreesMatch(Double current, Double expected, Double eps = 0.5)
    {
        var d = current - expected;
        if (Math.Abs(d) < eps) return true;
        if (Math.Abs(d - 360.0) < eps) return true;
        if (Math.Abs(d + 360.0) < eps) return true;
        return false;
    }
}
