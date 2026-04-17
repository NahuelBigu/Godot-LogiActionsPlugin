namespace Loupedeck.GodotMxBridge;

public class TogglePauseCommand : PluginDynamicCommand
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public TogglePauseCommand() : base("Toggle Pause", "Pause or resume the running scene", "Playback")
    {
        this.DisableLoupedeckLocalization();
    }

    protected override void RunCommand(string actionParameter)
    {
        if (Bridge.TryReadSnapshot(out var snap))
            Bridge.SendBool(EventIds.Pause, !snap.RuntimePaused);
    }

    protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return SvgIcons.GetReactiveIcon("rt_pause", snap);
    }

    protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
    {
        return Bridge.TryReadSnapshot(out var snap) && snap.RuntimePaused ? "Resume" : "Pause";
    }
}
