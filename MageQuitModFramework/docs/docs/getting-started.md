# Getting Started

This guide will help you create your first mod using the MageQuit Mod Framework.

## Prerequisites

- .NET Framework 4.7.2 SDK
- BepInEx installed in MageQuit
- Basic C# knowledge

## Creating Your First Mod

### 1. Create a New Project

Create a new C# class library targeting .NET Framework 4.7.2:

```bash
dotnet new classlib -n MyFirstMod -f net472
```

### 2. Add Framework Reference

Edit your `.csproj` file to reference the framework:

```xml
<ItemGroup>
  <Reference Include="MageQuitModFramework">
    <HintPath>path\to\MageQuitModFramework.dll</HintPath>
  </Reference>
  <Reference Include="BepInEx">
    <HintPath>path\to\BepInEx.dll</HintPath>
  </Reference>
  <Reference Include="0Harmony">
    <HintPath>path\to\0Harmony.dll</HintPath>
  </Reference>
</ItemGroup>
```

### 3. Create Your Plugin

```csharp
using BepInEx;
using HarmonyLib;
using MageQuitModFramework.Modding;

[BepInPlugin("com.yourname.myfirstmod", "MyFirstMod", "1.0.0")]
[BepInDependency("com.magequit.modframework")]
public class MyFirstModPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        var harmony = new Harmony(Info.Metadata.GUID);
        
        ModuleManager.RegisterModule(new MyModule());
        ModuleManager.LoadModule("MyModule");
        
        Logger.LogInfo("MyFirstMod loaded!");
    }
}
```

### 4. Create a Module

```csharp
using MageQuitModFramework.Modding;
using HarmonyLib;

public class MyModule : BaseModule
{
    public override string ModuleName => "MyModule";

    protected override void OnLoad(Harmony harmony)
    {
        FrameworkPlugin.Log.LogInfo("MyModule loading...");
        harmony.PatchAll(typeof(MyPatches));
    }

    protected override void OnUnload(Harmony harmony)
    {
        FrameworkPlugin.Log.LogInfo("MyModule unloading...");
        harmony.UnpatchSelf();
    }
}
```

### 5. Add Your Patches

```csharp
using HarmonyLib;
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;

public static class MyPatches
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    [HarmonyPostfix]
    static void OnSpellManagerAwake()
    {
        if (!GameDataInitializer.IsLoaded) return;
        
        // Double all spell damage
        foreach (var spell in SpellModificationSystem.SpellModifierTable.Keys)
        {
            SpellModificationSystem.TryUpdateModifier(spell, "DAMAGE", 1.0f);
        }
        
        var player = GetLocalPlayer();
        SpellModificationSystem.ApplyModifiersToGame(
            Globals.spell_manager,
            player
        );
    }
    
    private static Player GetLocalPlayer()
    {
        return PlayerManager.players.Values
            .FirstOrDefault(p => p.localPlayerNumber >= 0);
    }
}
```

### 6. Build and Install

```bash
dotnet build
```

Copy the following to `BepInEx/plugins/`:
- `MageQuitModFramework.dll`
- `MyFirstMod.dll`
