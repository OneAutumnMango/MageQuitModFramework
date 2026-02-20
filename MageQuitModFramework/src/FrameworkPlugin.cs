using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MageQuitModFramework.UI;
using MageQuitModFramework.Modding;
using MageQuitModFramework.Utilities;

namespace MageQuitModFramework
{
    /// <summary>
    /// Main BepInEx plugin for the MageQuit Mod Framework.
    /// Initializes the framework, manages the mod menu, and provides global access to logging.
    /// </summary>
    [BepInPlugin("com.magequit.modframework", "MageQuit Mod Framework", "1.1.0")]
    public class FrameworkPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// Global logger instance accessible to all mods using the framework.
        /// </summary>
        public static ManualLogSource Log { get; private set; }

        /// <summary>
        /// Singleton instance of the framework plugin.
        /// </summary>
        public static FrameworkPlugin Instance { get; private set; }

        private DynamicModMenu _modMenu;
        private Harmony _harmony;
        private ModuleManager _debugModuleManager;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("MageQuit Mod Framework initialized");

            _harmony = new Harmony("com.magequit.modframework");
            _harmony.PatchAll(typeof(Data.GameDataInitializer));
            _harmony.PatchAll(typeof(Data.GameEventsObserver));

            var menuObj = new GameObject("MageQuitModMenu");
            DontDestroyOnLoad(menuObj);
            _modMenu = menuObj.AddComponent<DynamicModMenu>();
            _modMenu.Initialize();

            PhotonHelper.InitializeEventSystem();


            _debugModuleManager = ModManager.RegisterMod("Debugger", "com.magequit.modframework.debug");
            _debugModuleManager.RegisterModule(new Debugging.InstantiateLogModule());
            _debugModuleManager.RegisterModule(new Debugging.HitboxModule());
            _debugModuleManager.RegisterModule(new Debugging.DamageHealingLogModule());

            ModUIRegistry.RegisterMod(
                "Debugger",
                "Debug utilities: damage hitboxes, damage/healing logs, unity object instantiation logs",
                () => {},
                priority: 1000  // at bottom
            );

            Log.LogInfo("Dynamic mod menu created and ready");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ToggleModMenu();
            }
        }

        /// <summary>
        /// Toggles the visibility of the in-game mod menu.
        /// </summary>
        public void ToggleModMenu()
        {
            _modMenu.Toggle();

            Log.LogInfo($"Mod menu GO: {(_modMenu.gameObject.activeSelf ? "shown" : "hidden")}");
        }

        /// <summary>
        /// Refreshes the mod menu to reflect changes in registered mods or modules.
        /// </summary>
        public void RefreshModMenu()
        {
            _modMenu?.RefreshModList();
        }
    }
}
