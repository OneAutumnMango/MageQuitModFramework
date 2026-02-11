# MageQuit Mod Framework

A BepInEx plugin framework for creating MageQuit mods with spell modifications, custom modules, and UI integration.

## Architecture

The framework is a **standalone BepInEx plugin** (`MageQuitModFramework.dll`) that provides:
- **Spell modification system** with Base × Mult modifiers
- **Module lifecycle management** for organized mod structure
- **Game data initialization** with event hooks  
- **Dynamic mod menu** (F5) for runtime configuration
- **UI component registry** for custom mod interfaces
- **Helper utilities** for common Harmony patterns

## Creating Mods with the Framework

### 1. Create a BepInEx Plugin

```csharp
using BepInEx;
using HarmonyLib;
using MageQuitModFramework.Modding;
using MageQuitModFramework.UI;

[BepInPlugin("com.yourname.yourmod", "YourMod", "1.0.0")]
[BepInDependency("com.magequit.modframework")]
public class YourModPlugin : BaseUnityPlugin
{
    private Harmony _harmony;

    private void Awake()
    {
        _harmony = new Harmony(Info.Metadata.GUID);
        
        // Register modules
        ModuleManager.RegisterModule(new YourModule());
        ModuleManager.LoadModule("YourModule");
        
        // Register UI (optional)
        ModUIRegistry.RegisterMod(
            "YourMod",
            () => DrawUI()
        );
    }

    private void DrawUI()
    {
        GUILayout.Label("YourMod Settings");
        if (GUILayout.Button("Action"))
        {
            // Your code
        }
    }
}
```

### 2. Create a Module

```csharp
using MageQuitModFramework.Modding;
using HarmonyLib;

public class YourModule : BaseModule
{
    public override string ModuleName => "YourModule";

    protected override void OnLoad(Harmony harmony)
    {
        harmony.PatchAll(typeof(YourPatches));
    }

    protected override void OnUnload(Harmony harmony)
    {
        harmony.UnpatchSelf();
    }
}
```

### 3. Add Framework Reference

In your `.csproj`:
```xml
<Reference Include="MageQuitModFramework">
  <HintPath>path\to\MageQuitModFramework.dll</HintPath>
</Reference>
```

## Framework Features

### Spell Modification System
```csharp
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;

// Wait for game data
GameDataInitializer.OnGameDataLoaded += () =>
{
    // Modify spell attribute
    SpellModificationSystem.TryUpdateModifier(
        SpellName.Fireball,
        "DAMAGE",
        0.5f  // +50% damage
    );

    // Apply to game
    var player = GetLocalPlayer();
    SpellModificationSystem.ApplyModifiersToGame(
        Globals.spell_manager,
        player
    );
};
```

### Game Data Initialization
```csharp
using MageQuitModFramework.Data;

// Check if loaded
if (GameDataInitializer.IsLoaded)
    Initialize();
else
    GameDataInitializer.OnGameDataLoaded += Initialize;
```

### Helper Utilities
```csharp
using MageQuitModFramework.Utilities;

// Modify spell table
GameModificationHelpers.ModifySpellTableEntry(
    Globals.spell_manager,
    SpellName.Fireball,
    spell => spell.cooldown *= 0.5f
);

// Access private fields
GameModificationHelpers.SetPrivateField(
    instance, "fieldName", value
);
var val = GameModificationHelpers.GetPrivateField<float>(
    instance, "fieldName"
);

// IL patching
var modified = GameModificationHelpers.ReplaceFloatConstant(
    instructions, 5.0f, 3.5f
);
```

### UI Registration
```csharp
using MageQuitModFramework.UI;

ModUIRegistry.RegisterMod(
    "MyMod",
    () =>
    {
        GUILayout.Label("Settings");
        if (GUILayout.Button("Action"))
        {
            // Your code
        }
    }
);
```

## Key Concepts

### Module System
Modules provide organized lifecycle management:
- `OnLoad(Harmony)` - Called when module loads
- `OnUnload(Harmony)` - Called when module unloads
- Each module can be loaded/unloaded independently

### Spell Modifiers
Base × Mult pattern for spell attributes:
- `Base` - Original game value (immutable)
- `Mult` - Multiplier (starts at 1.0)
- `Value` - Computed as `Base × Mult`
- Use `AddMultiplier(0.5f)` to add +50% (Mult becomes 1.5)

### Dynamic Mod Menu
- Press **F5** in-game to toggle
- Shows all registered mods
- Each mod can provide custom UI
- Managed by `ModUIRegistry`

## Building

```bash
# Framework
cd MageQuitModFramework
dotnet build
# Output: bin/Debug/net472/MageQuitModFramework.dll

# Your mod
cd YourMod
dotnet build
```

## Installation

1. Copy `MageQuitModFramework.dll` to `BepInEx/plugins/`
2. Copy your mod DLL to `BepInEx/plugins/`
3. Launch game - framework loads automatically via `BepInDependency`

## Documentation

See [src/README.md](src/README.md) for detailed API documentation and examples.

See [src/QUICK_REFERENCE.md](src/QUICK_REFERENCE.md) for quick reference guide.
