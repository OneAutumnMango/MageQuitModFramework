using HarmonyLib;
using System.Collections.Generic;

namespace MageQuitModFramework.Modding
{
    /// <summary>
    /// Represents a registered mod with its module manager and Harmony instance.
    /// </summary>
    public class ModEntry
    {
        /// <summary>
        /// The unique name of the mod.
        /// </summary>
        public string ModName { get; set; }

        /// <summary>
        /// The module manager instance handling this mod's modules.
        /// </summary>
        public ModuleManager ModuleManager { get; set; }

        /// <summary>
        /// The Harmony instance used for patching game code.
        /// </summary>
        public Harmony ModHarmony { get; set; }
    }

    /// <summary>
    /// Global registry for managing multiple mods and their module managers.
    /// Provides mod registration, retrieval, and Harmony instance coordination.
    /// </summary>
    public static class ModManager
    {
        private static readonly Dictionary<string, ModEntry> _mods = [];

        /// <summary>
        /// Registers a new mod with the framework and creates its module manager.
        /// </summary>
        /// <param name="modName">Unique display name for the mod</param>
        /// <param name="modGuid">Unique GUID for the mod's Harmony instance</param>
        /// <returns>The module manager instance for registering and managing modules</returns>
        public static ModuleManager RegisterMod(string modName, string modGuid)
        {
            if (_mods.ContainsKey(modName))
            {
                FrameworkPlugin.Log?.LogWarning($"Mod '{modName}' already registered");
                return _mods[modName].ModuleManager;
            }

            var modHarmony = new Harmony(modGuid);
            var moduleManager = new ModuleManager(modHarmony);

            _mods[modName] = new ModEntry
            {
                ModName = modName,
                ModuleManager = moduleManager,
                ModHarmony = modHarmony
            };

            FrameworkPlugin.Log?.LogInfo($"Registered mod: {modName}");
            return moduleManager;
        }

        /// <summary>
        /// Attempts to retrieve the module manager for a registered mod.
        /// </summary>
        /// <param name="modName">The name of the mod to look up</param>
        /// <param name="moduleManager">Output parameter for the module manager if found</param>
        /// <returns>True if the mod was found, false otherwise</returns>
        public static bool TryGetModuleManager(string modName, out ModuleManager moduleManager)
        {
            if (_mods.TryGetValue(modName, out var entry))
            {
                moduleManager = entry.ModuleManager;
                return true;
            }
            moduleManager = null;
            return false;
        }

        /// <summary>
        /// Checks if a module is currently loaded by any registered mod.
        /// Good for cross-mod compatibility checks without needing direct references to other mods.
        /// </summary>
        /// <param name="moduleName">The name of the module to check</param>
        /// <returns>True if the module exists and is loaded, false otherwise</returns>
        public static bool IsModuleLoaded(string moduleName)
        {
            foreach (var mod in _mods.Values)
            {
                if (mod.ModuleManager.IsModuleLoaded(moduleName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all registered mod entries.
        /// </summary>
        /// <returns>Collection of all registered mods</returns>
        public static IEnumerable<ModEntry> GetAllMods()
        {
            return _mods.Values;
        }

        /// <summary>
        /// Clears all registered mods from the manager.
        /// </summary>
        public static void Clear()
        {
            _mods.Clear();
        }
    }
}
