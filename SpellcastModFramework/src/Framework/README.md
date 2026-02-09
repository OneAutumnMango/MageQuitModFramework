# Modding Framework

A comprehensive framework for modding and customizing spell behaviors, attributes, and game mechanics.

## Overview

The framework provides a modular, extensible system for:
- **Modifying spell attributes** (damage, radius, cooldown, etc.)
- **Creating upgrade systems** with tiered modifications
- **Building UI components** for player interaction
- **Managing mod lifecycles** with easy loading/unloading
- **Direct game modifications** using helper utilities

## Framework Structure

```
Framework/
├── Core/                       # Core modification systems
│   ├── AttributeModifier.cs    # Base/Mult modifier system
│   ├── SpellModifiers.cs       # Collection of modifiers per spell
│   ├── SpellModificationSystem.cs  # Central spell modification manager
│   └── GameModificationHelpers.cs  # Utilities for patching
├── Modifiers/                  # Upgrade and tier systems
│   ├── TierSystem.cs           # Common/Rare/Legendary tier definitions
│   └── UpgradeSystem.cs        # Upgrade generation and application
├── UI/                         # UI components
│   ├── StyleManager.cs         # Centralized GUIStyle management
│   ├── UpgradeUI.cs           # Upgrade selection UI
│   └── UIComponents.cs        # Reusable UI elements
└── Loading/                    # Module lifecycle management
    ├── ModModule.cs           # Base mod module interface
    ├── ModuleManager.cs       # Module registration and loading
    └── GameDataInitializer.cs # Game data initialization
```

## Quick Start

### 1. Create a New Mod Module

```csharp
using BalancePatch.Framework.Loading;
using HarmonyLib;

public class MyCustomModule : BaseModModule
{
    public override string ModuleName => "MyMod";

    protected override void OnLoad(Harmony harmony)
    {
        // Apply your patches here
        harmony.PatchAll(typeof(MyPatches));
        FrameworkPlugin.Log.LogInfo("MyMod loaded!");
    }

    protected override void OnUnload(Harmony harmony)
    {
        // Clean up
        harmony.UnpatchAll(ModuleName);
    }
}
```

### 2. Modify Spell Attributes

```csharp
using BalancePatch.Framework.Core;

// Initialize the system (usually done automatically)
SpellModificationSystem.Initialize(defaultSpellTable, defaultClassAttributes);

// Modify a spell attribute
SpellModificationSystem.TryUpdateModifier(
    SpellName.Fireball,
    "DAMAGE",
    0.5f  // +50% damage (additive to multiplier)
);

// Apply changes to the game
SpellModificationSystem.ApplyModifiersToGame(
    Globals.spell_manager,
    localPlayer
);
```

### 3. Create Upgrades

```csharp
using BalancePatch.Framework.Modifiers;

// Generate upgrade options for a player
var spells = player.cooldowns.Keys.ToList();
var upgrades = UpgradeSystem.GenerateUpgradeOptions(
    spells,
    count: 3,
    rng: new Random()
);

// Apply an upgrade
foreach (var upgrade in upgrades)
{
    UpgradeSystem.ApplyUpgrade(upgrade, isPositive: true);
}
```

### 4. Use UI Components

```csharp
using BalancePatch.Framework.UI;

// In your OnGUI method
var upgradeUI = new UpgradeUI();
upgradeUI.OnUpgradeSelected += (option, isPositive) =>
{
    UpgradeSystem.ApplyUpgrade(option, isPositive);
};

upgradeUI.SetUpgradeOptions(upgrades);
upgradeUI.Draw();
```

## Core Concepts

### AttributeModifier

The `AttributeModifier` class uses a Base × Mult pattern:
- `Base`: Original value from game data
- `Mult`: Multiplicative modifier (starts at 1.0)
- `Value`: Computed as Base × Mult

```csharp
var modifier = new AttributeModifier(baseValue: 10f, mult: 1.0f);
modifier.AddMultiplier(0.5f);  // Mult becomes 1.5, Value = 15
modifier.ApplyMultiplier(2.0f); // Mult becomes 3.0, Value = 30
```

### SpellModifiers

Contains all modifiable attributes for a spell:
- **Class Attributes**: DAMAGE, RADIUS, POWER, Y_POWER, HEAL
- **Spell Table**: cooldown, windUp, windDown, initialVelocity

```csharp
var spellMods = new SpellModifiers();
if (spellMods.TryGetModifier("DAMAGE", out var damageModifier))
{
    damageModifier.AddMultiplier(0.25f); // +25% damage
}
```

### Tier System

Three built-in tiers for upgrades:
- **Common**: Rate 100%, Up +25%, Down -10%
- **Rare**: Rate 25%, Up +50%, Down -20%
- **Legendary**: Rate 5%, Up +75%, Down -30%

### Module System

Modules encapsulate related patches and can be loaded/unloaded:

```csharp
// Register a module
ModuleManager.RegisterModule(new MyCustomModule());

// Load it
ModuleManager.LoadModule("MyMod");

// Check status
bool isLoaded = ModuleManager.IsModuleLoaded("MyMod");

// Unload it
ModuleManager.UnloadModule("MyMod");
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
public class DamageBoostModule : BaseModModule
{
    public override string ModuleName => "DamageBoost";

    protected override void OnLoad(Harmony harmony)
    {
        // Boost all spell damage by 50%
        foreach (var spell in SpellModificationSystem.SpellModifierTable.Keys)
        {
            SpellModificationSystem.TryUpdateModifier(spell, "DAMAGE", 0.5f);
        }

        // Apply to game
        SpellModificationSystem.ApplyModifiersToGame(
            Globals.spell_manager,
            PlayerManager.players.Values.FirstOrDefault()
        );
    }

    protected override void OnUnload(Harmony harmony)
    {
        SpellModificationSystem.ResetAllMultipliers();
    }
}
```

### Example 2: Custom Upgrade UI

```csharp
public class MyPlugin : BaseUnityPlugin
{
    private UpgradeUI upgradeUI;

    private void Awake()
    {
        upgradeUI = new UpgradeUI
        {
            MaxUpgrades = 3,
            FreeBansPerRound = 1
        };

        upgradeUI.OnUpgradeSelected += (option, isPositive) =>
        {
            UpgradeSystem.ApplyUpgrade(option, isPositive);
            SpellModificationSystem.ApplyModifiersToGame(
                Globals.spell_manager,
                GetLocalPlayer()
            );
        };

        upgradeUI.OnUpgradeBanned += (option) =>
        {
            UpgradeSystem.BannedUpgrades.Add((option.Spell, option.Attribute));
        };
    }

    private void OnGUI()
    {
        upgradeUI.Draw();
    }
}
```

### Example 3: IL Code Patching

```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
class Patch_ChangeValue
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Change brrage count from 5 to 3
        return GameModificationHelpers.ReplaceIntConstant(instructions, 5, 3);
    }
}
```

## Best Practices

1. **Always check if game data is loaded** before modifying spells:
   ```csharp
   if (!GameDataInitializer.IsLoaded)
   {
       throw new InvalidOperationException("Game data not loaded");
   }
   ```

2. **Use the module system** for organized, loadable mods:
   ```csharp
   public class MyMod : BaseModModule { ... }
   ```

3. **Reset state on unload** to prevent conflicts:
   ```csharp
   protected override void OnUnload(Harmony harmony)
   {
       SpellModificationSystem.ResetAllMultipliers();
       harmony.UnpatchAll(ModuleName);
   }
   ```

4. **Use helpers for common patterns**:
   ```csharp
   GameModificationHelpers.ReplaceFloatConstant(...);
   GameModificationHelpers.ModifySpellTableEntry(...);
   ```

5. **Leverage the upgrade system** for balanced modifications:
   ```csharp
   var options = UpgradeSystem.GenerateUpgradeOptions(...);
   ```

## API Reference

### SpellModificationSystem

- `Initialize(defaultSpellTable, defaultClassAttributes)` - Initialize the system
- `TryUpdateModifier(spellName, attribute, additiveMult)` - Modify an attribute
- `TryGetModifier(spellName, attribute, out modifier)` - Get a modifier
- `ApplyModifiersToGame(spellManager, player)` - Apply changes to game
- `ResetAllMultipliers()` - Reset all modifications

### UpgradeSystem

- `GenerateUpgradeOptions(spells, count, rng, filter)` - Generate upgrades
- `ApplyUpgrade(option, isPositive)` - Apply an upgrade
- `IsUpgradeAllowed(spell, attribute, isPrimary)` - Check if upgrade is valid
- `BannedUpgrades` - Set of banned (spell, attribute) pairs
- `ManualRejections` - Dictionary of manual rejections

### ModuleManager

- `Initialize(harmony)` - Initialize the module system
- `RegisterModule(module)` - Register a module
- `LoadModule(moduleName)` - Load a module
- `UnloadModule(moduleName)` - Unload a module
- `IsModuleLoaded(moduleName)` - Check if loaded

## Migration Guide

### From Old Patches to Framework

**Before:**
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
class MyPatch
{
    static void Postfix() { ... }
}

// In Plugin.cs
harmony.PatchAll(typeof(MyPatch));
```

**After:**
```csharp
public class MyModule : BaseModModule
{
    public override string ModuleName => "MyMod";

    protected override void OnLoad(Harmony harmony)
    {
        harmony.PatchAll(typeof(MyPatches));
    }

    protected override void OnUnload(Harmony harmony)
    {
        harmony.UnpatchAll(ModuleName);
    }
}

// In Plugin.cs
ModuleManager.RegisterModule(new MyModule());
ModuleManager.LoadModule("MyMod");
```
