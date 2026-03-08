using System.Collections.Generic;

namespace MageQuitModFramework.Spells
{
    /// <summary>
    /// Registry for human-readable display names for SpellName enum values.
    /// Mods that add custom spell names (out-of-range enum casts) should register
    /// display names here so that UI code can show friendly names instead of raw numbers.
    /// </summary>
    public static class SpellNameRegistry
    {
        private static readonly Dictionary<SpellName, string> _names = [];

        /// <summary>
        /// Registers a display name for a spell.
        /// Overwrites any previously registered name for the same spell.
        /// </summary>
        /// <param name="spell">The spell name value (may be a cast out-of-range value)</param>
        /// <param name="displayName">Human-readable name shown in UI (e.g. "Axe Primary")</param>
        public static void Register(SpellName spell, string displayName)
        {
            _names[spell] = displayName;
        }

        /// <summary>
        /// Returns the registered display name for <paramref name="spell"/>, or null if none is registered.
        /// </summary>
        public static string GetDisplayName(SpellName spell)
        {
            return _names.TryGetValue(spell, out var name) ? name : null;
        }

        /// <summary>
        /// Returns the registered display name for <paramref name="spell"/> if one exists,
        /// otherwise falls back to <c>spell.ToString()</c>.
        /// </summary>
        public static string GetDisplayNameOrDefault(SpellName spell)
        {
            return _names.TryGetValue(spell, out var name) ? name : spell.ToString();
        }

        /// <summary>
        /// Removes the registered display name for <paramref name="spell"/>.
        /// </summary>
        public static void Unregister(SpellName spell)
        {
            _names.Remove(spell);
        }
    }
}
