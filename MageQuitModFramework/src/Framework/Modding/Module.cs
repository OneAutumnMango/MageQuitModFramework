using HarmonyLib;
using System;
using System.Reflection;

namespace MageQuitModFramework.Modding
{
    public interface IModule
    {
        string ModuleName { get; }
        bool IsLoaded { get; }
        void Load(Harmony harmony);
        void Unload(Harmony harmony);
    }

    public abstract class BaseModule : IModule
    {
        public abstract string ModuleName { get; }
        public bool IsLoaded { get; protected set; }

        public virtual void Load(Harmony harmony)
        {
            if (IsLoaded)
            {
                FrameworkPlugin.Log?.LogWarning($"{ModuleName} is already loaded");
                return;
            }

            try
            {
                OnLoad(harmony);
                IsLoaded = true;
                FrameworkPlugin.Log?.LogInfo($"{ModuleName} loaded successfully");
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log?.LogError($"Failed to load {ModuleName}: {ex}");
                throw;
            }
        }

        public virtual void Unload(Harmony harmony)
        {
            if (!IsLoaded)
            {
                FrameworkPlugin.Log?.LogWarning($"{ModuleName} is not loaded");
                return;
            }

            try
            {
                OnUnload(harmony);
                IsLoaded = false;
                FrameworkPlugin.Log?.LogInfo($"{ModuleName} unloaded successfully");
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log?.LogError($"Failed to unload {ModuleName}: {ex}");
                throw;
            }
        }

        protected abstract void OnLoad(Harmony harmony);
        protected abstract void OnUnload(Harmony harmony);

        /// <summary>
        /// Patches all Harmony-attributed types in the same namespace as the marker type.
        /// </summary>
        /// <param name="harmony">Harmony instance to use for patching</param>
        /// <param name="markerType">Type whose namespace will be scanned for patches</param>
        protected static void PatchGroup(Harmony harmony, Type markerType)
        {
            var asm = markerType.Assembly;
            var targetNamespace = markerType.Namespace;

            foreach (var type in asm.GetTypes())
            {
                if (type.Namespace == targetNamespace)
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
            }
        }
    }
}
