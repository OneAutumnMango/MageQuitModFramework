using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace MageQuitModFramework.Spells
{
    /// <summary>
    /// Central system for managing spell attribute modifications using Base × Mult pattern.
    /// </summary>
    public static class SpellModificationSystem
    {
        /// <summary>
        /// Spell object class attributes (DAMAGE, RADIUS, POWER, Y_POWER).
        /// </summary>
        public static readonly string[] ClassAttributeKeys = ["DAMAGE", "RADIUS", "POWER", "Y_POWER"];
        
        /// <summary>
        /// Spell table attributes (cooldown, windUp, windDown, initialVelocity).
        /// </summary>
        public static readonly string[] SpellTableKeys = ["cooldown", "windUp", "windDown", "initialVelocity"];
        
        /// <summary>
        /// Custom attributes not in class or table (HEAL).
        /// </summary>
        public static readonly string[] CustomKeys = ["HEAL"];

        /// <summary>
        /// Dictionary mapping spell names to their modifiable attributes.
        /// </summary>
        public static Dictionary<SpellName, SpellModifiers> SpellModifierTable { get; private set; } = [];

        /// <summary>
        /// Initializes the modification system with default game values.
        /// Called automatically by GameDataInitializer.
        /// </summary>
        /// <param name="defaultSpellTable">Original spell table from SpellManager</param>
        /// <param name="defaultClassAttributes">Original class attributes from spell objects</param>
        public static void Initialize(Dictionary<SpellName, Spell> defaultSpellTable, Dictionary<SpellName, Dictionary<string, float>> defaultClassAttributes)
        {
            SpellModifierTable.Clear();

            foreach (SpellName name in defaultSpellTable.Keys)
            {
                var spell = defaultSpellTable[name];
                var classAttrs = defaultClassAttributes.ContainsKey(name) ? defaultClassAttributes[name] : new Dictionary<string, float>();

                var mods = new SpellModifiers
                {
                    DAMAGE          = new AttributeModifier(classAttrs["DAMAGE"]),
                    RADIUS          = new AttributeModifier(classAttrs["RADIUS"]),
                    POWER           = new AttributeModifier(classAttrs["POWER"]),
                    Y_POWER         = new AttributeModifier(classAttrs["Y_POWER"]),
                    cooldown        = new AttributeModifier(spell.cooldown),
                    windUp          = new AttributeModifier(spell.windUp),
                    windDown        = new AttributeModifier(spell.windDown),
                    initialVelocity = new AttributeModifier(spell.initialVelocity),
                    HEAL = name == SpellName.FrogOfLife ? new AttributeModifier(15f) : new AttributeModifier(0)
                };

                SpellModifierTable[name] = mods;
            }
        }

        /// <summary>
        /// Updates a spell attribute by adding to its multiplier.
        /// </summary>
        /// <param name="name">The spell to modify</param>
        /// <param name="attribute">Attribute name (e.g., "DAMAGE", "cooldown")</param>
        /// <param name="additiveMult">Value to add to multiplier (0.5 = +50%)</param>
        /// <returns>True if successful, false if spell or attribute not found</returns>
        /// <example>
        /// <code>
        /// // Increase Fireball damage by 50%
        /// SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", 0.5f);
        /// </code>
        /// </example>
        public static bool TryUpdateModifier(SpellName name, string attribute, float additiveMult)
        {
            if (!SpellModifierTable.TryGetValue(name, out var mods))
                return false;

            if (mods.TryGetModifier(attribute, out var modifier))
            {
                modifier.AddMultiplier(additiveMult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the modifier for a specific spell attribute.
        /// </summary>
        /// <param name="name">The spell name</param>
        /// <param name="attribute">Attribute name</param>
        /// <param name="modifier">Output modifier if found</param>
        /// <returns>True if modifier found, false otherwise</returns>
        public static bool TryGetModifier(SpellName name, string attribute, out AttributeModifier modifier)
        {
            modifier = null;
            if (!SpellModifierTable.TryGetValue(name, out var mods))
                return false;

            return mods.TryGetModifier(attribute, out modifier);
        }

        /// <summary>
        /// Retrieves the default (base) value for a spell attribute before modifiers are applied.
        /// </summary>
        /// <param name="name">The spell to query</param>
        /// <param name="attribute">The attribute name</param>
        /// <param name="value">Output parameter for the base value if found</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
        public static bool TryGetDefaultValue(SpellName name, string attribute, out float value)
        {
            value = 0;
            if (TryGetModifier(name, attribute, out var modifier))
            {
                value = modifier.Base;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the current multiplier for a spell attribute.
        /// </summary>
        /// <param name="name">The spell to query</param>
        /// <param name="attribute">The attribute name</param>
        /// <param name="mult">Output parameter for the multiplier if found (default 1.0)</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
        public static bool TryGetMultiplier(SpellName name, string attribute, out float mult)
        {
            mult = 1f;
            if (TryGetModifier(name, attribute, out var modifier))
            {
                mult = modifier.Mult;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets all multipliers across all spells to 1.0, keeping base values intact.
        /// </summary>
        public static void ResetAllMultipliers()
        {
            foreach (var mods in SpellModifierTable.Values)
            {
                mods.ResetAllMultipliers();
            }
        }
        private static void ApplyModifiersToSpellTable(SpellManager spellManager)
        {
            foreach (var kvp in SpellModifierTable)
            {
                SpellName name = kvp.Key;
                SpellModifiers mods = kvp.Value;

                if (spellManager.spell_table.TryGetValue(name, out Spell spell))
                {
                    spell.cooldown        = mods.cooldown;
                    spell.windUp          = mods.windUp;
                    spell.windDown        = mods.windDown;
                    spell.initialVelocity = mods.initialVelocity;

                    if (spell.additionalCasts == null)
                        continue;

                    for (int i = 0; i < spell.additionalCasts.Length; i++)
                    {
                        var subspell = Data.GameDataInitializer.DefaultSpellTable[name].additionalCasts[i];  // SpellMod table doesnt store additional casts
                        spell.additionalCasts[i].cooldown        = subspell.cooldown        * mods.cooldown.Mult;
                        spell.additionalCasts[i].initialVelocity = subspell.initialVelocity * mods.initialVelocity.Mult;
                    }
                }
            }
        }

        private static void ApplyModifiersToPlayer(Player player)
        {
            foreach (var kvp in player.cooldowns)
            {
                SpellName name = kvp.Key;
                Cooldown playerCooldown = kvp.Value;

                if (SpellModifierTable.TryGetValue(name, out SpellModifiers mods))
                {
                    playerCooldown.cooldown = mods.cooldown;

                    if (playerCooldown.subCooldowns == null)
                        continue;

                    for (int i = 0; i < playerCooldown.subCooldowns.Length; i++)
                    {
                        var subspell = Data.GameDataInitializer.DefaultSpellTable[name].additionalCasts[i];  // SpellMod table doesnt store additional casts
                        playerCooldown.subCooldowns[i].cooldown = subspell.cooldown * mods.cooldown.Mult;
                    }
                }
            }
        }

        /// <summary>
        /// Applies all registered modifiers to the game's spell table and player cooldowns.
        /// </summary>
        /// <param name="spellManager">The game's spell manager instance</param>
        /// <param name="player">The player to apply cooldown modifiers to</param>
        public static void ApplyModifiersToGame(SpellManager spellManager, Player player)
        {
            ApplyModifiersToSpellTable(spellManager);
            ApplyModifiersToPlayer(player);
        }

        /// <summary>
        /// Gets the fully qualified type name for a spell's SpellObject class.
        /// </summary>
        /// <param name="name">The spell name</param>
        /// <returns>The type name string for reflection-based access</returns>
        public static string GetSpellObjectTypeName(SpellName name)
        {
            return name switch
            {
                SpellName.RockBlock => "StonewallObject",
                SpellName.FlameLeash => "BurningLeashObject",
                SpellName.SomerAssault => "SomAssaultObject",
                SpellName.Sustain => "SustainObjectObject",
                _ => $"{name}Object"
            };
        }

        /// <summary>
        /// Converts a SpellObject type name back to its SpellName enum value.
        /// </summary>
        /// <param name="typeName">The type name (e.g., "FireBallObject")</param>
        /// <returns>The corresponding SpellName, or default if not found</returns>
        public static SpellName? GetSpellNameFromTypeName(string typeName)
        {
            return typeName switch
            {
                "StonewallObject" => SpellName.RockBlock,
                "BurningLeashObject" => SpellName.FlameLeash,
                "SomAssaultObject" => SpellName.SomerAssault,
                "SustainObjectObject" => SpellName.Sustain,
                _ => Enum.GetValues(typeof(SpellName))
                    .Cast<SpellName?>()
                    .FirstOrDefault(name => $"{name}Object" == typeName)
            };
        }

        /// <summary>
        /// Gets the local player instance from the PlayerManager.
        /// </summary>
        /// <returns>The local Player object, or null if no local player exists</returns>
        public static Player GetLocalPlayer()
        {
            return PlayerManager.players.Values.FirstOrDefault(p => p.localPlayerNumber >= 0);
        }

        /// <summary>
        /// Checks if game data (specifically the spell manager and spell table) is ready for access.
        /// </summary>
        /// <returns>True if game data is loaded and accessible, false otherwise</returns>
        public static bool IsGameDataReady()
        {
            return Globals.spell_manager != null && Globals.spell_manager.spell_table != null;
        }

        /// <summary>
        /// Modifies a single spell table entry using a callback function.
        /// </summary>
        /// <param name="manager">The SpellManager instance containing the spell table</param>
        /// <param name="spellName">The spell to modify</param>
        /// <param name="modifier">Callback that receives the Spell object for modification</param>
        public static void ModifySpellTableEntry(SpellManager manager, SpellName spellName, Action<Spell> modifier)
        {
            if (manager?.spell_table == null)
                return;

            if (manager.spell_table.TryGetValue(spellName, out Spell spell))
            {
                modifier(spell);
            }
        }

        /// <summary>
        /// Applies a modification callback to all spells in the spell table.
        /// </summary>
        /// <param name="manager">The SpellManager instance containing the spell table</param>
        /// <param name="modifier">Callback that receives each Spell object for modification</param>
        public static void ModifyAllSpells(SpellManager manager, Action<Spell> modifier)
        {
            if (manager?.spell_table == null)
                return;

            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                if (manager.spell_table.TryGetValue(name, out Spell spell))
                {
                    modifier(spell);
                }
            }
        }

        /// <summary>
        /// Modifies specific float fields across all spells using a transformation function.
        /// </summary>
        /// <param name="manager">The SpellManager instance containing the spell table</param>
        /// <param name="fieldNames">Array of field names to modify (e.g., "cooldown", "windUp")</param>
        /// <param name="transform">Function that takes (spell, fieldName, originalValue) and returns the new value</param>
        public static void ModifySpellTableValues(SpellManager manager, string[] fieldNames, Func<Spell, string, float, float> transform)
        {
            if (manager?.spell_table == null)
                return;

            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                if (!manager.spell_table.TryGetValue(name, out Spell spell))
                    continue;

                foreach (var fieldName in fieldNames)
                {
                    var field = typeof(Spell).GetField(fieldName);
                    if (field != null && field.FieldType == typeof(float))
                    {
                        float original = (float)field.GetValue(spell);
                        float transformed = transform(spell, fieldName, original);
                        field.SetValue(spell, transformed);
                    }
                }
            }
        }

        /// <summary>
        /// Applies Harmony patches to the Init method of all SpellObject types.
        /// Useful for modifying spell behavior globally at initialization time.
        /// </summary>
        /// <param name="harmony">The Harmony instance to use for patching</param>
        /// <param name="prefixMethod">Optional prefix method to apply before Init</param>
        /// <param name="postfixMethod">Optional postfix method to apply after Init</param>
        public static void PatchAllSpellObjectInit(Harmony harmony, MethodInfo prefixMethod = null, MethodInfo postfixMethod = null)
        {
            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                string fullTypeName = GetSpellObjectTypeName(name);

                Type spellType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(fullTypeName))
                    .FirstOrDefault(t => t != null);

                if (spellType == null)
                    continue;

                MethodInfo initMethod = spellType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (initMethod == null)
                    continue;

                HarmonyMethod prefix = prefixMethod != null ? new HarmonyMethod(prefixMethod) : null;
                HarmonyMethod postfix = postfixMethod != null ? new HarmonyMethod(postfixMethod) : null;

                harmony.Patch(initMethod, prefix: prefix, postfix: postfix);
            }
        }
    }
}
