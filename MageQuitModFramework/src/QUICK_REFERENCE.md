# Framework Quick Reference

## Common Tasks

### Create a Module

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

### Modify Spell Attributes

```csharp
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;

if (GameDataInitializer.IsLoaded)
{
    SpellModificationSystem.TryUpdateModifier(
        SpellName.Fireball,
        "DAMAGE",
        0.5f  // +50% (additive to multiplier)
    );

    var player = GetLocalPlayer();
    SpellModificationSystem.ApplyModifiersToGame(
        Globals.spell_manager,
        player
    );
}
```

### Hook into Game Data Loading

```csharp
using MageQuitModFramework.Data;

protected override void OnLoad(Harmony harmony)
{
    if (GameDataInitializer.IsLoaded)
        Initialize();
    else
        GameDataInitializer.OnGameDataLoaded += Initialize;
}

private void Initialize()
{
    // Apply modifications
}
```

### Register Custom UI

```csharp
using MageQuitModFramework.UI;

protected override void OnLoad(Harmony harmony)
{
    ModUIRegistry.RegisterMod(
        ModuleName,
        () => DrawUI()
    );
}

private void DrawUI()
{
    GUILayout.Label("My Mod Settings");
    if (GUILayout.Button("Action"))
    {
        // Your code
    }
}
```

### Replace Constants in IL

```csharp
using MageQuitModFramework.Utilities;

[HarmonyTranspiler]
static IEnumerable<CodeInstruction> MyPatch(
    IEnumerable<CodeInstruction> instructions)
{
    // Replace float
    return GameModificationHelpers.ReplaceFloatConstant(
        instructions, 4.7f, 3.5f
    );
    
    // Replace int
    return GameModificationHelpers.ReplaceIntConstant(
        instructions, 5, 3
    );
}
```

### Modify Spell Table

```csharp
using MageQuitModFramework.Utilities;

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

## Available Attributes

### Class Attributes (SpellObject fields)
- `DAMAGE` - Damage dealt
- `RADIUS` - Effect radius  
- `POWER` - Horizontal knockback
- `Y_POWER` - Vertical knockback
- `HEAL` - Heal amount (Frog of Life only)

### Spell Table Attributes
- `cooldown` - Spell cooldown
- `windUp` - Cast time
- `windDown` - Recovery time after cast
- `initialVelocity` - Projectile speed

## Module Lifecycle

```csharp
using MageQuitModFramework.Modding;

// Register module
ModuleManager.RegisterModule(new MyModule());

// Load module
ModuleManager.LoadModule("MyModule");

// Check if loaded
bool isLoaded = ModuleManager.IsModuleLoaded("MyModule");

// Unload module  
ModuleManager.UnloadModule("MyModule");

// Get all modules
var modules = ModuleManager.GetAllModules();
```

## Spell Name Mappings

```csharp
SpellName.RockBlock      → "StonewallObject"
SpellName.FlameLeash     → "BurningLeashObject"
SpellName.SomerAssault   → "SomAssaultObject"
SpellName.Sustain        → "SustainObjectObject"
Others                   → "{SpellName}Object"
```

## Common Patterns

### Get Local Player
```csharp
var player = PlayerManager.players.Values
    .FirstOrDefault(p => p.localPlayerNumber >= 0);
```

### Wait for Game Data
```csharp
using MageQuitModFramework.Data;

if (GameDataInitializer.IsLoaded)
    DoSomething();
else
    GameDataInitializer.OnGameDataLoaded += DoSomething;
```

### Access Private Fields
```csharp
using MageQuitModFramework.Utilities;

// Get
float value = GameModificationHelpers.GetPrivateField<float>(
    instance, "fieldName"
);

// Set
GameModificationHelpers.SetPrivateField(
    instance, "fieldName", 5.0f
);
```

### Patch All Spell Init Methods
```csharp
using MageQuitModFramework.Utilities;

GameModificationHelpers.PatchAllSpellObjectInit(
    harmony,
    prefixMethod: typeof(MyClass).GetMethod("Prefix"),
    postfixMethod: typeof(MyClass).GetMethod("Postfix")
);
```

## Reset Modifications

```csharp
using MageQuitModFramework.Spells;

// Reset all spells
SpellModificationSystem.ResetAllMultipliers();

// Reset specific spell
if (SpellModificationSystem.SpellModifierTable.TryGetValue(
    SpellName.Fireball,
    out var mods))
{
    mods.ResetAllMultipliers();
}

// Reset specific attribute
if (SpellModificationSystem.TryGetModifier(
    SpellName.Fireball,
    "DAMAGE",
    out var modifier))
{
    modifier.ResetMultiplier();
}
```

## Helper Utilities

```csharp
using MageQuitModFramework.Utilities;

// Modify all spells
GameModificationHelpers.ModifyAllSpells(
    Globals.spell_manager,
    spell => spell.cooldown *= 0.8f
);

// Apply fields to instance
var values = new Dictionary<string, float>
{
    ["DAMAGE"] = 50f,
    ["RADIUS"] = 3.5f
};
GameModificationHelpers.ApplyFieldValuesToInstance(
    spellObject, values
);
```
