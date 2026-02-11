# MageQuit Mod Framework

A BepInEx plugin framework for creating mods that modify spells and game mechanics. This framework provides a shared infrastructure that multiple mods can use without interfering with each other.

## Architecture

The framework is compiled as a **standalone DLL** (`MageQuitModFramework.dll`) that:
- Runs as a BepInEx plugin itself
- Provides shared services (UI, modification systems, helpers)
- Allows multiple independent mods to register and coexist
- Manages a unified mod menu accessible via **F5**
- **Each mod maintains its own Harmony instance** for independent patch management

## For Mod Developers

### Creating a New Mod

1. **Create a new C# project** targeting `.NET Framework 4.7.2`

2. **Add framework reference** in your `.csproj`:
```xml
<Reference Include="MageQuitModFramework">
  <HintPath>..\MageQuitModFramework\bin\Debug\net472\MageQuitModFramework.dll</HintPath>
</Reference>
```

3. **Add BepInDependency** to your plugin class:
```csharp
using BepInEx;
using MageQuitModFramework;
using MageQuitModFramework.UI;
using MageQuitModFramework.Loading;

[BepInPlugin("com.yourname.yourmod", "Your Mod Name", "1.0.0")]
[BepInDependency("com.magequit.modframework", BepInDependency.DependencyFlags.HardDependency)]
public class YourModPlugin : BaseUnityPlugin
{
    private ModuleManager _moduleManager;
    
    private void Awake()
    {
        // Each mod gets its own ModuleManager with its own Harmony instance
        _moduleManager = ModManager.RegisterMod("Your Mod Name", Info.Metadata.GUID);
        
        // Register your modules (each gets its own Harmony instance too)
        _moduleManager.RegisterModule(new YourModule());
        
        // Register UI
        ModUIRegistry.RegisterMod(
            "Your Mod Name",
            "Description of your mod",
            BuildModUI,
            priority: 100  // Lower = appears first in menu
        );
    }

    private void BuildModUI()
    {
        // Build your mod's UI using UIComponents
        UIComponents.Label("My Settings");
        UIComponents.Button("Click Me");
    }
}
```

### Available Framework Features

#### UI Components
```csharp
using MageQuitModFramework.UI;

// Create UI elements in your mod menu panel
UIComponents.CreateText(parent, "id", "text", fontSize);
UIComponents.CreateButton(parent, "id", "text", onClick);
UIComponents.CreatePanel(parent, "id", width, height);
UIComponents.CreateScrollView(parent, width, height);
```

#### Spell Modification
```csharp
using MageQuitModFramework.Core;

// Modify spell table entries
GameModificationHelpers.ModifySpellTableEntry(manager, SpellName.Fireball, spell =>
{
    spell.cooldown *= 0.5f;
    spell.damage *= 1.5f;
});

// Access private fields
GameModificationHelpers.SetPrivateField<float>(instance, "fieldName", value);
var value = GameModificationHelpers.GetPrivateField<float>(instance, "fieldName");
```

#### Spell Modification System
```csharp
using MageQuitModFramework.Core;
using MageQuitModFramework.Modifiers;

// Initialize with default values (usually in a SpellManager.Awake patch)
SpellModificationSystem.Initialize(defaultSpellTable, defaultClassAttributes);

// Update modifiers
SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", 
    mod => mod.AddBase(10f).AddMult(0.5f));

// Apply all modifications to the game
SpellModificationSystem.ApplyModifiersToGame();
```

#### Upgrade System
```csharp
using MageQuitModFramework.Modifiers;

// Apply upgrades
UpgradeSystem.ApplyUpgrade(option, isPositive: true);

// Get available upgrades for current spell loadout
var options = UpgradeSystem.GenerateUpgradeOptions(player, count: 10);
```

## How Multiple Mods Coexist

### Harmony Instance Isolation
- **Each mod creates its own Harmony instance** using `ModManager.RegisterMod(modName, modGuid)`
- Each module within a mod gets its own sub-Harmony instance
- Mods can unpatch their changes independently without affecting other mods
- Follows Harmony best practices for multi-mod environments

### Mod Menu Integration
- Each mod calls `ModUIRegistry.RegisterMod(...)` during initialization
- The framework maintains a **static registry** accessible across all assemblies
- Press **F5** to open the mod menu showing all registered mods
- Each mod entry expands to show that mod's custom UI panel

### Modification Isolation
- Each mod modifies spell data independently
- The `SpellModificationSystem` accumulates modifications from all mods
- Modifications are applied cumulatively (Base + Multiplier pattern)
- No conflicts between mods modifying the same spells

### Example: Two Mods Modifying Fireball
```csharp
// Mod A
SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE",
    mod => mod.AddBase(10f));  // +10 base damage

// Mod B  
SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE",
    mod => mod.AddMult(0.5f));  // +50% damage multiplier

// Result: (OriginalDamage + 10) * 1.5
```

## Building

### Framework
```bash
cd MageQuitModFramework
dotnet build
# Output: bin/Debug/net472/MageQuitModFramework.dll
```

### Your Mod
```bash
cd YourMod
dotnet build
# Ensure MageQuitModFramework.dll is in BepInEx/plugins before your mod loads
```

## Load Order

BepInEx loads plugins alphabetically by GUID. The framework uses:
- GUID: `com.magequit.modframework` 
- This ensures it loads before most mods (starting with 'com.')
- Use `BepInDependency` to enforce framework loads first

## Distribution

When distributing your mod:
1. **Include MageQuitModFramework.dll** in your release
2. Instruct users to place both DLLs in `BepInEx/plugins/`
3. Users can run multiple framework-based mods simultaneously

## Example Project Structure

```
MageQuitModFramework/          # Framework project
├── src/
│   ├── FrameworkPlugin.cs      # Main plugin
│   ├── Framework/
│   │   ├── Core/               # Spell modification
│   │   ├── UI/                 # UI components
│   │   ├── Modifiers/          # Upgrade system
│   │   └── Loading/            # Module system
│   └── MageQuitModFramework.csproj
└── bin/Debug/net472/
    └── MageQuitModFramework.dll

YourMod/                        # Your mod project
├── YourModPlugin.cs
├── YourMod.csproj              # References MageQuitModFramework.dll
└── bin/Debug/net472/
    └── YourMod.dll

BepInEx/plugins/                # Game plugins folder
├── MageQuitModFramework.dll   # Framework loads first
├── ModA.dll                    # Mod A
├── ModB.dll                    # Mod B
└── YourMod.dll                 # Your mod
```

## API Documentation

See [QUICK_REFERENCE.md](src/Framework/QUICK_REFERENCE.md) for detailed API documentation.

## License

MIT - feel free to use in your own mods!
