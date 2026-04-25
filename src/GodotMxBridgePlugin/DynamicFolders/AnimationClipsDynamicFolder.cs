using System;
using System.Globalization;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// One button per animation on the active <c>AnimationPlayer</c> (absolute index → Godot <c>mx.anim.clip_select</c>).
/// Same UX pattern as <see cref="AnimationTracksDynamicFolder"/>.
/// </summary>
public sealed class AnimationClipsDynamicFolder : PluginDynamicFolder, IGodotContextSubscriber
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    private const int MaxClips = 128;

    private string[]? _lastTouchActions;
    private Int32 _lastClipFolderSig = Int32.MinValue;

    public AnimationClipsDynamicFolder()
    {
        DisplayName = "Anim - Animation list";
        GroupName   = "Animation";
    }

    public override bool Load()
    {
        GodotContextBroadcastService.Subscribe(this);
        return base.Load();
    }

    public override bool Unload()
    {
        GodotContextBroadcastService.Unsubscribe(this);
        return base.Unload();
    }

    void IGodotContextSubscriber.OnGodotContextSnapshot(ContextSnapshot snap)
    {
        var sig = ClipFolderSignature(snap);
        if (sig == _lastClipFolderSig) return;
        _lastClipFolderSig = sig;
        RefreshLayout();
    }

    private static Int32 ClipFolderSignature(ContextSnapshot s)
    {
        var hc = new HashCode();
        hc.Add(s.HasAnimation);
        hc.Add(s.AnimationName ?? "");
        var n = s.HasAnimation ? Math.Min(s.AnimationClipNames.Length, MaxClips) : 0;
        hc.Add(n);
        for (var i = 0; i < n; i++)
            hc.Add(s.AnimationClipNames[i] ?? "");
        return hc.ToHashCode();
    }

    private void RefreshLayout()
    {
        var touch = GetButtonPressActionNames(default(DeviceType)).ToArray();
        if (!DynamicFolderLayoutCache.AreEqual(_lastTouchActions, touch))
        {
            _lastTouchActions = touch;
            ButtonActionNamesChanged();
        }

        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        var n = Math.Min(snap.AnimationClipNames.Length, MaxClips);
        for (var i = 0; i < n; i++)
            CommandImageChanged(ClipKey(i));
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _)
    {
        yield return NavigateUpActionName;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation || snap.AnimationClipNames.Length <= 0)
            yield break;
        var n = Math.Min(snap.AnimationClipNames.Length, MaxClips);
        for (var i = 0; i < n; i++)
            yield return CreateCommandName(ClipKey(i));
    }

    public override void RunCommand(string actionParameter)
    {
        if (!TryParseClipKey(actionParameter, out var idx)) return;
        Bridge.SendInt(EventIds.AnimClipSelect, idx);
        Bridge.RequestFreshSnapshot();
        CommandImageChanged(ClipKey(idx));
    }

    /// <summary>Folder tile: embedded <c>anim_clip_scroll.svg</c> (see <c>copy-icons.ps1</c>).</summary>
    public override BitmapImage GetButtonImage(PluginImageSize imageSize) =>
        SvgIcons.GetAnimationClipsFolderIcon(imageSize);

    public override string? GetCommandDisplayName(string actionParameter, PluginImageSize _)
    {
        if (actionParameter == NavigateUpActionName) return "Back";
        if (!TryParseClipKey(actionParameter, out var idx)) return actionParameter;
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return $"#{idx + 1}";
        return FormatClipLabel(snap, idx);
    }

    public override string GetButtonDisplayName(PluginImageSize _)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation)
            return "Animation list";
        var n = Math.Min(snap.AnimationClipNames.Length, MaxClips);
        return n <= 0
            ? "Animation list (none)"
            : $"Animation list - {n}";
    }

    private static string ClipKey(int i) => $"c{i}";

    private static bool TryParseClipKey(string actionParameter, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(actionParameter) || actionParameter[0] != 'c') return false;
        return int.TryParse(
            actionParameter.AsSpan(1),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out index);
    }

    private static string FormatClipLabel(ContextSnapshot snap, int index)
    {
        var name = index >= 0 && index < snap.AnimationClipNames.Length
            ? snap.AnimationClipNames[index]
            : "";
        var selected = !string.IsNullOrEmpty(name) && string.Equals(snap.AnimationName, name, StringComparison.Ordinal);
        var check = selected ? "\u2713 " : "";
        if (!string.IsNullOrEmpty(name))
            return $"{check}{name}";
        return $"{check}{index + 1}/{snap.AnimationClipNames.Length}";
    }
}
