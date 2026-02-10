using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using MageQuitModFramework.UI;

namespace MageQuitModFramework
{
    [BepInPlugin("com.magequit.modframework", "MageQuit Mod Framework", "1.0.0")]
    public class FrameworkPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static FrameworkPlugin Instance { get; private set; }

        private DynamicModMenu _modMenu;
        private bool _menuVisible = false;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("Spellcast Mod Framework initialized");

            var menuObj = new GameObject("SpellcastModMenu");
            DontDestroyOnLoad(menuObj);
            _modMenu = menuObj.AddComponent<DynamicModMenu>();
            _modMenu.Initialize();

            Log.LogInfo("Dynamic mod menu created and ready");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ToggleModMenu();
            }
        }

        public void ToggleModMenu()
        {
            _menuVisible = !_menuVisible;
            if (_menuVisible)
                _modMenu?.Show();
            else
                _modMenu?.Hide();

            Log.LogInfo($"Mod menu {(_menuVisible ? "shown" : "hidden")}");
        }

        public void RefreshModMenu()
        {
            _modMenu?.RefreshModList();
        }
    }
}
