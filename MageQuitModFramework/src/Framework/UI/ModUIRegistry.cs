using System;
using System.Collections.Generic;
using UnityEngine;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.UI
{
    public class ModUIEntry
    {
        public string ModName { get; set; }
        public string Description { get; set; }
        public Action DrawIMGUI { get; set; }
        public bool IsExpanded { get; set; }
        public int Priority { get; set; } = 100;
    }

    public static class ModUIRegistry
    {
        private static Dictionary<string, ModUIEntry> _entries = new();
        private static List<ModUIEntry> _sortedEntries = null;

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

        public static void UnregisterMod(string modName)
        {
            if (_entries.Remove(modName))
                _sortedEntries = null;
        }

        public static IEnumerable<ModUIEntry> GetAllMods()
        {
            if (_sortedEntries == null)
            {
                _sortedEntries = new List<ModUIEntry>(_entries.Values);
                _sortedEntries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
            return _sortedEntries;
        }

        public static bool TryGetMod(string modName, out ModUIEntry entry)
        {
            return _entries.TryGetValue(modName, out entry);
        }

        public static void Clear()
        {
            _entries.Clear();
            _sortedEntries = null;
        }
    }
}
