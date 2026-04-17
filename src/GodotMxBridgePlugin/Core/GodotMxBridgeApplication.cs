using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Links this Logitech plugin to the Godot editor process.
/// The SDK switches the active profile when Godot gains/loses focus.
/// Recognizes official <c>Godot_v4.x-stable_*</c> (and 3.x) executables plus any process
/// name or path containing <c>Godot</c> (dev builds, patch releases, custom exports).
/// </summary>
public class GodotMxBridgeApplication : ClientApplication
{
    private const String GodotFileDescription = "Godot Engine";
    private static readonly Regex GodotExeNameRegex = new(
        pattern: @"^godot(_v\d+(\.\d+){0,3}(-[a-z0-9]+)?(_[a-z0-9\.\-]+)?)?$",
        options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly ConcurrentDictionary<String, Boolean> ExecutablePathSupportCache =
        new(StringComparer.OrdinalIgnoreCase);

    protected override Boolean IsProcessNameSupported(String processName) =>
        LooksLikeGodotEditorProcess(processName, executablePath: null);

    protected override Boolean IsProcessNameSupported(String processName, String executablePath) =>
        LooksLikeGodotEditorProcess(processName, executablePath);

    private static Boolean LooksLikeGodotEditorProcess(String? processName, String? executablePath)
    {
        if (LooksLikeGodotProcessName(processName))
            return true;

        if (!String.IsNullOrWhiteSpace(executablePath) && IsGodotExecutablePath(executablePath))
            return true;

        return false;
    }

    private static Boolean LooksLikeGodotProcessName(String? processName)
    {
        if (String.IsNullOrWhiteSpace(processName))
            return false;

        if (processName.Contains("Godot", StringComparison.OrdinalIgnoreCase))
            return true;

        return GodotExeNameRegex.IsMatch(processName);
    }

    private static Boolean IsGodotExecutablePath(String executablePath) =>
        ExecutablePathSupportCache.GetOrAdd(executablePath, static path =>
        {
            try
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                if (String.Equals(fileVersionInfo.FileDescription, GodotFileDescription, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch
            {
                // Ignore metadata read failures and fallback to filename/path heuristics.
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            if (LooksLikeGodotProcessName(fileName))
                return true;

            return path.Contains("Godot", StringComparison.OrdinalIgnoreCase);
        });

    protected override String GetProcessName() => "Godot";

    // macOS requires the exact bundle identifier. Official Godot editor uses lowercase 'godot'.
    protected override String GetBundleName() => "org.godotengine.godot";
}
