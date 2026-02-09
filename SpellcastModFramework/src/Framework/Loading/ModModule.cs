using HarmonyLib;
using System;

namespace SpellcastModFramework.Loading
{
    public interface IModModule
    {
        string ModuleName { get; }
        bool IsLoaded { get; }
        void Load(Harmony harmony);
        void Unload(Harmony harmony);
    }

    public abstract class BaseModModule : IModModule
    {
        public abstract string ModuleName { get; }
        public bool IsLoaded { get; protected set; }

        public virtual void Load(Harmony harmony)
        {
            if (IsLoaded)
            {
                FrameworkPlugin.Log.LogWarning($"{ModuleName} is already loaded");
                return;
            }

            try
            {
                OnLoad(harmony);
                IsLoaded = true;
                FrameworkPlugin.Log.LogInfo($"{ModuleName} loaded successfully");
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.LogError($"Failed to load {ModuleName}: {ex}");
                throw;
            }
        }

        public virtual void Unload(Harmony harmony)
        {
            if (!IsLoaded)
            {
                FrameworkPlugin.Log.LogWarning($"{ModuleName} is not loaded");
                return;
            }

            try
            {
                OnUnload(harmony);
                IsLoaded = false;
                FrameworkPlugin.Log.LogInfo($"{ModuleName} unloaded successfully");
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.LogError($"Failed to unload {ModuleName}: {ex}");
                throw;
            }
        }

        protected abstract void OnLoad(Harmony harmony);
        protected abstract void OnUnload(Harmony harmony);
    }
}
