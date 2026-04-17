using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// One button per animation track (absolute selection → Godot <c>mx.anim.track_select</c>).
/// Replaces the former dial-based track scroll adjustment.
/// </summary>
public sealed class AnimationTracksDynamicFolder : PluginDynamicFolder, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    /// <summary>Cap so the touch grid stays bounded on huge animations.</summary>
    private const int MaxTracks = 128;

    private string[]? _lastTouchActions;

    public AnimationTracksDynamicFolder()
    {
        DisplayName = "Anim - Tracks";
        GroupName   = "Animation";
    }

    public override bool Load()
    {
        GodotContextBroadcastService.Subscribe(this);
        if (Bridge != null) Bridge.ContextChanged += OnContextChanged;
        return base.Load();
    }

    public override bool Unload()
    {
        if (Bridge != null) Bridge.ContextChanged -= OnContextChanged;
        GodotContextBroadcastService.Unsubscribe(this);
        return base.Unload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot _) => RefreshLayout();

    private void OnContextChanged() => RefreshLayout();

    private void RefreshLayout()
    {
        var touch = GetButtonPressActionNames(default(DeviceType)).ToArray();
        if (!DynamicFolderLayoutCache.AreEqual(_lastTouchActions, touch))
        {
            _lastTouchActions = touch;
            ButtonActionNamesChanged();
        }

        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        var n = Math.Min(snap.AnimationTrackCount, MaxTracks);
        for (var i = 0; i < n; i++)
            CommandImageChanged(TrackKey(i));
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _)
    {
        yield return NavigateUpActionName;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation || snap.AnimationTrackCount <= 0)
            yield break;
        var n = Math.Min(snap.AnimationTrackCount, MaxTracks);
        for (var i = 0; i < n; i++)
            yield return CreateCommandName(TrackKey(i));
    }

    public override void RunCommand(string actionParameter)
    {
        if (!TryParseTrackKey(actionParameter, out var idx)) return;
        Bridge.SendInt(EventIds.AnimTrackSelect, idx);
        Bridge.RequestFreshSnapshot();
        CommandImageChanged(TrackKey(idx));
    }

    /// <summary>Folder entry tile (parent page): same pattern as <see cref="SpatialViewsDynamicFolder"/> — load by embedded short name <c>anim_track_scroll</c>, not <c>anim_tracks_folder</c> (no matching <c>.svg</c> in <c>Icons/svg</c>).</summary>
    public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
        SvgIcons.GetAnimationTracksFolderIcon(imageSize);

  

    public override string? GetCommandDisplayName(string actionParameter, PluginImageSize _)
    {
        if (actionParameter == NavigateUpActionName) return "Back";
        if (!TryParseTrackKey(actionParameter, out var idx)) return actionParameter;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return $"#{idx + 1}";
        return FormatTrackLabel(snap, idx);
    }

    public override string GetButtonDisplayName(PluginImageSize _)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation)
            return "Tracks";
        return snap.AnimationTrackCount <= 0
            ? "Tracks (no tracks)"
            : $"Tracks - {snap.AnimationTrackCount}";
    }

    private static string TrackKey(int i) => $"t{i}";

    private static bool TryParseTrackKey(string actionParameter, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(actionParameter) || actionParameter[0] != 't') return false;
        return int.TryParse(
            actionParameter.AsSpan(1),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out index);
    }

    /// <summary>
    /// Full Godot track path as in the snapshot (<c>track_get_path</c> string), no splitting on <c>:</c>.
    /// (A previous two-line split hid the “full” label on one-line UIs.) Prefix <c>✓</c> when selected.
    /// </summary>
    private static string FormatTrackLabel(ContextSnapshot snap, int index)
    {
        var selected = snap.AnimationSelectedTrack == index;
        var check = selected ? "✓ " : "";

        if (index >= 0 && snap.AnimationTrackNames.Length > index && !string.IsNullOrEmpty(snap.AnimationTrackNames[index]))
            return $"{check}{snap.AnimationTrackNames[index]}";

        return $"{check}{index + 1}/{snap.AnimationTrackCount}";
    }
}
