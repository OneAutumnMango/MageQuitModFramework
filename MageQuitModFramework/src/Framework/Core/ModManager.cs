using HarmonyLib;
using System.Collections.Generic;
using MageQuitModFramework.Loading;

namespace MageQuitModFramework.Core
{
    public class ModEntry
    {
        public string ModName { get; set; }
        public ModuleManager ModuleManager { get; set; }
        public Harmony ModHarmony { get; set; }
    }

    public static class ModManager
    {
        private static readonly Dictionary<string, ModEntry> _mods = new();

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

        public static IEnumerable<ModEntry> GetAllMods()
        {
            return _mods.Values;
        }

        public static void Clear()
        {
            _mods.Clear();
        }
    }
}
