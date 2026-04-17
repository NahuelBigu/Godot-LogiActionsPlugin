# C# plugin for Logi Actions SDK (MX Creative Console)

Practical guide based on the [official Logitech documentation](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction). Use it as an implementation checklist for this repository.

## 1. Prerequisites

- **.NET and C#** development experience.
- A .NET-capable IDE (Visual Studio 2022, Rider, VS Code).
- Host app: **Logi Options+** or **Loupedeck** (installs **Logi Plugin Service**, which hosts plugins).
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**.
- Hardware or test flow: [MX Creative Console](https://logitech.github.io/actions-sdk-docs/supported-devices), a screen-based Loupedeck device, or **Actions Ring** (on-screen overlay) for development without a physical console.

## 2. Command-line tooling

Install the global tool once:

```bash
dotnet tool install --global LogiPluginTool
```

Typical capabilities: **generate** projects, **pack** `.lplug4`, **verify** packages, **XLIFF** workflows for localization (see *Plugin Localization* on the SDK site).

## 3. First project and build

```bash
logiplugintool generate MyPlugin
cd MyPluginPlugin
dotnet build
```

The generator creates a folder with a `Plugin` suffix (for example `MyPluginPlugin`).

### Development output (Windows)

After a successful build, a **`.link`** file should appear under the Logi Plugin Service plugins directory. It points at your build output:

`%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\<ProjectName>.link`

That file tells **Logi Plugin Service** where to load the assembly during development. Projects created with **`logiplugintool generate`** add MSBuild targets that create this file and copy **`package/`** into **`metadata/`** next to a **`bin/`** output folder (see `LoupedeckPackage.yaml` **`pluginFolderWin: bin`**). Hand-maintained projects must match that layout and create the `.link` file on build, or the host will never list the plugin. If the plugin does not show in the UI, **Restart Logi Plugin Service** from Options+ settings often resolves it.

On macOS, use the path given in the [C# introduction](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction) for your user account.

## 4. Testing in Options+

1. Open Options+ and the customization view for **Actions Ring**, **MX Creative Keypad**, or **MX Creative Dialpad**.
2. Open **All Actions** and confirm the plugin under **Installed Plugins**.
3. In Loupedeck: use **Show and hide plugins** in the Action Panel as needed.

**If the plugin “does nothing”:** it only runs code when you **trigger an action** (dial, key, Actions Ring, etc.). In Options+, open the device profile, drag an action from **Godot MX Console Bridge** onto a control, then use that control. The plugin sends events to Godot via **loopback HTTP** (`POST /events`) and may also mirror to **`%LOCALAPPDATA%\GodotMXCreativeConsole\events.json`** for debugging; the **Godot editor addon** must be running with the bridge enabled to apply events. Open that folder after a click: you may see `events.json` update and log lines via `PluginLog` (host plugin log / SDK logging). For how live context flows back from Godot, see [live-context-observer.md](live-context-observer.md).

### 4.1 Debugging (breakpoints)

Official overview: [Testing and Debugging the Plugin](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/testing-and-debugging-the-plugin/).

**Visual Studio:** the generated template sets **Start Action → executable** to `LogiPluginService.exe` (Windows: typically `C:\Program Files\Logi\LogiPluginService\LogiPluginService.exe`). To match that without committing a `.csproj.user` file (often gitignored), add a `SdkLayoutProbe`-style block to `GodotMxBridgePlugin.csproj.user`, or use **Debug → Attach to Process** and pick **LogiPluginService**. Switch to **Debug** configuration, build, restart the plugin service (or rebuild so the `.link` output reloads), press **F5** or attach, then trigger an action to hit breakpoints in `RunCommand`.

**VS Code / Cursor:** this repo includes `.vscode/launch.json` with **Attach to Logi Plugin Service** (`LogiPluginService.exe` on Windows). Workflow:

1. `dotnet build GodotMxBridgePlugin.sln -c Debug` (or the VS Code task **Build plugin (Debug)**) so symbols match.
2. Restart **Logi Plugin Service** once so it loads the **Debug** binaries from your `.link` path (`bin\Debug\` after that build).
3. Run **Attach to Logi Plugin Service**, set breakpoints (for example in `GodotBridgeWriter.EnqueueEvent` or a command’s `RunCommand`).
4. Trigger the action from Options+ / device. If breakpoints stay hollow, confirm only one `LogiPluginService.exe` is running and that you attached to it.

## 5. Hot reload (optional)

From the generated project `src` folder:

**Windows**

```powershell
cd MyPluginPlugin\src
dotnet watch build
```

**macOS**

```bash
cd MyPluginPlugin/src/
dotnet watch build
```

More detail: [.NET Hot Reload](https://devblogs.microsoft.com/dotnet/introducing-net-hot-reload/).

## 6. Required code architecture

Per [Plugin structure](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/plugin-structure):

| Class | Base class | Responsibility |
|-------|------------|----------------|
| `{Name}Plugin` | `Plugin` | Plugin-level logic (action registration, lifecycle). |
| `{Name}Application` | `ClientApplication` | Logic tied to the **client application** (for example Godot when it is in the foreground). |

The installable package is made of folders that end up inside the `.lplug4` archive (zip with a fixed layout).

### Required folders

- **`metadata/`**
  - `LoupedeckPackage.yaml` — plugin manifest.
  - `Icon256x256.png` — plugin icon.
  - `DefaultIconTemplate.ict` — optional default icon template.

### Optional folders

- `win` / `mac` — platform binaries.
- `actionicons`, `icontemplates`, `actionsymbols` — action visuals.
- `profiles` — default `.lp5` profiles.
- `localization` — XLIFF files.
- `events` — event definitions (for example haptics-related).

## 7. `LoupedeckPackage.yaml` (summary)

Notable required fields:

- `type: plugin4`
- `name` — unique ID (`[a-zA-Z0-9_-]+`), must **not** end with `Plugin`.
- `displayName`, `version`, `author`
- `license`, `licenseUrl`, `supportPageUrl`
- `pluginFileName` — main DLL.
- `pluginFolderWin` / `pluginFolderMac` when applicable.

Full field list and example: [Plugin configuration file structure](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/plugin-structure#plugin-configuration-file-structure).

## 8. Adding an action (command)

Typical pattern: a class derived from **`PluginDynamicCommand`**, constructor calling `base(displayName, description, groupName)`, and override **`RunCommand`** (and other virtuals as needed).

Walkthrough in the SDK docs: [Add a simple command](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/add-a-simple-command).

UI grouping: use `###` in `groupName` for sub-levels (max three levels), for example `"Godot###Scene###Run"`.

## 9. Linking the plugin to Godot (or another app)

Override **`GetProcessName`** on your `ClientApplication` to return the executable’s process name. For multiple names, use **`GetProcessNames`**. For flexible matching, use **`IsProcessNameSupported`**.

Step-by-step: [Link the plugin to an application](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/link-the-plugin-to-an-application).

With **Adapt to App** enabled in the UI, the device profile can switch when Godot is focused.

This bridge plugin follows the same **`Plugin` flags as the official Demo** (`UsesApplicationApiOnly` / `HasNoApplication` both **true**): actions run whenever you trigger them, so `events.json` is written even if Godot is not focused. `ClientApplication` is still used only for profile switching. If **`HasNoApplication` is false**, the host may run commands only while the linked app is in the foreground, which looks like “nothing happens” and an empty `events.json`.

### 9.1 Dynamic folders (Game / Node3D)

This repo adds **`PluginDynamicFolder`** types (see [Dynamic Folders](https://logitech.github.io/actions-sdk-docs/csharp/plugin-features/implementing-dynamic-folders/)):

- **Godot · Game** — When the game is running: pause, stop, restart, reset time scale, time-scale dial, focus Game tab. When you are on the Game tab but not running: run current/main scene, focus Game tab. Otherwise: open Game tab.
- **Godot · Node3D** — When a `Node3D` is selected: **one encoder row per channel** (uniform scale, position X/Y/Z, rotation X/Y/Z), same interaction as **Time scale** in the Game folder — focus that row, then use the dial; plus **Toggle visible**. If nothing is selected, a placeholder explains to select a `Node3D` in Godot.

Layouts refresh when **`HttpBridgeTransport`** polls **`GET /context`** and the parsed snapshot / fingerprint changes (Godot addon also refreshes editor state on its own timer). Some code still subscribes to **`ContextChanged`**; Node3D encoders use **`IGodotContextSubscriber`** for poll-only updates—see [live-context-observer.md](live-context-observer.md). The addon may still write **`context.json`** for debugging; the plugin does **not** use a separate JSON file transport class.

## 10. Distribution

Pack and verify (adjust paths to your `Release` output):

```bash
logiplugintool pack ./bin/Release/ ./MyPlugin_1_0.lplug4
logiplugintool verify ./MyPlugin_1_0.lplug4
```

Suggested package naming: `pluginName_version.lplug4`.

Marketplace: see [Marketplace approval guidelines](https://logitech.github.io/actions-sdk-docs/marketplace-approval-guidelines/) and the submission flow at [marketplace.logitech.com/contribute](https://marketplace.logitech.com/contribute).

## 11. Further reading

- [C# tutorial index](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/)
- Adjustments, action parameters, vector icons, default profiles, plugin status, local storage — under **Plugin Features** on the SDK site.

For testing and debugging, use the **Testing and Debugging** section from the sidebar under [Plugin development](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction) (internal URLs may change between site revisions).
