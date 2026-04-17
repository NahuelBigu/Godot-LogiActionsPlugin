namespace Loupedeck.GodotMxBridge;

/// <summary>Formatting helpers for on-device labels tied to <see cref="IBridgeTransport.PresentationTargetChanged"/>.</summary>
internal static class BridgePresentation
{
    public static String FormatTransformTargetHint(ContextSnapshot snap)
    {
        if (!snap.HasTransformNode)
            return "";
        if (String.IsNullOrEmpty(snap.TransformPath))
            return snap.TransformKind == NodeTransformKind.Node2D ? "Node2D" : "Node3D";
        return GetLastPathSegment(snap.TransformPath);
    }

    public static String TransformPresentationPathKey(ContextSnapshot snap) =>
        snap.HasTransformNode ? $"{(Int32)snap.TransformKind}:{snap.TransformPath ?? ""}" : "";

    public static Boolean TransformPresentationIdentityChanged(
        ContextSnapshot snap,
        Boolean? lastHasTransform,
        String? lastPathKey)
    {
        var key = TransformPresentationPathKey(snap);
        if (!lastHasTransform.HasValue)
            return true;
        return lastHasTransform.Value != snap.HasTransformNode
            || !String.Equals(lastPathKey, key, StringComparison.Ordinal);
    }

    private static String GetLastPathSegment(String path)
    {
        var normalized = path.Replace('\\', '/');
        var idx        = normalized.LastIndexOf('/');
        return idx >= 0 && idx < normalized.Length - 1
            ? normalized[(idx + 1)..]
            : normalized;
    }
}
