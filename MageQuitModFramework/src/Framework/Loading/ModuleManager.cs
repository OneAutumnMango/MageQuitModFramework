using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace MageQuitModFramework.Loading
{
    public static class ModuleManager
    {
        private static readonly Dictionary<string, IModModule> _modules = [];
        private static Harmony _harmony;

        public static void Initialize(Harmony harmony)
        {
            _harmony = harmony;
        }

        public static void RegisterModule(IModModule module)
        {
            if (_modules.ContainsKey(module.ModuleName))
            {
                FrameworkPlugin.Log?.LogWarning($"Module {module.ModuleName} is already registered");
                return;
            }

            _modules[module.ModuleName] = module;
            FrameworkPlugin.Log?.LogInfo($"Registered module: {module.ModuleName}");
        }

        public static bool LoadModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
            {
                FrameworkPlugin.Log?.LogError($"Module {moduleName} not found");
                return false;
            }

            module.Load(_harmony);
            return true;
        }

        public static bool UnloadModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
            {
                FrameworkPlugin.Log?.LogError($"Module {moduleName} not found");
                return false;
            }

            module.Unload(_harmony);
            return true;
        }

        public static bool IsModuleLoaded(string moduleName)
        {
            return _modules.TryGetValue(moduleName, out var module) && module.IsLoaded;
        }

        public static IEnumerable<string> GetAllModuleNames()
        {
            return _modules.Keys;
        }

        public static IEnumerable<string> GetLoadedModuleNames()
        {
            return _modules.Where(kvp => kvp.Value.IsLoaded).Select(kvp => kvp.Key);
        }

        public static void Clear()
        {
            _modules.Clear();
        }
    }
}
