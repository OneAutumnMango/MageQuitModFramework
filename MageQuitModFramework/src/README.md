# MageQuit Mod Framework

A lightweight, modular framework for creating MageQuit mods with spell modifications, custom modules, and UI integration.

## Overview

The framework provides:
- **Spell modification system** with Base × Mult modifiers
- **Module lifecycle management** for loading/unloading mods
- **Game data initialization** with event hooks
- **UI utilities** including dynamic mod menu and style management
- **Helper utilities** for Harmony patching and game modifications

## Framework Structure

```
MageQuitModFramework/
├── Data/
│   └── GameDataInitializer.cs  # Game data loading with events
├── Modding/
│   ├── Module.cs               # Base module interface
│   ├── ModuleManager.cs        # Module lifecycle management
│   └── ModManager.cs           # Deprecated wrapper
├── Spells/
│   ├── AttributeModifier.cs    # Base × Mult modifier system
│   ├── SpellModifiers.cs       # Per-spell modifier collection
│   └── SpellModificationSystem.cs  # Central modification manager
├── UI/
│   ├── DynamicModMenu.cs       # In-game mod menu (F5)
│   ├── ModUIRegistry.cs        # UI component registration
│   ├── StyleManager.cs         # GUIStyle definitions
│   └── UIComponents.cs         # Reusable UI elements
└── Utilities/
    └── GameModificationHelpers.cs  # Harmony helpers
```

## Quick Start

### 1. Create a New Module

```csharp
using MageQuitModFramework.Modding;
using HarmonyLib;

public class MyCustomModule : BaseModule
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

### 2. Modify Spell Attributes

```csharp
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;

// Wait for game data to load
GameDataInitializer.OnGameDataLoaded += () =>
{
    // Modify a spell attribute
    SpellModificationSystem.TryUpdateModifier(
        SpellName.Fireball,
        "DAMAGE",
        0.5f  // +50% damage (additive to multiplier)
    );

    // Apply changes
    var player = GetLocalPlayer();
    SpellModificationSystem.ApplyModifiersToGame(
        Globals.spell_manager,
        player
    );
};
```

### 3. Register UI Components

```csharp
using MageQuitModFramework.UI;

// Register custom UI for your mod
ModUIRegistry.RegisterMod(
    "MyMod",
    () =>
    {
        if (GUILayout.Button("Toggle Feature"))
        {
            // Your action
        }
        GUILayout.Label($"Status: Active");
    }
);
```

## Core Concepts

### AttributeModifier

Base × Mult pattern for attribute modifications:
- `Base`: Original game value (immutable)
- `Mult`: Multiplier (starts at 1.0, modified additively)
- `Value`: Computed as `Base × Mult`

```csharp
var modifier = new AttributeModifier(baseValue: 10f);
modifier.AddMultiplier(0.5f);     // Mult = 1.5, Value = 15
modifier.AddMultiplier(0.25f);    // Mult = 1.75, Value = 17.5
modifier.SetMultiplier(2.0f);     // Mult = 2.0, Value = 20
modifier.ResetMultiplier();       // Mult = 1.0, Value = 10
```

### SpellModifiers

Contains modifiers for each spell attribute:
- **Class Attributes**: DAMAGE, RADIUS, POWER, Y_POWER, HEAL
- **Spell Table**: cooldown, windUp, windDown, initialVelocity

```csharp
if (SpellModificationSystem.TryGetModifier(
    SpellName.Fireball,
    "DAMAGE",
    out var modifier))
{
    modifier.AddMultiplier(0.25f); // +25% damage
}
```

### Module System

Modules provide lifecycle management:

```csharp
// Register and load
ModuleManager.RegisterModule(new MyModule());
ModuleManager.LoadModule("MyModule");

// Check status
bool isLoaded = ModuleManager.IsModuleLoaded("MyModule");

// Unload
ModuleManager.UnloadModule("MyModule");
```

### Game Data Initialization

Hook into game data loading:

```csharp
if (GameDataInitializer.IsLoaded)
{
    // Data ready, proceed
}
else
{
    GameDataInitializer.OnGameDataLoaded += () =>
    {
        // Called once data is loaded
    };
}
```

## Game Modification Helpers

### Replace Constants in IL

```csharp
using BalancePatch.Framework.Core;
using System.Collections.Generic;
using System.Reflection.Emit;

[HarmonyTranspiler]
static IEnumerable<CodeInstruction> MyTranspiler(IEnumerable<CodeInstruction> instructions)
{
    // Replace float constant
    return GameModificationHelpers.ReplaceFloatConstant(
        instructions,
        oldValue: 5.0f,
        newValue: 7.5f
    );

    // Replace int constant
    return GameModificationHelpers.ReplaceIntConstant(
        instructions,
        oldValue: 5,
        newValue: 3
    );
}
```

### Modify Spell Table

```csharp
GameModificationHelpers.ModifySpellTableEntry(
    Globals.spell_manager,
    SpellName.Fireball,
    spell =>
    {
        spell.cooldown = 5f;
        spell.description = "New description";
    }
);
```

### Patch All Spell Objects

```csharp
GameModificationHelpers.PatchAllSpellObjectInit(
    harmony,
    prefixMethod: typeof(MyClass).GetMethod("MyPrefix"),
    postfixMethod: typeof(MyClass).GetMethod("MyPostfix")
);
```

## Examples

### Example 1: Damage Boost Module

```csharp
using MageQuitModFramework.Modding;
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;
using HarmonyLib;

public class DamageBoostModule : BaseModule
{
    public override string ModuleName => "DamageBoost";

    protected override void OnLoad(Harmony harmony)
    {
        if (GameDataInitializer.IsLoaded)
            ApplyBoost();
        else
            GameDataInitializer.OnGameDataLoaded += ApplyBoost;
    }

    private void ApplyBoost()
    {
        foreach (var spell in SpellModificationSystem.SpellModifierTable.Keys)
        {
            SpellModificationSystem.TryUpdateModifier(spell, "DAMAGE", 0.5f);
        }

        var player = GetLocalPlayer();
        SpellModificationSystem.ApplyModifiersToGame(
            Globals.spell_manager,
            player
        );
    }

    protected override void OnUnload(Harmony harmony)
    {
        SpellModificationSystem.ResetAllMultipliers();
    }
}
```

### Example 2: Custom UI Integration

```csharp
using MageQuitModFramework.UI;
using UnityEngine;

public class MyModule : BaseModule
{
    public override string ModuleName => "MyMod";

    protected override void OnLoad(Harmony harmony)
    {
        ModUIRegistry.RegisterMod(
            ModuleName,
            () => DrawUI()
        );
    }

    private void DrawUI()
    {
        GUILayout.Label("My Custom Mod");
        
        if (GUILayout.Button("Toggle Feature"))
        {
            // Your action
        }
    }

    protected override void OnUnload(Harmony harmony)
    {
        ModUIRegistry.UnregisterMod(ModuleName);
    }
}
```

### Example 3: IL Patching

```csharp
using MageQuitModFramework.Utilities;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
class Patch_ChangeValue
{
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return GameModificationHelpers.ReplaceIntConstant(
            instructions, 5, 3
        );
    }
}
```

## Best Practices

1. **Hook into game data loading**:
   ```csharp
   if (GameDataInitializer.IsLoaded)
       Initialize();
   else
       GameDataInitializer.OnGameDataLoaded += Initialize;
   ```

2. **Use the module system**:
   ```csharp
   public class MyMod : BaseModule { ... }
   ```

3. **Clean up on unload**:
   ```csharp
   protected override void OnUnload(Harmony harmony)
   {
       SpellModificationSystem.ResetAllMultipliers();
       ModUIRegistry.UnregisterMod(ModuleName);
       harmony.UnpatchSelf();
   }
   ```

4. **Use helper utilities**:
   ```csharp
   GameModificationHelpers.ReplaceFloatConstant(...);
   GameModificationHelpers.ModifySpellTableEntry(...);
   GameModificationHelpers.SetPrivateField(...);
   ```

5. **Register UI components**:
   ```csharp
   ModUIRegistry.RegisterMod(ModuleName, () => DrawUI());
   ```

## API Reference

### SpellModificationSystem

- `Initialize(defaultSpellTable, defaultClassAttributes)` - Initialize system
- `TryUpdateModifier(spellName, attribute, additiveMult)` - Add to multiplier
- `TryGetModifier(spellName, attribute, out modifier)` - Get modifier
- `ApplyModifiersToGame(spellManager, player)` - Apply to game
- `ResetAllMultipliers()` - Reset all to 1.0
- `GetSpellObjectTypeName(spellName)` - Get type name for spell

### ModuleManager

- `RegisterModule(module)` - Register a module
- `LoadModule(moduleName)` - Load registered module
- `UnloadModule(moduleName)` - Unload module
- `IsModuleLoaded(moduleName)` - Check load status
- `GetAllModules()` - Get all registered modules

### GameDataInitializer

- `IsLoaded` - Check if game data is loaded
- `OnGameDataLoaded` - Event fired when data loads
- `DefaultSpellTable` - Original spell data
- `DefaultClassAttributes` - Original class attributes

### ModUIRegistry

- `RegisterMod(modName, drawAction)` - Register UI
- `UnregisterMod(modName)` - Unregister UI
- `GetAllMods()` - Get all registered UIs

## Spell Name Mappings

Most spells map to `{SpellName}Object`, with exceptions:

```csharp
SpellName.RockBlock    → "StonewallObject"
SpellName.FlameLeash   → "BurningLeashObject"
SpellName.SomerAssault → "SomAssaultObject"
SpellName.Sustain      → "SustainObjectObject"
```

Use `SpellModificationSystem.GetSpellObjectTypeName(spellName)` to get the correct type name.
