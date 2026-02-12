using HarmonyLib;
using System;
using System.Reflection;

namespace MageQuitModFramework.Modding
{
    /// <summary>
    /// Interface for loadable/unloadable mod modules.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Unique name for this module.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Whether the module is currently loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Loads the module and applies patches.
        /// </summary>
        /// <param name="harmony">Harmony instance for patching</param>
        void Load(Harmony harmony);

        /// <summary>
        /// Unloads the module and removes patches.
        /// </summary>
        /// <param name="harmony">Harmony instance for unpatching</param>
        void Unload(Harmony harmony);
    }

    /// <summary>
    /// Base class for creating loadable modules with lifecycle management.
    /// </summary>
    public abstract class BaseModule : IModule
    {
        /// <summary>
        /// Unique identifier for this module.
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        /// Whether this module is currently loaded.
        /// </summary>
        public bool IsLoaded { get; protected set; }

        /// <summary>
        /// Loads the module, calling OnLoad() implementation.
        /// </summary>
        /// <param name="harmony">Harmony instance for patching</param>
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

        /// <summary>
        /// Unloads the module, calling OnUnload() implementation.
        /// </summary>
        /// <param name="harmony">Harmony instance for unpatching</param>
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

        /// <summary>
        /// Called when the module is loaded. Override to apply Harmony patches.
        /// </summary>
        /// <param name="harmony">Harmony instance for patching</param>
        protected abstract void OnLoad(Harmony harmony);

        /// <summary>
        /// Called when the module is unloaded. Override to remove patches and clean up.
        /// </summary>
        /// <param name="harmony">Harmony instance for unpatching</param>
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
