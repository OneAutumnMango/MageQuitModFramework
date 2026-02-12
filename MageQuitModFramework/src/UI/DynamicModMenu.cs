using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.UI
{
    /// <summary>
    /// In-game IMGUI menu for viewing and managing registered mods.
    /// Toggle visibility with F5 key. Displays expandable panels for each registered mod.
    /// </summary>
    public class DynamicModMenu : MonoBehaviour
    {
        private bool _isVisible = false;
        private Rect _windowRect = new Rect(20, 20, 500, 600);
        private Vector2 _scrollPosition = Vector2.zero;
        private Dictionary<string, bool> _modExpanded = new Dictionary<string, bool>();
        private Dictionary<string, HashSet<string>> _savedModuleStates = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Initializes the mod menu component.
        /// </summary>
        public void Initialize()
        {
            FrameworkPlugin.Log.LogInfo("Initializing mod menu with IMGUI");
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            GUI.depth = -1000;
            _windowRect = GUI.Window(12345, _windowRect, DrawModMenu, "Mod Menu (F5 to toggle)");
        }

        private void DrawModMenu(int windowID)
        {
            GUILayout.BeginVertical();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(550));

            var mods = ModUIRegistry.GetAllMods().ToList();

            if (mods.Count == 0)
            {
                GUILayout.Label("No mods registered yet.");
            }
            else
            {
                foreach (var modEntry in mods)
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    // Mod header with expand/collapse button and enable toggle
                    GUILayout.BeginHorizontal();

                    if (!_modExpanded.ContainsKey(modEntry.ModName))
                        _modExpanded[modEntry.ModName] = modEntry.IsExpanded;

                    string arrow = _modExpanded[modEntry.ModName] ? "▼" : "►";
                    if (GUILayout.Button(arrow + " " + modEntry.ModName, GUILayout.Height(30), GUILayout.Width(380)))
                    {
                        _modExpanded[modEntry.ModName] = !_modExpanded[modEntry.ModName];
                        modEntry.IsExpanded = _modExpanded[modEntry.ModName];
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Enabled:", GUILayout.ExpandWidth(false));
                    bool previousEnabled = modEntry.IsEnabled;
                    modEntry.IsEnabled = GUILayout.Toggle(modEntry.IsEnabled, "", GUILayout.Width(20));

                    // Handle enable/disable toggle changes
                    if (previousEnabled != modEntry.IsEnabled)
                    {
                        ToggleModModules(modEntry.ModName, modEntry.IsEnabled);
                    }

                    GUILayout.EndHorizontal();

                    // Show options if expanded
                    if (_modExpanded[modEntry.ModName])
                    {
                        GUILayout.BeginVertical(GUI.skin.box);

                        // Grey out and disable interactions when mod is disabled
                        bool wasGuiEnabled = GUI.enabled;
                        GUI.enabled = modEntry.IsEnabled;

                        // Show description
                        if (!string.IsNullOrEmpty(modEntry.Description))
                        {
                            GUILayout.Label(modEntry.Description, GUI.skin.GetStyle("Label"));
                            GUILayout.Space(5);
                        }

                        // Draw module toggles for this mod
                        if (ModManager.TryGetModuleManager(modEntry.ModName, out var moduleManager))
                        {
                            var moduleNames = moduleManager.GetAllModuleNames().ToList();
                            if (moduleNames.Count > 0)
                            {
                                foreach (var moduleName in moduleNames)
                                {
                                    UIComponents.DrawModuleToggle(modEntry.ModName, moduleName, moduleManager);
                                }
                                GUILayout.Space(10);
                            }
                        }

                        // Call IMGUI callback if available
                        if (modEntry.DrawIMGUI != null)
                        {
                            try
                            {
                                modEntry.DrawIMGUI();
                            }
                            catch (Exception ex)
                            {
                                GUILayout.Label($"Error drawing mod UI: {ex.Message}");
                            }
                        }
                        else
                        {
                            GUILayout.Label("No options available for this mod.");
                        }

                        // Restore previous enabled state
                        GUI.enabled = wasGuiEnabled;

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 500, 20));
        }

        /// <summary>
        /// Clears the expanded state of all mod entries, causing them to collapse.
        /// </summary>
        public void RefreshModList()
        {
            _modExpanded.Clear();
        }

        /// <summary>
        /// Shows the mod menu.
        /// </summary>
        public void Show()
        {
            _isVisible = true;
        }

        /// <summary>
        /// Hides the mod menu.
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
        }

        /// <summary>
        /// Toggles all modules for a mod on or off, saving and restoring their previous state.
        /// </summary>
        /// <param name="modName">The name of the mod</param>
        /// <param name="enable">True to enable modules, false to disable</param>
        private void ToggleModModules(string modName, bool enable)
        {
            if (!ModManager.TryGetModuleManager(modName, out var moduleManager))
                return;

            if (enable)
            {
                // Restore previous state
                if (_savedModuleStates.TryGetValue(modName, out var savedState))
                {
                    foreach (var moduleName in savedState)
                    {
                        moduleManager.LoadModule(moduleName);
                        FrameworkPlugin.Log?.LogInfo($"Re-enabled module: {moduleName}");
                    }
                }
                else
                {
                    // First time enabling, load all modules
                    foreach (var moduleName in moduleManager.GetAllModuleNames())
                    {
                        if (!moduleManager.IsModuleLoaded(moduleName))
                        {
                            moduleManager.LoadModule(moduleName);
                        }
                    }
                }
            }
            else
            {
                // Save current state and disable all
                var loadedModules = new HashSet<string>();
                foreach (var moduleName in moduleManager.GetAllModuleNames())
                {
                    if (moduleManager.IsModuleLoaded(moduleName))
                    {
                        loadedModules.Add(moduleName);
                        moduleManager.UnloadModule(moduleName);
                        FrameworkPlugin.Log?.LogInfo($"Disabled module: {moduleName}");
                    }
                }
                _savedModuleStates[modName] = loadedModules;
            }
        }

        /// <summary>
        /// Toggles the mod menu visibility.
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
        }
    }
}
