# Documentation — Logitech plugin (MX Creative Console)

This repository (**[Godot-LogiActionsPlugin](https://github.com/NahuelBigu/Godot-LogiActionsPlugin)**) contains the **C# plugin for Logi Actions SDK**. It is designed to work alongside the separate Godot editor addon **[Godot-MXConsoleAddon](https://github.com/NahuelBigu/Godot-MXConsoleAddon)**. The two projects are independent; they relate through workflow, process detection, and the HTTP bridge contract (`GET /context`, `POST /events`).

## Contents

| Document | Description |
|----------|-------------|
| [plugin-csharp-logitech.md](plugin-csharp-logitech.md) | Tooling, `LogiPluginTool`, package layout, commands, profiles, `.lplug4` distribution. |
| [godot-integration.md](godot-integration.md) | Linking the plugin to Godot (process name, profiles) and how it pairs with the editor addon. |
| [live-context-observer.md](live-context-observer.md) | Live context bridge: HTTP poll, cache, fingerprints, `ContextChanged` vs `IGodotContextSubscriber`, Godot hash alignment. |

## Official references

- [Logi Actions SDK — C# introduction](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction)
- [Getting started](https://logitech.github.io/actions-sdk-docs/getting-started)
- [Plugin structure](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/plugin-structure)
- [Supported devices (MX Creative Console, Actions Ring, Loupedeck)](https://logitech.github.io/actions-sdk-docs/supported-devices)
- [Distributing the plugin](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/distributing-the-plugin)
- [Tutorial: add a simple command](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/add-a-simple-command)
- [Tutorial: link the plugin to an application](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/link-the-plugin-to-an-application)
- Reference implementation: [Logitech/actions-sdk](https://github.com/Logitech/actions-sdk) (includes `DemoPlugin`)

## Source layout (this repo)

- **`src/GodotMxBridgePlugin/`** — C# Logi Actions plugin (bridge to `%LOCALAPPDATA%\GodotMXCreativeConsole\`).
- Build with **.NET 8 SDK** and **`PluginApi.dll`** from Logi Plugin Service (see link below).
- Prefer **`logiplugintool generate`** for a fresh template; this repo keeps a hand-maintained project when the tool/SDK are not installed on a machine.

## Quick workflow

1. Install **Logi Options+** (or Loupedeck), the **.NET 8 SDK**, and the global **LogiPluginTool** ([introduction](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction)). Ensure **`PluginApi.dll`** is available (often under `C:\Program Files\Logi\LogiPluginService\` on Windows).
2. From the **`Godot-LogiActionsPlugin`** repo root, run **`dotnet build GodotMxBridgePlugin.sln -c Release`** (or build from your IDE).
3. On each successful build, the project writes **`%LOCALAPPDATA%\Logi\LogiPluginService\Plugins\GodotMxBridgePlugin.link`**, pointing at **`bin/Release/`** (layout: `bin\*.dll` + `metadata\LoupedeckPackage.yaml`, same as **`logiplugintool generate`**). That is what makes the plugin show under **Installed Plugins**.
4. If it still does not appear: **Options+ → Settings → Restart Logi Plugin Service** (see [C# introduction](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction)).
5. Optional distribution: **`logiplugintool pack .\bin\Release\ .\GodotMxBridge_0_1_0.lplug4`** then **`logiplugintool install`** on the `.lplug4` file.
6. Test in Options+ → MX Creative Keypad / Dialpad / Actions Ring → **All Actions** → **Installed Plugins** (name: **Godot MX Console Bridge**).

For how this connects to Godot, see [godot-integration.md](godot-integration.md). For live editor context, HTTP polling, and observer APIs, see [live-context-observer.md](live-context-observer.md).
