# Modding - Module System

The Modding namespace provides lifecycle management for organizing mod functionality.

## BaseModule

Abstract base class for creating loadable/unloadable mod modules.

### Creating a Module

```csharp
using MageQuitModFramework.Modding;
using HarmonyLib;

public class MyModule : BaseModule
{
    public override string ModuleName => "MyModule";

    protected override void OnLoad(Harmony harmony)
    {
        // Called when module loads
        harmony.PatchAll(typeof(MyPatches));
        FrameworkPlugin.Log.LogInfo($"{ModuleName} loaded");
    }

    protected override void OnUnload(Harmony harmony)
    {
        // Called when module unloads
        harmony.UnpatchSelf();
        FrameworkPlugin.Log.LogInfo($"{ModuleName} unloaded");
    }
}
```

### Module Lifecycle

1. **Registration** - `ModuleManager.RegisterModule(module)`
2. **Loading** - `ModuleManager.LoadModule(moduleName)` calls `OnLoad()`
3. **Running** - Module is active, patches applied
4. **Unloading** - `ModuleManager.UnloadModule(moduleName)` calls `OnUnload()`

### ModuleManager

Manages registration and lifecycle of modules.

```csharp
using MageQuitModFramework.Modding;

// Register a module
ModuleManager.RegisterModule(new MyModule());

// Load it
ModuleManager.LoadModule("MyModule");

// Check status
bool isLoaded = ModuleManager.IsModuleLoaded("MyModule");

// Unload it
ModuleManager.UnloadModule("MyModule");

// Get all registered modules
var allModules = ModuleManager.GetAllModules();
```

### Best Practices

#### Clean Up Resources

```csharp
protected override void OnUnload(Harmony harmony)
{
    // Unpatch Harmony patches
    harmony.UnpatchSelf();
    
    // Reset spell modifications
    SpellModificationSystem.ResetAllMultipliers();
    
    // Unregister UI
    ModUIRegistry.UnregisterMod(ModuleName);
    
    // Clear any static state
    MyStaticCache.Clear();
}
```

#### Wait for Game Data

```csharp
protected override void OnLoad(Harmony harmony)
{
    if (GameDataInitializer.IsLoaded)
        Initialize(harmony);
    else
        GameDataInitializer.OnGameDataLoaded += () => Initialize(harmony);
}

private void Initialize(Harmony harmony)
{
    // Apply patches that need game data
    harmony.PatchAll(typeof(MyPatches));
}
```

#### Organize by Feature

Create separate modules for distinct features:

```csharp
ModuleManager.RegisterModule(new BalanceModule());
ModuleManager.RegisterModule(new DebugModule());
ModuleManager.RegisterModule(new RandomizerModule());

// Load only what you need
ModuleManager.LoadModule("Balance");
ModuleManager.LoadModule("Debug");
```

### Module Properties

- `ModuleName` - Unique identifier for the module
- `IsLoaded` - Whether the module is currently loaded

### Error Handling

Exceptions during `OnLoad()` are caught and logged:

```csharp
protected override void OnLoad(Harmony harmony)
{
    try
    {
        harmony.PatchAll(typeof(MyPatches));
    }
    catch (Exception ex)
    {
        FrameworkPlugin.Log.LogError($"Failed to patch: {ex}");
        throw; // Re-throw to mark load as failed
    }
}
```

If `OnLoad()` throws, the module's `IsLoaded` remains `false`.
