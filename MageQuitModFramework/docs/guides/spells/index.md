# Spells - Spell Modification System

The Spells namespace provides a centralized system for modifying spell attributes.

## Overview

The spell modification system uses a **Base × Mult** pattern where:
- **Base** = Original game value (immutable)
- **Mult** = Multiplier starting at 1.0
- **Value** = Base × Mult (computed property)

## AttributeModifier

Represents a single modifiable attribute.

```csharp
var modifier = new AttributeModifier(baseValue: 10f);

// Add to multiplier (+50% = mult becomes 1.5)
modifier.AddMultiplier(0.5f);  // Value = 10 × 1.5 = 15

// Add more (+25% = mult becomes 1.75)
modifier.AddMultiplier(0.25f); // Value = 10 × 1.75 = 17.5

// Set multiplier directly
modifier.SetMultiplier(2.0f);  // Value = 10 × 2.0 = 20

// Reset to original
modifier.ResetMultiplier();    // Mult = 1.0, Value = 10
```

### Properties

- `Base` - Original value (read-only)
- `Mult` - Current multiplier
- `Value` - Computed as Base × Mult (read-only)

## SpellModifiers

Collection of all modifiable attributes for a single spell.

### Available Attributes

**Class Attributes** (from SpellObject):
- `DAMAGE` - Damage dealt
- `RADIUS` - Effect radius
- `POWER` - Horizontal knockback
- `Y_POWER` - Vertical knockback
- `HEAL` - Heal amount (Frog of Life only)

**Spell Table Attributes**:
- `cooldown` - Spell cooldown
- `windUp` - Cast time
- `windDown` - Recovery time
- `initialVelocity` - Projectile speed

### Usage

```csharp
using MageQuitModFramework.Spells;

// Get modifiers for a spell
if (SpellModificationSystem.SpellModifierTable.TryGetValue(
    SpellName.Fireball,
    out var modifiers))
{
    // Modify specific attribute
    if (modifiers.TryGetModifier("DAMAGE", out var damage))
    {
        damage.AddMultiplier(0.5f); // +50% damage
    }
    
    // Reset all attributes for this spell
    modifiers.ResetAllMultipliers();
}
```

## SpellModificationSystem

Central manager for all spell modifications.

### Initialization

Automatically initialized by `GameDataInitializer`. Captures default values for all spells.

```csharp
// Check if ready
if (!GameDataInitializer.IsLoaded)
{
    FrameworkPlugin.Log.LogWarning("Game data not loaded yet");
    return;
}
```

### Modifying Spells

```csharp
using MageQuitModFramework.Spells;

// Update a single attribute
SpellModificationSystem.TryUpdateModifier(
    SpellName.Fireball,
    "DAMAGE",
    0.5f  // +50% damage (adds to multiplier)
);

// Get a modifier to work with directly
if (SpellModificationSystem.TryGetModifier(
    SpellName.Fireball,
    "DAMAGE",
    out var modifier))
{
    modifier.SetMultiplier(2.0f); // Set directly to 2x
}
```

### Applying Changes

After modifying attributes, apply them to the game:

```csharp
using MageQuitModFramework.Spells;

// Get local player
var player = PlayerManager.players.Values
    .FirstOrDefault(p => p.localPlayerNumber >= 0);

// Apply all modifications
SpellModificationSystem.ApplyModifiersToGame(
    Globals.spell_manager,
    player
);
```

### Resetting

```csharp
// Reset all spells
SpellModificationSystem.ResetAllMultipliers();

// Reset specific spell
if (SpellModificationSystem.SpellModifierTable.TryGetValue(
    SpellName.Fireball,
    out var mods))
{
    mods.ResetAllMultipliers();
}
```

## Complete Example

```csharp
using MageQuitModFramework.Spells;
using MageQuitModFramework.Data;
using HarmonyLib;

[HarmonyPatch(typeof(SpellManager), "Awake")]
[HarmonyPostfix]
static void ModifySpells()
{
    if (!GameDataInitializer.IsLoaded) return;
    
    // Increase all spell damage by 50%
    foreach (var spell in SpellModificationSystem.SpellModifierTable.Keys)
    {
        SpellModificationSystem.TryUpdateModifier(spell, "DAMAGE", 0.5f);
    }
    
    // Reduce Fireball cooldown by 30%
    SpellModificationSystem.TryUpdateModifier(
        SpellName.Fireball,
        "cooldown",
        -0.3f  // -30% = mult becomes 0.7
    );
    
    // Apply changes
    var player = GetLocalPlayer();
    SpellModificationSystem.ApplyModifiersToGame(
        Globals.spell_manager,
        player
    );
}

static Player GetLocalPlayer()
{
    return PlayerManager.players.Values
        .FirstOrDefault(p => p.localPlayerNumber >= 0);
}
```

## Spell Name Mappings

Most spells map to `{SpellName}Object`, with exceptions:

```csharp
SpellName.RockBlock    → "StonewallObject"
SpellName.FlameLeash   → "BurningLeashObject"
SpellName.SomerAssault → "SomAssaultObject"
SpellName.Sustain      → "SustainObjectObject"
```

Use `SpellModificationSystem.GetSpellObjectTypeName(spellName)` to get the correct type name.
