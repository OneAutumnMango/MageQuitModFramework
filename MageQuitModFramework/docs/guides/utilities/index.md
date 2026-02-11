# Utilities - Helper Functions

The Utilities namespace provides helper functions for common modding tasks.

## GameModificationHelpers

Collection of utility functions for Harmony patching and game modifications.

### IL Code Patching

#### Replace Float Constants

```csharp
using MageQuitModFramework.Utilities;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
class Patch_ChangeFloat
{
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return GameModificationHelpers.ReplaceFloatConstant(
            instructions,
            oldValue: 4.7f,
            newValue: 3.5f,
            tolerance: 1e-6f  // Optional, default 1e-6f
        );
    }
}
```

#### Replace Int Constants

```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
class Patch_ChangeInt
{
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        // Change barrage count from 5 to 3
        return GameModificationHelpers.ReplaceIntConstant(
            instructions,
            oldValue: 5,
            newValue: 3
        );
    }
}
```

Handles all int opcode variations:
- `Ldc_I4_0` through `Ldc_I4_8`
- `Ldc_I4_S` (sbyte)
- `Ldc_I4` (int)

### Private Field Access

#### Get Private Field

```csharp
using MageQuitModFramework.Utilities;

float damage = GameModificationHelpers.GetPrivateField<float>(
    spellObject,
    "DAMAGE"
);
```

#### Set Private Field

```csharp
GameModificationHelpers.SetPrivateField(
    spellObject,
    "DAMAGE",
    50.0f
);
```

#### Set Multiple Fields

```csharp
var fieldValues = new Dictionary<string, object>
{
    ["DAMAGE"] = 50f,
    ["RADIUS"] = 3.5f,
    ["POWER"] = 10f
};

GameModificationHelpers.SetMultipleFields(spellObject, fieldValues);
```

### Spell Table Modification

#### Modify Single Spell

```csharp
using MageQuitModFramework.Utilities;

GameModificationHelpers.ModifySpellTableEntry(
    Globals.spell_manager,
    SpellName.Fireball,
    spell =>
    {
        spell.cooldown = 5.0f;
        spell.windUp = 0.3f;
        spell.description = "Modified fireball";
    }
);
```

#### Modify All Spells

```csharp
GameModificationHelpers.ModifyAllSpells(
    Globals.spell_manager,
    spell =>
    {
        spell.cooldown *= 0.8f; // Reduce all cooldowns by 20%
    }
);
```

#### Modify Specific Attributes

```csharp
string[] attributes = { "cooldown", "windUp", "windDown" };

GameModificationHelpers.ModifySpellTableValues(
    Globals.spell_manager,
    attributes,
    (spell, fieldName, original) =>
    {
        // Reduce cast times by 30%
        return original * 0.7f;
    }
);
```

### Spell Object Patching

Patch all spell object `Init` methods at once:

```csharp
using MageQuitModFramework.Utilities;
using HarmonyLib;

public class MyModule : BaseModule
{
    protected override void OnLoad(Harmony harmony)
    {
        GameModificationHelpers.PatchAllSpellObjectInit(
            harmony,
            prefixMethod: typeof(MyPatches).GetMethod("InitPrefix"),
            postfixMethod: typeof(MyPatches).GetMethod("InitPostfix")
        );
    }
}

public static class MyPatches
{
    public static void InitPrefix(SpellObject __instance)
    {
        // Called before Init on all spell objects
    }
    
    public static void InitPostfix(SpellObject __instance)
    {
        // Called after Init on all spell objects
    }
}
```

### Apply Field Values

Apply a dictionary of values to a spell object instance:

```csharp
var values = new Dictionary<string, float>
{
    ["DAMAGE"] = 50f,
    ["RADIUS"] = 3.5f,
    ["POWER"] = 10f,
    ["Y_POWER"] = 15f
};

GameModificationHelpers.ApplyFieldValuesToInstance(
    spellObject,
    values
);
```

## Helper Functions

### Get Local Player

```csharp
using MageQuitModFramework.Utilities;

var player = GameModificationHelpers.GetLocalPlayer();
```

### Check Game Data Ready

```csharp
if (GameModificationHelpers.IsGameDataReady())
{
    // Globals.spell_manager and spell_table are available
}
```

## Complete Example: IL Patching

```csharp
using HarmonyLib;
using MageQuitModFramework.Utilities;
using System.Collections.Generic;
using System.Reflection.Emit;

// Change barrage spell to fire 3 projectiles instead of 5
[HarmonyPatch(typeof(BarrageObject), "Init")]
class Patch_BarrageCount
{
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var modified = GameModificationHelpers.ReplaceIntConstant(
            instructions,
            oldValue: 5,
            newValue: 3
        );
        
        FrameworkPlugin.Log.LogInfo("Patched BarrageObject.Init");
        return modified;
    }
}

// Reduce all spell cast times
[HarmonyPatch(typeof(SpellManager), "Awake")]
class Patch_ReduceCastTimes
{
    static void Postfix(SpellManager __instance)
    {
        GameModificationHelpers.ModifySpellTableValues(
            __instance,
            new[] { "windUp", "windDown" },
            (spell, fieldName, original) =>
            {
                float reduced = original * 0.7f; // 30% reduction
                FrameworkPlugin.Log.LogInfo(
                    $"{spell.spellName} {fieldName}: {original:F2} -> {reduced:F2}"
                );
                return reduced;
            }
        );
    }
}
```

## Best Practices

### IL Patching

When using IL patching:
1. **Log your changes** for debugging
2. **Test thoroughly** - IL patches can crash if incorrect
3. **Use specific values** - avoid ambiguous constants
4. **Check game updates** - IL code can change between versions

```csharp
static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions)
{
    var modified = GameModificationHelpers.ReplaceFloatConstant(
        instructions, 4.7f, 3.5f
    );
    
    FrameworkPlugin.Log.LogInfo("Applied float constant patch");
    return modified;
}
```

### Private Field Access

Prefer public APIs when available. Use private field access only when necessary:

```csharp
// Good - use public property if available
spell.cooldown = 5.0f;

// Only when needed - private field
GameModificationHelpers.SetPrivateField(
    spellObject, "INTERNAL_DAMAGE", 50f
);
```

### Spell Table Modifications

Modify spells after `SpellManager.Awake` or use `GameDataInitializer`:

```csharp
[HarmonyPatch(typeof(SpellManager), "Awake")]
[HarmonyPostfix]
static void ModifySpells(SpellManager __instance)
{
    if (!GameModificationHelpers.IsGameDataReady()) return;
    
    GameModificationHelpers.ModifyAllSpells(__instance, spell =>
    {
        // Your modifications
    });
}
```
