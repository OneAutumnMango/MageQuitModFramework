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
    [BepInPlugin("com.magequit.modframework", "MageQuit Mod Framework", "1.3.0")]
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
            _harmony.PatchAll(typeof(Debugging.PlayerUtils));
            Debugging.PlayerUtils.PatchAll(_harmony);


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
                AddModButtons,
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
        /// Adds all mod buttons to the mod menu.
        /// </summary>
        public void AddModButtons()
        {
            AddTogglePlayerInvincibilityButton();
            AddToggleGodModeButton();
            AddHPModButtons();
            AddKillSelfButton();
        }

        /// <summary>
        /// Adds a toggle button for player invincibility to the mod menu.
        /// </summary>
        public void AddTogglePlayerInvincibilityButton()
        {
            string label = $"Invincibility {(Debugging.PlayerUtils.Invincible ? "ON" : "OFF")}";
            if (UIComponents.Button(label))
            {
                Debugging.PlayerUtils.ToggleInvincibility();
                Log.LogInfo($"Invincibility toggled to {(Debugging.PlayerUtils.Invincible ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// Adds a toggle button for god mode to the mod menu.
        /// </summary>
        public void AddToggleGodModeButton()
        {
            string label = $"God Mode {(Debugging.PlayerUtils.GodMode ? "ON" : "OFF")}";
            if (UIComponents.Button(label))
            {
                Debugging.PlayerUtils.ToggleGodMode();
                Log.LogInfo($"God Mode toggled to {(Debugging.PlayerUtils.GodMode ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// Adds a button to the mod menu that allows the player to kill themselves.
        /// </summary>
        public void AddKillSelfButton()
        {
            if (UIComponents.Button("Kill Self"))
                Debugging.PlayerUtils.KillSelf();
        }

        /// <summary>
        /// Adds buttons to the mod menu for modifying the player's HP.
        /// </summary>
        public void AddHPModButtons()
        {
            if (UIComponents.Button("100% HP"))
                Debugging.PlayerUtils.FullHP();

            if (UIComponents.Button("50% HP"))
                Debugging.PlayerUtils.HalfHP();

            if (UIComponents.Button("25% HP"))
                Debugging.PlayerUtils.QuarterHP();
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
