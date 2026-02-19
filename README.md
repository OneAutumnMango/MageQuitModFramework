# MageQuit Mod Framework

A lightweight modding framework for MageQuit that provides spell modification, module lifecycle management, and UI integration.

## Features

- **Spell modification system** with Base × Mult modifiers
- **Module lifecycle management** for loading/unloading
- **Game data initialization** with event hooks
- **Dynamic mod menu** (F5 to toggle)
- **UI component registry** for custom mod UIs
- **RPC helper utilities** for multiplayer communication
- **NEW!! Debugging mod** to show hitboxes, damage/healing, and unity object creation

## Installation

1. Install [BepinEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.4) into your MageQuit directory.
1. Build the framework: `dotnet build`
1. Copy `MageQuitModFramework.dll` to `MageQuit/BepInEx/plugins/`
1. Build and install mods that depend on the framework

See https://oneautumnmango.github.io/MageQuitModFramework for detailed documentation.


## Structure

- `MageQuitModFramework/` - Core framework DLL
- `ExampleMod/` - Example mod demonstrating framework usage
- `lib/` - Reference assemblies (BepInEx, Unity, game DLLs)

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```
