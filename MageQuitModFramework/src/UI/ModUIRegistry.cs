using System;
using System.Collections.Generic;
using UnityEngine;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.UI
{
    /// <summary>
    /// Represents a registered mod's UI configuration including display name, description,
    /// drawing callback, and UI state.
    /// </summary>
    public class ModUIEntry
    {
        /// <summary>
        /// The display name of the mod shown in the UI.
        /// </summary>
        public string ModName { get; set; }

        /// <summary>
        /// Optional description text displayed when the mod entry is expanded.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// IMGUI drawing callback invoked when the mod's UI panel is expanded.
        /// </summary>
        public Action DrawIMGUI { get; set; }

        /// <summary>
        /// Whether the mod's UI panel is currently expanded in the menu.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Display priority for sorting mod entries. Lower values appear first.
        /// Default is 100.
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Whether the mod is currently enabled. When disabled, the mod's UI content will not be drawn.
        /// Default is true.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Central registry for mod UI entries. Manages registration, retrieval, and sorting of mod menu interfaces.
    /// </summary>
    public static class ModUIRegistry
    {
        private static Dictionary<string, ModUIEntry> _entries = new();
        private static List<ModUIEntry> _sortedEntries = null;

        /// <summary>
        /// Registers a mod's UI with the framework menu system.
        /// </summary>
        /// <param name="modName">Unique identifier and display name for the mod</param>
        /// <param name="description">Optional description shown when the mod entry is expanded</param>
        /// <param name="drawIMGUI">IMGUI drawing callback for rendering the mod's settings/controls</param>
        /// <param name="priority">Display priority for sorting (default 100, lower values appear first)</param>
        public static void RegisterMod(string modName, string description, Action drawIMGUI, int priority = 100)
        {
            _entries[modName] = new ModUIEntry
            {
                ModName = modName,
                Description = description,
                DrawIMGUI = drawIMGUI,
                Priority = priority
            };
            _sortedEntries = null;
        }

        /// <summary>
        /// Unregisters a mod from the UI registry.
        /// </summary>
        /// <param name="modName">The name of the mod to remove</param>
        public static void UnregisterMod(string modName)
        {
            if (_entries.Remove(modName))
                _sortedEntries = null;
        }

        /// <summary>
        /// Gets all registered mods sorted by priority (lowest first).
        /// Results are cached until the registry is modified.
        /// </summary>
        /// <returns>Enumerable of mod entries in priority order</returns>
        public static IEnumerable<ModUIEntry> GetAllMods()
        {
            if (_sortedEntries == null)
            {
                _sortedEntries = new List<ModUIEntry>(_entries.Values);
                _sortedEntries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
            return _sortedEntries;
        }

        /// <summary>
        /// Attempts to retrieve a specific mod's UI entry by name.
        /// </summary>
        /// <param name="modName">The name of the mod to find</param>
        /// <param name="entry">The mod entry if found, or null</param>
        /// <returns>True if the mod was found, false otherwise</returns>
        public static bool TryGetMod(string modName, out ModUIEntry entry)
        {
            return _entries.TryGetValue(modName, out entry);
        }

        /// <summary>
        /// Clears all registered mods from the registry.
        /// </summary>
        public static void Clear()
        {
            _entries.Clear();
            _sortedEntries = null;
        }
    }
}
