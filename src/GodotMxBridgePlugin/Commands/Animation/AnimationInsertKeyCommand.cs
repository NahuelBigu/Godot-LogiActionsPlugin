namespace Loupedeck.GodotMxBridge;

/// <summary>Insert a keyframe at the current animation time for the selected track / node.</summary>
public sealed class AnimationInsertKeyCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public AnimationInsertKeyCommand()
        : base("Anim - Insert Key", "Insert a keyframe at the current time", "Animation")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (!Bridge.TryReadSnapshot(out var snap) || !snap.HasAnimation) return;
        Bridge.SendTrigger(EventIds.AnimInsertKey);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        SvgIcons.GetAnimIcon("anim_insert_key");
}
