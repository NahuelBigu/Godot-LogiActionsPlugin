using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// HTTP bridge: Godot addon serves <c>GET /context</c> and <c>POST /events</c> on loopback.
/// Same JSON contract as the former file transport.
/// </summary>
internal sealed class HttpBridgeTransport : IBridgeTransport
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new() { WriteIndented = true };
    private readonly object _eventLock = new();
    private readonly object _pollLock = new();

    private readonly HttpClient _http = new(new SocketsHttpHandler
    {
        ConnectTimeout = TimeSpan.FromSeconds(1),
    })
    {
        Timeout = TimeSpan.FromSeconds(2),
    };

    private readonly Uri _contextUri = new($"http://127.0.0.1:{BridgePorts.Http}/context");
    private readonly Uri _eventsUri = new($"http://127.0.0.1:{BridgePorts.Http}/events");

    private System.Threading.Timer? _timer;
    private System.Threading.Timer? _labelNudgeTimer;
    private int _pollGate;
    private string? _lastContextRaw;
    private string? _lastOptionsRaw;
    private Int32? _lastLiveUiFingerprint;
    private bool? _lastEmittedReachable;

    private readonly object _snapshotCacheLock = new();
    private ContextSnapshot _cachedSnapshot = new();
    private Int64 _cachedSnapshotMs;
    private volatile Boolean _cachedSnapshotValid;
    private String? _cachedFullJson;

    /// <summary>Raised when the Godot editor HTTP bridge becomes reachable or stops responding.</summary>
    public event Action<bool>? GodotServiceReachableChanged;

    public event Action? ContextChanged;

    public event Action<ContextSnapshot>? PresentationTargetChanged;

    private Boolean? _presentationHasTransform;
    private String?   _presentationTransformPathKey;

    public void Start()
    {
        if (_timer != null) return;
        // Do not poll at dueTime 0: synchronous HTTP in the plugin Load path blocks Logi Plugin Service.
        _timer = new System.Threading.Timer(
            _ => PollConnectionSafe(),
            null,
            dueTime: BridgePollSchedule.InitialDelayMs,
            period: BridgePollSchedule.PeriodMs);
        _labelNudgeTimer = new System.Threading.Timer(
            _ => LabelNudgeSafe(),
            null,
            dueTime: BridgePollSchedule.InitialDelayMs + 500,
            period: 1000);
    }

    public void Stop()
    {
        _labelNudgeTimer?.Dispose();
        _labelNudgeTimer = null;
        _timer?.Dispose();
        _timer = null;
        Interlocked.Exchange(ref _pollGate, 0);
        _lastContextRaw = null;
        _lastOptionsRaw = null;
        _lastLiveUiFingerprint = null;
        _lastEmittedReachable = null;
        ResetPresentationTargetTracking();
        InvalidateSnapshotCache();
        lock (_snapshotCacheLock) _cachedFullJson = null;
    }

    public void RequestFreshSnapshot() => InvalidateSnapshotCache();

    public bool TryReadSnapshot(out ContextSnapshot snapshot)
    {
        var now = Environment.TickCount64;
        lock (_snapshotCacheLock)
        {
            var age = unchecked(now - _cachedSnapshotMs);
            if (_cachedSnapshotValid && age is >= 0 and < SnapshotCacheTtlMs)
            {
                snapshot = _cachedSnapshot;
                return true;
            }
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, _contextUri);
            using var resp = _http.Send(req, HttpCompletionOption.ResponseHeadersRead);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                snapshot = new ContextSnapshot();
                return false;
            }

            var text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!BridgePayloadParser.TryParseContext(text, out snapshot))
                snapshot = new ContextSnapshot();
            PublishSnapshotCache(snapshot, Environment.TickCount64, text);
            return true;
        }
        catch (Exception ex)
        {
            PluginLog.Warning($"Bridge HTTP: could not read context: {ex.Message}");
            snapshot = new ContextSnapshot();
            return false;
        }
    }

    /// <summary>
    /// Short-lived cache so a single poll can serve many <see cref="ContextChanged"/> handlers
    /// without one loopback GET per dial/folder.
    /// </summary>
    /// <summary>Slightly longer than <see cref="BridgePollSchedule.PeriodMs"/> so bursts of handlers share one GET per poll.</summary>
    private const Int64 SnapshotCacheTtlMs = 500;

    private void PublishSnapshotCache(ContextSnapshot snap, Int64 tickMs, string fullJson)
    {
        lock (_snapshotCacheLock)
        {
            _cachedSnapshot     = snap;
            _cachedSnapshotMs   = tickMs;
            _cachedSnapshotValid = true;
            _cachedFullJson     = fullJson;
        }
    }

    private void InvalidateSnapshotCache()
    {
        lock (_snapshotCacheLock)
        {
            _cachedSnapshotValid = false;
            _cachedFullJson = null;
        }
    }

    /// <summary>
    /// Reads the focused Inspector property from the current context snapshot.
    /// Reuses the snapshot cache — no extra HTTP request unless the cache is stale.
    /// </summary>
    public bool TryReadFocusedProp(out FocusedPropSnapshot prop)
    {
        // Ensure the cache is populated (makes a fresh GET only when TTL has expired).
        TryReadSnapshot(out _);

        lock (_snapshotCacheLock)
        {
            if (_cachedSnapshotValid && _cachedFullJson is { } json)
                return BridgePayloadParser.TryParseFocusedProp(json, out prop);
        }

        prop = new FocusedPropSnapshot();
        return false;
    }

    public void SendTrigger(string eventId) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]   = eventId,
            ["kind"] = EventKind.Trigger,
        });

    public void SendBool(string eventId, bool value) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]    = eventId,
            ["kind"]  = EventKind.SetBool,
            ["value"] = value,
        });

    public void SendFloat(string eventId, double value) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]    = eventId,
            ["kind"]  = EventKind.SetFloat,
            ["value"] = value,
        });

    public void SendRelativeFloat(string eventId, double deltaValue) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]       = eventId,
            ["kind"]     = EventKind.SetFloat,
            ["value"]    = deltaValue,
            ["relative"] = true,
        });

    public void SendInspectorPropStepDelta(int deltaSteps) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]       = EventIds.InspectorProp,
            ["kind"]     = EventKind.SetFloat,
            ["value"]    = deltaSteps,
            ["relative"] = true,
        });

    public void SendInt(string eventId, int value) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]    = eventId,
            ["kind"]  = EventKind.SetInt,
            ["value"] = value,
        });

    public void SendEditorShortcut(string editorShortcutPath) =>
        EnqueueEvent(new JsonObject
        {
            ["id"]   = EventIds.EditorShortcut,
            ["kind"] = EventKind.EditorShortcut,
            ["path"] = editorShortcutPath,
        });

    private void EnqueueEvent(JsonObject evt)
    {
        lock (_eventLock)
        {
            try
            {
                var root = new JsonObject
                {
                    ["schema"] = BridgeSchema.Events,
                    ["events"] = new JsonArray { evt },
                };
                var json = root.ToJsonString(JsonWriteOptions);
                using var req = new HttpRequestMessage(HttpMethod.Post, _eventsUri)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                using var resp = _http.Send(req, HttpCompletionOption.ResponseHeadersRead);
                AppendAudit(evt.ToJsonString());
                if ((int)resp.StatusCode >= 400)
                    PluginLog.Warning($"Bridge HTTP: POST /events returned {(int)resp.StatusCode}");
                else
                    ScheduleContextPollAfterEvent();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Bridge HTTP: failed to POST event: {ex.Message}");
            }
        }
    }

    /// <summary>Picks up editor changes soon after Godot applies POST /events (undo + context refresh).</summary>
    private void ScheduleContextPollAfterEvent()
    {
        _ = Task.Run(() => PollConnectionSafe());
        _ = Task.Run(async () =>
        {
            await Task.Delay(120).ConfigureAwait(false);
            PollConnectionSafe();
        });
    }

    private static void AppendAudit(string payloadOneLine)
    {
        try
        {
            Directory.CreateDirectory(BridgePaths.BridgeDirectory);
            if (File.Exists(BridgePaths.AuditLogPath))
            {
                var fi = new FileInfo(BridgePaths.AuditLogPath);
                if (fi.Length > 2 * 1024 * 1024)
                {
                    var backup = BridgePaths.AuditLogPath + ".bak";
                    if (File.Exists(backup)) File.Delete(backup);
                    File.Move(BridgePaths.AuditLogPath, backup);
                }
            }
            File.AppendAllText(
                BridgePaths.AuditLogPath,
                $"{DateTime.UtcNow:O} {payloadOneLine}{Environment.NewLine}");
        }
        catch { /* ignore audit failures */ }
    }

    /// <summary>Forces dial/touch labels to refresh ~1×/s while Godot is connected.</summary>
    private void LabelNudgeSafe()
    {
        try
        {
            if (_lastEmittedReachable == true)
            {
                // Periodic refresh: cache can otherwise serve a snapshot up to SnapshotCacheTtlMs old,
                // so MX readouts (dynamic folders only get AdjustmentValueChanged(string)) skip HTTP until TTL expires.
                InvalidateSnapshotCache();
                // Fan-out to IGodotContextSubscriber only when logical UI state changed (same fingerprint rule as poll),
                // so subscribers are not woken ~1 Hz with identical snapshots. ContextChanged still nudges wide listeners.
                if (TryReadSnapshot(out var nudgeSnap))
                {
                    var fp = LiveContextFingerprint.Compute(nudgeSnap);
                    var shouldDispatch = false;
                    lock (_pollLock)
                    {
                        shouldDispatch = !_lastLiveUiFingerprint.HasValue || fp != _lastLiveUiFingerprint.Value;
                    }

                    if (shouldDispatch)
                        GodotContextBroadcastService.DispatchSnapshot(nudgeSnap);
                }

                ContextChanged?.Invoke();
            }
        }
        catch
        {
            /* ignore — host may be tearing down */
        }
    }

    /// <summary>Timer callback: runs on the thread pool; skip if the previous poll is still running.</summary>
    private void PollConnectionSafe()
    {
        if (Interlocked.CompareExchange(ref _pollGate, 1, 0) != 0)
            return;
        try
        {
            PollConnection();
        }
        catch (Exception ex)
        {
            PluginLog.Warning($"Bridge HTTP: background poll failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _pollGate, 0);
        }
    }

    private void ResetPresentationTargetTracking()
    {
        _presentationHasTransform    = null;
        _presentationTransformPathKey = null;
    }

    private void PollConnection()
    {
        var shouldNotify             = false;
        var shouldNotifyPresentation = false;
        ContextSnapshot snapOut = new();
        lock (_pollLock)
        {
            if (!TryFetchContextForPoll(out var fullText, out var rawCtx, out var rawOpts))
            {
                UpdateReachable(false);
                _lastLiveUiFingerprint = null;
                ResetPresentationTargetTracking();
                InvalidateSnapshotCache();
                return;
            }

            UpdateReachable(true);

            if (!BridgePayloadParser.TryParseContext(fullText, out snapOut))
                snapOut = new ContextSnapshot();

            PublishSnapshotCache(snapOut, Environment.TickCount64, fullText);

            var fingerprint = LiveContextFingerprint.Compute(snapOut);
            var rawUnchanged     = rawCtx == _lastContextRaw && rawOpts == _lastOptionsRaw;
            var fingerprintMatch = _lastLiveUiFingerprint.HasValue && fingerprint == _lastLiveUiFingerprint.Value;
            if (rawUnchanged && fingerprintMatch)
                return;

            if (BridgePresentation.TransformPresentationIdentityChanged(
                    snapOut,
                    _presentationHasTransform,
                    _presentationTransformPathKey))
            {
                _presentationHasTransform     = snapOut.HasTransformNode;
                _presentationTransformPathKey = BridgePresentation.TransformPresentationPathKey(snapOut);
                shouldNotifyPresentation      = true;
            }

            _lastContextRaw          = rawCtx;
            _lastOptionsRaw          = rawOpts;
            _lastLiveUiFingerprint = fingerprint;
            shouldNotify             = true;
        }

        if (shouldNotifyPresentation)
            PresentationTargetChanged?.Invoke(snapOut);
        if (shouldNotify)
        {
            ContextChanged?.Invoke();
            GodotContextBroadcastService.DispatchSnapshot(snapOut);
        }
    }

    private void UpdateReachable(bool reachable)
    {
        if (_lastEmittedReachable == reachable)
            return;
        _lastEmittedReachable = reachable;
        GodotServiceReachableChanged?.Invoke(reachable);
    }

    private Boolean TryFetchContextForPoll(out String fullText, out String rawCtx, out String rawOpts)
    {
        fullText = "";
        rawCtx = "";
        rawOpts = "[]";
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, _contextUri);
            using var resp = _http.Send(req, HttpCompletionOption.ResponseHeadersRead);
            if (resp.StatusCode != HttpStatusCode.OK)
                return false;

            fullText = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return BridgePayloadParser.TryGetContextAndOptionsRaw(fullText, out rawCtx, out rawOpts);
        }
        catch
        {
            return false;
        }
    }
}
