# Framework Quick Reference

## Common Tasks

### Modify a Spell Attribute

```csharp
SpellModificationSystem.TryUpdateModifier(
    SpellName.Fireball,
    "DAMAGE",
    0.5f  // +50% additive to multiplier
);

SpellModificationSystem.ApplyModifiersToGame(
    Globals.spell_manager,
    localPlayer
);
```

### Create a Simple Module

```csharp
using BalancePatch.Framework.Loading;
using HarmonyLib;

public class MyMod : BaseModModule
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
```

### Generate Upgrades

```csharp
var player = PlayerManager.players.Values.FirstOrDefault(p => p.localPlayerNumber >= 0);
var spells = player.cooldowns.Keys.ToList();

var upgrades = UpgradeSystem.GenerateUpgradeOptions(
    spells,
    count: 5,
    rng: new Random()
);
```

### Apply an Upgrade

```csharp
UpgradeSystem.ApplyUpgrade(upgrade, isPositive: true);

// Then apply to game
SpellModificationSystem.ApplyModifiersToGame(
    Globals.spell_manager,
    localPlayer
);
```

### Replace Float in IL

```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> MyPatch(IEnumerable<CodeInstruction> instructions)
{
    return GameModificationHelpers.ReplaceFloatConstant(
        instructions,
        oldValue: 4.7f,
        newValue: 3.5f
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

### Create Upgrade UI

```csharp
var upgradeUI = new UpgradeUI
{
    MaxUpgrades = 3,
    FreeBansPerRound = 1
};

upgradeUI.OnUpgradeSelected += (option, isPositive) =>
{
    UpgradeSystem.ApplyUpgrade(option, isPositive);
};

upgradeUI.OnUpgradeBanned += (option) =>
{
    UpgradeSystem.BannedUpgrades.Add((option.Spell, option.Attribute));
};

// In OnGUI
upgradeUI.SetUpgradeOptions(upgrades);
upgradeUI.Draw();
```

## Available Attributes

### Class Attributes (SpellObject fields)
- `DAMAGE` - Damage dealt
- `RADIUS` - Effect radius
- `POWER` - Knockback force
- `Y_POWER` - Vertical knockback
- `HEAL` - Healing amount (Frog of Life only)

### Spell Table Attributes
- `cooldown` - Spell cooldown time
- `windUp` - Cast time
- `windDown` - Recovery time after cast
- `initialVelocity` - Projectile speed

## Tier Values

```csharp
Tier.Common:    Rate 100%, Up +25%, Down -10%
Tier.Rare:      Rate  25%, Up +50%, Down -20%
Tier.Legendary: Rate   5%, Up +75%, Down -30%
```

## Module Lifecycle

```csharp
// Initialize framework
LoaderV2.Initialize();

// Register module
ModuleManager.RegisterModule(new MyModule());

// Load module
ModuleManager.LoadModule("MyModule");

// Check if loaded
bool isLoaded = ModuleManager.IsModuleLoaded("MyModule");

// Unload module
ModuleManager.UnloadModule("MyModule");
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

### Check Game Data Loaded
```csharp
if (!GameDataInitializer.IsLoaded)
{
    FrameworkPlugin.Log.LogError("Game data not loaded");
    return;
}
```

### Get Spell Manager
```csharp
var mgr = Globals.spell_manager;
if (mgr == null || mgr.spell_table == null)
    return;
```

### Patch on Round End
```csharp
[HarmonyPatch(typeof(NetworkManager), "CombineRoundScores")]
[HarmonyPostfix]
static void OnRoundEnd()
{
    if (PlayerManager.round <= 0) return;
    // Your code
}
```

### Set Private Field
```csharp
GameModificationHelpers.SetPrivateField(
    instance,
    "START_TIME",
    5.0f
);
```

## Style Colors

```csharp
StyleManager.Green          // Positive/Up button style
StyleManager.Red            // Negative/Down button style
StyleManager.CommonStyle    // Common tier label
StyleManager.RareStyle      // Purple rare tier label
StyleManager.LegendaryStyle // Gold legendary tier label
```

## Ban Upgrades

```csharp
// Ban specific upgrade
UpgradeSystem.BannedUpgrades.Add((SpellName.Fireball, "DAMAGE"));

// Manual rejections (attribute doesn't work for this spell)
UpgradeSystem.ManualRejections[SpellName.FrogOfLife] = 
    new[] { "DAMAGE", "POWER", "Y_POWER" };
```

## Filter Upgrades

```csharp
var upgrades = UpgradeSystem.GenerateUpgradeOptions(
    spells,
    count: 5,
    rng: new Random(),
    spellFilter: spell => spell != SpellName.Primary // Exclude primary
);
```

## Reset Modifications

```csharp
// Reset all multipliers to 1.0
SpellModificationSystem.ResetAllMultipliers();

// Reset specific spell
if (SpellModificationSystem.SpellModifierTable.TryGetValue(
    SpellName.Fireball, 
    out var mods))
{
    mods.ResetAllMultipliers();
}
```
