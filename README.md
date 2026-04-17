# Godot MX Creative Console - Logitech Options+ Plugin

![License](https://img.shields.io/badge/License-MIT-green.svg)
![C#](https://img.shields.io/badge/Language-C%23-blue.svg)

This is the **C# plugin** for Logitech Options+ and Loupedeck (**[Godot-LogiActionsPlugin](https://github.com/NahuelBigu/Godot-LogiActionsPlugin)**). It acts as the bridge client, talking to the Godot editor over loopback HTTP to give you tactile hardware control.

> ⚠️ **Important:** This plugin does nothing on its own! It requires the **[Godot-MXConsoleAddon](https://github.com/NahuelBigu/Godot-MXConsoleAddon)** Godot editor addon to be installed and enabled in your project.

## 📥 Installation & Setup

To use this plugin, you must compile it and let Logitech Options+ discover it.

### Prerequisites
* Windows OS
* Logitech Options+ (or Loupedeck Software)
* .NET SDK 8.0 (or greater)
* A Logitech MX Creative Console or Loupedeck device.

### Build Instructions

1. Clone **[Godot-LogiActionsPlugin](https://github.com/NahuelBigu/Godot-LogiActionsPlugin)** and open the repo root:
   ```bash
   cd Godot-LogiActionsPlugin
   ```
2. Build the project using `dotnet`:
   ```bash
   dotnet build src/GodotMxBridgePlugin.sln -c Release
   ```
3. A `LoupedeckPackage` will be built inside the `bin/` folder. Use the Loupedeck Plugin Tool or manually place the `.zip`/`.lplug4` into your Local AppData Loupedeck plugins directory.

*(For detailed development setup, see the Logitech SDK documentation).*

## 🔌 Reconnecting to Godot
The plugin connects via `localhost` (loopback HTTP) to Godot. If your device displays a warning icon or a "Bridge Disconnected" message, check the following:
* Godot is open.
* The `mx_creative_console` addon is enabled in Godot.
* No firewall is blocking localhost traffic on the designated bridge port.

## 🛠️ Modifying the Plugin
This plugin maps hardware events (Touches, Encoder Rotations) to Godot Bridge commands.
* Dynamic folders handle specific toolbars like `Transform`, `TileMap`, and `Animation`.
* Commands fire REST payloads to Godot. 
If you add a new action, be sure to update `EventIds.cs` in sync with the GDScript addon.
