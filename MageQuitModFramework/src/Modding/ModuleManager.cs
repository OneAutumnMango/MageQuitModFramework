using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace MageQuitModFramework.Modding
{
    /// <summary>
    /// Manages module registration, loading, and unloading for a specific mod.
    /// Each mod gets its own ModuleManager instance with isolated Harmony patching.
    /// </summary>
    public class ModuleManager
    {
        private readonly Dictionary<string, IModule> _modules = new();
        private readonly Dictionary<string, Harmony> _moduleHarmonyInstances = new();
        private readonly Harmony _baseHarmony;

        /// <summary>
        /// Creates a new module manager with the specified Harmony instance.
        /// </summary>
        /// <param name="harmony">The base Harmony instance for this mod</param>
        public ModuleManager(Harmony harmony)
        {
            _baseHarmony = harmony;
        }

        /// <summary>
        /// Registers a module with this manager. Does not load the module automatically.
        /// </summary>
        /// <param name="module">The module instance to register</param>
        public void RegisterModule(IModule module)
        {
            if (_modules.ContainsKey(module.ModuleName))
            {
                FrameworkPlugin.Log?.LogWarning($"Module {module.ModuleName} is already registered");
                return;
            }

            _modules[module.ModuleName] = module;
            FrameworkPlugin.Log?.LogInfo($"Registered module: {module.ModuleName}");
        }

        /// <summary>
        /// Loads a registered module and applies its Harmony patches.
        /// </summary>
        /// <param name="moduleName">The name of the module to load</param>
        /// <returns>True if loaded successfully, false if module not found</returns>
        public bool LoadModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
            {
                FrameworkPlugin.Log?.LogError($"Module {moduleName} not found");
                return false;
            }

            if (!_moduleHarmonyInstances.ContainsKey(moduleName))
            {
                _moduleHarmonyInstances[moduleName] = new Harmony($"{_baseHarmony.Id}.{moduleName}");
            }

            module.Load(_moduleHarmonyInstances[moduleName]);
            return true;
        }

        /// <summary>
        /// Unloads a module and removes its Harmony patches.
        /// </summary>
        /// <param name="moduleName">The name of the module to unload</param>
        /// <returns>True if unloaded successfully, false if module not found</returns>
        public bool UnloadModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
            {
                FrameworkPlugin.Log?.LogError($"Module {moduleName} not found");
                return false;
            }

            if (_moduleHarmonyInstances.TryGetValue(moduleName, out var harmony))
            {
                module.Unload(harmony);
            }

            return true;
        }

        /// <summary>
        /// Checks if a module is currently loaded.
        /// </summary>
        /// <param name="moduleName">The name of the module to check</param>
        /// <returns>True if the module exists and is loaded, false otherwise</returns>
        public bool IsModuleLoaded(string moduleName)
        {
            return _modules.TryGetValue(moduleName, out var module) && module.IsLoaded;
        }

        /// <summary>
        /// Gets the names of all registered modules (loaded and unloaded).
        /// </summary>
        /// <returns>Collection of all module names</returns>
        public IEnumerable<string> GetAllModuleNames()
        {
            return _modules.Keys;
        }

        /// <summary>
        /// Gets the names of only the currently loaded modules.
        /// </summary>
        /// <returns>Collection of loaded module names</returns>
        public IEnumerable<string> GetLoadedModuleNames()
        {
            return _modules.Where(kvp => kvp.Value.IsLoaded).Select(kvp => kvp.Key);
        }

        /// <summary>
        /// Clears all registered modules from this manager.
        /// </summary>
        public void Clear()
        {
            _modules.Clear();
            _moduleHarmonyInstances.Clear();
        }
    }
}
