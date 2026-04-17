# Integrating the Logitech plugin with Godot

This document explains how the **C# plugin** (this repository) and the **Godot editor addon** ([Godot-MXConsoleAddon](https://github.com/NahuelBigu/Godot-MXConsoleAddon)) fit together. The two repositories are separate; product-level integration usually combines **focus detection** in Options+ with optional **inter-process communication** when hardware actions must reflect the project open in the editor.

## 1. Roles

| Component | Runtime | Role |
|-----------|---------|------|
| **Logi plugin** (C#) | Logi Plugin Service | Actions on MX Creative Console / Actions Ring; can react when Godot is in the foreground. |
| **Godot addon** | Godot editor process | Editor UI (for example main screen), tooling, and project-facing features. |

Godot runs **GDScript/C# in the editor** only inside the editor process. The Logitech plugin is **not** loaded into Godot; it is a separate assembly hosted by Logitech. A clear design therefore spells out:

1. **What the Logitech plugin alone does** (global shortcuts, key injection, OS APIs, and so on).
2. **What the Godot addon alone does** (docks, main screen, project resources).
3. **What needs a bridge** (files, sockets, named pipes, native extensions, and so on) — implement as requirements dictate.

## 2. Linking profiles to Godot (Logi Options+)

In your `ClientApplication` class:

- Implement **`GetProcessName()`** (or `GetProcessNames` / `IsProcessNameSupported`) with the **actual process name** of the Godot executable on each platform.
- On Windows, names often resemble `Godot_v4.x-stable_win64.exe` depending on build; confirm with the OS task or process list.

Official flow: [Link the plugin to an application](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/link-the-plugin-to-an-application).

With **Adapt to App** enabled, the console can show the linked profile when the editor (or a running game using the same executable name) is focused.

## 3. Actions tailored to a Godot workflow

When defining commands (`PluginDynamicCommand` and related types):

- Use **clear names and groups** on the device, for example `Godot###Editor###Run scene`.
- Use **descriptions** that state whether the action targets the OS, the editor, or a running game.
- If an action sends **keyboard shortcuts**, use the SDK APIs (as in the *Toggle Mute* example) and document the expected key map in the addon repository.

Adjustments and parameters follow the general C# tutorials: [C# tutorial](https://logitech.github.io/actions-sdk-docs/csharp/tutorial/).

## 4. Coordinating with the Godot addon

The addon can:

- Provide a **debug console** or **main screen** panel that surfaces project state and documents supported Logi actions.
- Store **configuration** under `user://` or user-approved `res://` files for a future consumer.
- Serve **loopback HTTP** (`GET /context`, `POST /events`) so the Logitech plugin reads live editor state without relying on a C# file-transport.

The C# plugin can:

- Use **`HttpBridgeTransport`** as the primary path: poll **`/context`**, send events to **`/events`** (see [live-context-observer.md](live-context-observer.md)).
- Still use the same **`%LOCALAPPDATA%\GodotMXCreativeConsole\`** folder where the addon writes **`events.json`** / **`context.json`** for debugging or tooling; keep file/HTTP contracts aligned when you change payloads.

Without an explicit bridge, integration still adds value through **profiles and shortcuts** while Godot is focused.

## 5. Versioning across projects

- Keep **`.lplug4` version** and **addon changelog** aligned when shortcuts or file/IPC contracts change.
- Document the **minimum recommended Logitech plugin version** in the addon’s README.

## 6. Links

- This Logi Actions plugin (source): [Godot-LogiActionsPlugin](https://github.com/NahuelBigu/Godot-LogiActionsPlugin)
- Live context, polling, and observer APIs (this repo): [live-context-observer.md](live-context-observer.md)
- Companion Godot addon: [Godot-MXConsoleAddon](https://github.com/NahuelBigu/Godot-MXConsoleAddon) — [docs](https://github.com/NahuelBigu/Godot-MXConsoleAddon/tree/main/docs)
- Devices: [Supported devices](https://logitech.github.io/actions-sdk-docs/supported-devices)
