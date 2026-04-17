namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Dynamic folder for Script editor: New/Save/Run/Cut/Copy/Paste/Find/Help
/// + a nested "Debug" sub-folder button.
/// </summary>
public class ScriptDynamicFolder : PluginDynamicFolder
{
    private static IBridgeTransport Bridge => GodotMxBridgePlugin.Bridge;

    public ScriptDynamicFolder()
    {
        this.DisplayName = "Script";
        this.GroupName = "Script";
    }

    public override IEnumerable<string> GetButtonPressActionNames(DeviceType _) =>
        new[]
        {
            PluginDynamicFolder.NavigateUpActionName,
            this.CreateCommandName(ActionKeys.ScNew),
            this.CreateCommandName(ActionKeys.ScSave),
            this.CreateCommandName(ActionKeys.ScRun),
            this.CreateCommandName(ActionKeys.ScCut),
            this.CreateCommandName(ActionKeys.ScCopy),
            this.CreateCommandName(ActionKeys.ScPaste),
            this.CreateCommandName(ActionKeys.ScFind),
            this.CreateCommandName(ActionKeys.ScHelp),
        };

    // ── Commands ─────────────────────────────────────────────────────────────

    public override void RunCommand(string actionParameter)
    {
        switch (actionParameter)
        {
            case ActionKeys.ScNew:  Bridge.SendTrigger(EventIds.ScNew);        break;
            case ActionKeys.ScSave: Bridge.SendTrigger(EventIds.ScSave);       break;
            case ActionKeys.ScRun:  Bridge.SendTrigger(EventIds.ScRun);        break;
            case ActionKeys.ScCut:  Bridge.SendTrigger(EventIds.ScCut);        break;
            case ActionKeys.ScCopy: Bridge.SendTrigger(EventIds.ScCopy);       break;
            case ActionKeys.ScPaste: Bridge.SendTrigger(EventIds.ScPaste);     break;
            case ActionKeys.ScFind: Bridge.SendTrigger(EventIds.ScFind);       break;
            case ActionKeys.ScHelp: Bridge.SendTrigger(EventIds.ScSearchHelp); break;
        }
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) =>
        actionParameter switch
        {
            ActionKeys.ScNew  => "New Script",
            ActionKeys.ScSave => "Save",
            ActionKeys.ScRun  => "Run",
            ActionKeys.ScCut  => "Cut",
            ActionKeys.ScCopy => "Copy",
            ActionKeys.ScPaste => "Paste",
            ActionKeys.ScFind => "Find",
            ActionKeys.ScHelp => "Help (F1)",
            _             => base.GetCommandDisplayName(actionParameter, imageSize),
        };

    public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
    {
        Bridge.TryReadSnapshot(out var snap);
        return actionParameter switch
        {
            ActionKeys.ScNew => SvgIcons.GetSceneTreeToolbarIcon(ActionKeys.SceneAttachScript, imageSize),
            ActionKeys.ScSave => SvgIcons.GetReactiveIcon(ActionKeys.ScSave, snap),
            ActionKeys.ScRun => SvgIcons.GetReactiveIcon(ActionKeys.ScRun, snap),
            ActionKeys.ScCut => SvgIcons.GetReactiveIcon(ActionKeys.ScCut, snap),
            ActionKeys.ScCopy => SvgIcons.GetReactiveIcon(ActionKeys.ScCopy, snap),
            ActionKeys.ScPaste => SvgIcons.GetReactiveIcon(ActionKeys.ScPaste, snap),
            ActionKeys.ScFind => SvgIcons.GetReactiveIcon(ActionKeys.ScFind, snap),
            ActionKeys.ScHelp => SvgIcons.GetReactiveIcon(ActionKeys.ScHelp, snap),
            _             => base.GetCommandImage(actionParameter, imageSize),
        };
    }

    public override string GetButtonDisplayName(PluginImageSize imageSize)
    {
        if (!Bridge.TryReadSnapshot(out var snap)) return "Script";
        if (string.IsNullOrEmpty(snap.ScriptFileName)) return "Script";
        var name = Path.GetFileName(snap.ScriptFileName);
        return name.Length > 12 ? name[..12] + "…" : name;
    }
}
