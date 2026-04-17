namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for debug controls: Step Into, Step Over, Breakpoint toggle, Continue.
/// Active when the game is running (available as sub-folder from Script context).
/// </summary>
public class DebugDynamicFolder : PluginDynamicFolder
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public DebugDynamicFolder()
    {
        this.DisplayName = "Debug";
        this.GroupName = "Script";
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            this.CreateCommandName(ActionKeys.DbgStepInto),
            this.CreateCommandName(ActionKeys.DbgStepOver),
            this.CreateCommandName(ActionKeys.DbgBreakpoint),
            this.CreateCommandName(ActionKeys.DbgContinue),
        };

    // ── Commands ─────────────────────────────────────────────────────────────

    public override void RunCommand(string actionParameter)
    {
        switch (actionParameter)
        {
            case ActionKeys.DbgStepInto:  Bridge.SendTrigger(EventIds.DbgStepInto);   break;
            case ActionKeys.DbgStepOver:  Bridge.SendTrigger(EventIds.DbgStepOver);   break;
            case ActionKeys.DbgBreakpoint: Bridge.SendTrigger(EventIds.DbgBreakpoint); break;
            case ActionKeys.DbgContinue:   Bridge.SendTrigger(EventIds.DbgContinue);   break;
        }
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.DbgStepInto  => "Step Into",
            ActionKeys.DbgStepOver  => "Step Over",
            ActionKeys.DbgBreakpoint => "Breakpoint",
            ActionKeys.DbgContinue   => "Continue",
            _            => base.GetCommandDisplayName(actionParameter, imageSize),
        };

    public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.DbgStepInto  => SvgIcons.GetAnimIcon("anim_forward"),
            ActionKeys.DbgStepOver  => SvgIcons.GetAnimIcon("anim_to_end"),
            ActionKeys.DbgBreakpoint => SvgIcons.GetReactiveIcon(ActionKeys.RtStop, new ContextSnapshot()),
            ActionKeys.DbgContinue   => SvgIcons.GetReactiveIcon(ActionKeys.ScRun, new ContextSnapshot()),
            _            => base.GetCommandImage(actionParameter, imageSize),
        };
}
