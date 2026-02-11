# MageQuit Mod Framework

A modding framework for MageQuit that enables easy spell modifications, upgrades, and custom UI.

## Structure

- `MageQuitModFramework/` - The core framework DLL
- `ExampleMod/` - Example mod showing how to use the framework
- `lib/` - Reference assemblies (BepInEx, Unity, game DLLs)

## Building

```bash
dotnet build MageQuitModFramework/MageQuitModFramework.csproj
dotnet build ExampleMod/ExampleMod.csproj
```

## Usage

See `ExampleMod/ExampleModPlugin.cs` for a complete example of:
- Registering as a BepInEx plugin
- Adding framework dependency
- Registering mod UI
- Using helper functions

## Testing

Build both projects and copy the DLLs to your BepInEx plugins folder to test.
