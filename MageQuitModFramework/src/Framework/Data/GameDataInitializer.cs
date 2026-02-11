using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using MageQuitModFramework.Spells;
using MageQuitModFramework.Utilities;

namespace MageQuitModFramework.Data
{
    [HarmonyPatch(typeof(SpellManager), "Awake")]
    public static class GameDataInitializer
    {
        public static Dictionary<SpellName, Dictionary<string, float>> DefaultClassAttributes { get; private set; } = [];
        public static Dictionary<SpellName, Spell> DefaultSpellTable { get; private set; } = [];
        public static bool IsLoaded { get; private set; } = false;

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

            SpellModificationSystem.Initialize(DefaultSpellTable, DefaultClassAttributes);

            FrameworkPlugin.Log.LogInfo("Game data loaded successfully");
            OnGameDataLoaded?.Invoke();
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
