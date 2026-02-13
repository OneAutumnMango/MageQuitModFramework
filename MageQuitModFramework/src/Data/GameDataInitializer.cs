using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using MageQuitModFramework.Spells;
using MageQuitModFramework.Utilities;

namespace MageQuitModFramework.Data
{
    /// <summary>
    /// Initializes and caches game data when the SpellManager loads.
    /// Provides access to default spell configurations and fires events when data is ready.
    /// </summary>
    /// <remarks>
    /// This class uses a Harmony patch on SpellManager.Awake to detect when game data becomes available.
    /// Since game data isn't loaded at plugin startup, mods should subscribe to <see cref="OnGameDataLoaded"/>
    /// or check <see cref="IsLoaded"/> before accessing spell information.
    /// </remarks>
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    public static class GameDataInitializer
    {
        /// <summary>
        /// Default attribute values for spell class fields (DAMAGE, RADIUS, POWER, Y_POWER).
        /// Keyed by spell name, then attribute name.
        /// </summary>
        public static Dictionary<SpellName, Dictionary<string, float>> DefaultClassAttributes { get; private set; } = [];

        /// <summary>
        /// Default spell table entries from the game's SpellManager.
        /// Contains baseline cooldown, windUp, windDown, and other spell properties.
        /// </summary>
        public static Dictionary<SpellName, Spell> DefaultSpellTable { get; private set; } = [];

        /// <summary>
        /// Whether game data has been loaded and is available for use.
        /// </summary>
        public static bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Event fired when game data has been successfully loaded and cached.
        /// Subscribe to this event to safely access spell data that isn't available at plugin startup.
        /// </summary>
        /// <example>
        /// <code>
        /// GameDataInitializer.OnGameDataLoaded += () =>
        /// {
        ///     // Safe to register spell modifiers here
        ///     SpellModificationSystem.RegisterModifier(...);
        /// };
        /// </code>
        /// </example>
        public static event Action OnGameDataLoaded;

        static void Postfix(SpellManager __instance)
        {
            SpellManager mgr = __instance ?? Globals.spell_manager;
            if (mgr == null || mgr.spell_table == null)
                return;

            FrameworkPlugin.Log.LogInfo("Loading game data...");
            IsLoaded = true;

            DefaultSpellTable = mgr.spell_table.ToDictionary(kvp => kvp.Key, kvp => new Spell(kvp.Value));

            PopulateDefaultClassAttributes();

            SpellModificationSystem.InitializeDefaultTable(DefaultSpellTable, DefaultClassAttributes);

            FrameworkPlugin.Log.LogInfo("Game data loaded successfully");
            OnGameDataLoaded?.Invoke();
            FrameworkPlugin.Log.LogInfo("Fired OnGameDataLoaded event");
        }

        private static void PopulateDefaultClassAttributes()
        {
            string[] attributeFields = ["DAMAGE", "RADIUS", "POWER", "Y_POWER"];

            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                string typeName = SpellModificationSystem.GetSpellObjectTypeName(name);

                Type spellType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(typeName, false))
                    .FirstOrDefault(t => t != null);

                if (spellType == null)
                    continue;

                var instance = Activator.CreateInstance(spellType) as SpellObject;
                var values = new Dictionary<string, float>();

                foreach (var fieldName in attributeFields)
                {
                    try
                    {
                        float original = GameModificationHelpers.GetPrivateField<float>(instance, fieldName);
                        values[fieldName] = original;
                    }
                    catch { }
                }

                if (values.Count > 0)
                    DefaultClassAttributes[name] = values;
            }
        }
    }
}
