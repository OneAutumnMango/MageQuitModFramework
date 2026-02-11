# MageQuit Mod Framework

A lightweight modding framework for MageQuit that provides spell modification, module lifecycle management, and UI integration.

## Structure

- `MageQuitModFramework/` - Core framework DLL
- `ExampleMod/` - Example mod demonstrating framework usage
- `lib/` - Reference assemblies (BepInEx, Unity, game DLLs)

## Features

- **Spell modification system** with Base × Mult modifiers
- **Module lifecycle management** for loading/unloading
- **Game data initialization** with event hooks
- **Dynamic mod menu** (F5 to toggle)
- **UI component registry** for custom mod UIs
- **Helper utilities** for Harmony patching

## Building

```bash
dotnet build MageQuitModFramework/MageQuitModFramework.csproj
dotnet build ExampleMod/ExampleMod.csproj
```

## Quick Start

See `MageQuitModFramework/src/README.md` for detailed documentation.

Basic module example:
```csharp
using MageQuitModFramework.Modding;
using HarmonyLib;

public class MyMod : BaseModule
{
    public override string ModuleName => "MyMod";

    protected override void OnLoad(Harmony harmony)
    {
        harmony.PatchAll(typeof(MyPatches));
    }

    protected override void OnUnload(Harmony harmony)
    {
        harmony.UnpatchSelf();
    }
}
```

## Installation

1. Build the framework: `dotnet build MageQuitModFramework/MageQuitModFramework.csproj`
2. Copy `MageQuitModFramework.dll` to `BepInEx/plugins/`
3. Build and install mods that depend on the framework
