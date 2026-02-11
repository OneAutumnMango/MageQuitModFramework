using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace MageQuitModFramework.Modding
{
    public class ModuleManager
    {
        private readonly Dictionary<string, IModModule> _modules = new();
        private readonly Dictionary<string, Harmony> _moduleHarmonyInstances = new();
        private readonly Harmony _baseHarmony;

        public ModuleManager(Harmony harmony)
        {
            _baseHarmony = harmony;
        }

        public void RegisterModule(IModModule module)
        {
            if (_modules.ContainsKey(module.ModuleName))
            {
                FrameworkPlugin.Log?.LogWarning($"Module {module.ModuleName} is already registered");
                return;
            }

            _modules[module.ModuleName] = module;
            FrameworkPlugin.Log?.LogInfo($"Registered module: {module.ModuleName}");
        }

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

        public bool IsModuleLoaded(string moduleName)
        {
            return _modules.TryGetValue(moduleName, out var module) && module.IsLoaded;
        }

        public IEnumerable<string> GetAllModuleNames()
        {
            return _modules.Keys;
        }

        public IEnumerable<string> GetLoadedModuleNames()
        {
            return _modules.Where(kvp => kvp.Value.IsLoaded).Select(kvp => kvp.Key);
        }

        public void Clear()
        {
            _modules.Clear();
            _moduleHarmonyInstances.Clear();
        }
    }
}
