# Core Concepts

This section covers the core concepts and systems in the MageQuit Mod Framework.

## Module System

The module system provides structured lifecycle management for your mod components. The framework defines two key types:

### IModule Interface
The base interface for all modules that participate in the framework lifecycle:
- `void Initialize()` - Called first when the mod loads
- `void InitializeAfterGameData()` - Called after game data is loaded and ready
- `bool Enabled { get; }` - Whether this module is active

### BaseModule Class
A convenience base class that implements `IModule` with:
- Automatic initialization tracking
- Default enabled state (true)
- Protected `IsInitialized` property
- Virtual methods for subclass overrides

**Example:**
```csharp
public class MyCustomModule : BaseModule
{
    public override void Initialize()
    {
        base.Initialize();
        // Your initialization logic
    }

    public override void InitializeAfterGameData()
    {
        base.InitializeAfterGameData();
        // Initialize after game data is ready
    }
}
```

## Spell Modification System

The `SpellModificationSystem` provides centralized control over spell behavior modifications. Instead of patching methods directly, register modifiers that transform spell attributes.

### AttributeModifier

The `AttributeModifier` struct uses a Base × Multiplier model:
```csharp
public struct AttributeModifier
{
    public float BaseModifier;      // Additive offset
    public float MultiplierModifier; // Multiplicative scaling

    public float Apply(float original) 
        => (original + BaseModifier) * MultiplierModifier;
}
```

### Modifier Registration

Register modifiers during `InitializeAfterGameData()`:
```csharp
SpellModificationSystem.RegisterModifier(
    spellName: "Fire Bolt",
    attributeName: "projectile_count",
    modifier: new AttributeModifier 
    { 
        BaseModifier = 2f,      // +2 projectiles
        MultiplierModifier = 1.5f // ×1.5 scaling
    }
);
```

The system automatically applies modifiers when spells are cast.

## Game Data Initialization

The `GameDataInitializer` provides event hooks for when game data becomes available:

```csharp
GameDataInitializer.OnInitialized += () =>
{
    // Game data is now loaded and accessible
    // Safe to interact with Elements, SpellObjects, etc.
};
```

This is critical because game data isn't available at plugin load time. Use this event to:
- Register spell modifiers
- Precompute attribute transformations
- Access game configuration data

## UI System

The UI system provides standardized menu components for mods:

### DynamicModMenu
Auto-generated settings menu with type-safe controls for mod configuration.

### ModUIRegistry
Central registry for managing mod UI components. Register your UI during initialization so the framework can properly manage the lifecycle.

### UIComponents
Reusable UI building blocks:
- Sliders with value display
- Toggle buttons
- Text input fields
- Styled panels and layouts

## Helper Utilities

### GameModificationHelpers
Static helpers for common game modifications:
- Accessing game managers
- Element configuration
- Player state queries

## Best Practices

1. **Module Lifecycle:** Always call `base.Initialize()` and `base.InitializeAfterGameData()` when overriding
2. **Game Data Timing:** Use `GameDataInitializer.OnInitialized` for operations that require game data
3. **Spell Modifiers:** Register modifiers during `InitializeAfterGameData()`, not earlier
4. **Error Handling:** Modules should handle their own initialization errors gracefully
5. **Performance:** Modifier application happens during spell casting - keep `Apply()` methods lightweight

## Architecture Overview

```
Plugin (BepInEx) → FrameworkPlugin
    ├── Module Registry
    │   ├── Your Custom Modules
    │   └── Framework Modules
    ├── Spell Modification System
    │   └── Attribute Modifiers
    ├── UI System
    │   └── Mod Menus
    └── Game Data Initializer
        └── Initialization Events
```

The framework handles:
- Module lifecycle orchestration
- Harmony patch management
- Game data readiness detection
- UI registration and rendering
- Modifier application to spells

Your mod focuses on:
- Defining module behavior
- Registering modifiers
- Building UI components
- Implementing game logic
    GUILayout.Label("MyMod Settings");
    
    if (GUILayout.Button("Toggle Feature"))
    {
        // Your action
    }
});
```

### Styles

```csharp
GUILayout.Button("Positive", StyleManager.Green);
GUILayout.Button("Negative", StyleManager.Red);
GUILayout.Label("Common", StyleManager.CommonStyle);
GUILayout.Label("Rare", StyleManager.RareStyle);
GUILayout.Label("Legendary", StyleManager.LegendaryStyle);
```

## Helper Utilities

### IL Patching

```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> MyPatch(IEnumerable<CodeInstruction> instructions)
{
    // Replace float constant
    return GameModificationHelpers.ReplaceFloatConstant(
        instructions, 5.0f, 3.5f
    );
    
    // Replace int constant
    return GameModificationHelpers.ReplaceIntConstant(
        instructions, 5, 3
    );
}
```

### Private Field Access

```csharp
// Get
float damage = GameModificationHelpers.GetPrivateField<float>(
    spellObject, "DAMAGE"
);

// Set
GameModificationHelpers.SetPrivateField(
    spellObject, "DAMAGE", 50f
);
```

### Spell Table Modification

```csharp
// Modify single spell
GameModificationHelpers.ModifySpellTableEntry(
    Globals.spell_manager,
    SpellName.Fireball,
    spell => spell.cooldown = 5.0f
);

// Modify all spells
GameModificationHelpers.ModifyAllSpells(
    Globals.spell_manager,
    spell => spell.cooldown *= 0.8f
);
```

## Best Practices

### Wait for Game Data

Always check if game data is loaded:

```csharp
protected override void OnLoad(Harmony harmony)
{
    if (GameDataInitializer.IsLoaded)
        ApplyPatches(harmony);
    else
        GameDataInitializer.OnGameDataLoaded += () => ApplyPatches(harmony);
}
```

### Clean Up on Unload

```csharp
protected override void OnUnload(Harmony harmony)
{
    // Unpatch
    harmony.UnpatchSelf();
    
    // Reset modifications
    SpellModificationSystem.ResetAllMultipliers();
    
    // Unregister UI
    ModUIRegistry.UnregisterMod(ModuleName);
}
```

### Use Modules for Organization

Create separate modules for distinct features:

```csharp
ModuleManager.RegisterModule(new CombatModule());
ModuleManager.RegisterModule(new DebugModule());
ModuleManager.RegisterModule(new UIModule());
```
