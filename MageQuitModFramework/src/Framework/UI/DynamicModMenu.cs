using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.UI
{
    public class DynamicModMenu : MonoBehaviour
    {
        private bool _isVisible = false;
        private Rect _windowRect = new Rect(20, 20, 500, 600);
        private Vector2 _scrollPosition = Vector2.zero;
        private Dictionary<string, bool> _modExpanded = new Dictionary<string, bool>();

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

                    // Mod header with expand/collapse button
                    GUILayout.BeginHorizontal();

                    if (!_modExpanded.ContainsKey(modEntry.ModName))
                        _modExpanded[modEntry.ModName] = modEntry.IsExpanded;

                    string arrow = _modExpanded[modEntry.ModName] ? "▼" : "►";
                    if (GUILayout.Button(arrow + " " + modEntry.ModName, GUILayout.Height(30)))
                    {
                        _modExpanded[modEntry.ModName] = !_modExpanded[modEntry.ModName];
                        modEntry.IsExpanded = _modExpanded[modEntry.ModName];
                    }

                    GUILayout.EndHorizontal();

                    // Show options if expanded
                    if (_modExpanded[modEntry.ModName])
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        
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

        public void RefreshModList()
        {
            _modExpanded.Clear();
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }
    }
}
