# Data - Game Data Initialization

The Data namespace provides hooks into the game's data loading process.

## GameDataInitializer

Patches `SpellManager.Awake` to capture default game data and fire events when data is loaded.

### Key Features

- **Default Spell Table** - Stores original spell attributes
- **Default Class Attributes** - Stores original spell object fields
- **Load Event** - `OnGameDataLoaded` event fires when data is ready
- **Load Status** - `IsLoaded` property to check if data is available

### Usage

```csharp
using MageQuitModFramework.Data;

// Check if already loaded
if (GameDataInitializer.IsLoaded)
{
    Initialize();
}
else
{
    // Wait for data to load
    GameDataInitializer.OnGameDataLoaded += Initialize;
}

private void Initialize()
{
    // Access default data
    var defaultFireball = GameDataInitializer.DefaultSpellTable[SpellName.Fireball];
    var defaultDamage = GameDataInitializer.DefaultClassAttributes[SpellName.Fireball]["DAMAGE"];
    
    // Your initialization code
}
```

### Properties

- `DefaultSpellTable` - Dictionary<SpellName, Spell> of original spell data
- `DefaultClassAttributes` - Dictionary<SpellName, Dictionary<string, float>> of original class attributes
- `IsLoaded` - bool indicating if data has been loaded
- `OnGameDataLoaded` - Event fired when data is ready

### When to Use

Use GameDataInitializer when you need to:
- Wait for game data before initializing your mod
- Access original, unmodified spell values
- Reset modifications back to defaults
- Compare current values with originals

### Example: Reset to Defaults

```csharp
using MageQuitModFramework.Data;
using MageQuitModFramework.Spells;

public void ResetAllSpells()
{
    foreach (var kvp in GameDataInitializer.DefaultSpellTable)
    {
        var spellName = kvp.Key;
        var originalSpell = kvp.Value;
        
        // Reset spell table attributes
        var currentSpell = Globals.spell_manager.spell_table[spellName];
        currentSpell.cooldown = originalSpell.cooldown;
        currentSpell.windUp = originalSpell.windUp;
        currentSpell.windDown = originalSpell.windDown;
    }
    
    // Reset modifiers
    SpellModificationSystem.ResetAllMultipliers();
}
```
