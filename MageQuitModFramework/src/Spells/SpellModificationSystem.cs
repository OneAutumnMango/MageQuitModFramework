using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace MageQuitModFramework.Spells
{
    /// <summary>
    /// Holds a spell modification table and provides methods to manipulate and apply modifiers.
    /// </summary>
    public class SpellModifierTable
    {
        /// <summary>
        /// Dictionary mapping spell names to their modifiable attributes.
        /// </summary>
        public Dictionary<SpellName, SpellModifiers> Modifiers { get; private set; } = [];

        /// <summary>
        /// Initializes the table with default game values.
        /// </summary>
        /// <param name="defaultSpellTable">Default spell attributes.</param>
        /// <param name="defaultClassAttributes">Default class-specific attributes.</param>
        public void Initialize(Dictionary<SpellName, Spell> defaultSpellTable, Dictionary<SpellName, Dictionary<string, float>> defaultClassAttributes)
        {
            Modifiers.Clear();

            foreach (SpellName name in defaultSpellTable.Keys)
            {
                var spell = defaultSpellTable[name];
                var classAttrs = defaultClassAttributes.ContainsKey(name) ? defaultClassAttributes[name] : new Dictionary<string, float>();

                float GetAttr(string key) => classAttrs.TryGetValue(key, out var val) ? val : 0f;

                var mods = new SpellModifiers
                {
                    DAMAGE          = new AttributeModifier(GetAttr("DAMAGE")),
                    RADIUS          = new AttributeModifier(GetAttr("RADIUS")),
                    POWER           = new AttributeModifier(GetAttr("POWER")),
                    Y_POWER         = new AttributeModifier(GetAttr("Y_POWER")),
                    cooldown        = new AttributeModifier(spell.cooldown),
                    windUp          = new AttributeModifier(spell.windUp),
                    windDown        = new AttributeModifier(spell.windDown),
                    initialVelocity = new AttributeModifier(spell.initialVelocity),
                    HEAL = name == SpellName.FrogOfLife ? new AttributeModifier(15f) : new AttributeModifier(0)
                };

                Modifiers[name] = mods;
            }
        }

        /// <summary>
        /// Sets the base value of a spell attribute directly.
        /// </summary>
        /// <param name="name">The spell to modify</param>
        /// <param name="attribute">Attribute name (e.g., "DAMAGE", "cooldown")</param>
        /// <param name="baseValue">The new base value to set</param>
        /// <returns>True if successful, false if spell or attribute not found</returns>
        public bool TrySetBase(SpellName name, string attribute, float baseValue)
        {
            if (!Modifiers.TryGetValue(name, out var mods))
                return false;

            if (mods.TryGetModifier(attribute, out var modifier))
            {
                modifier.Base = baseValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates a spell attribute by adding to its multiplier (additive change).
        /// </summary>
        /// <param name="name">The spell to modify</param>
        /// <param name="attribute">Attribute name (e.g., "DAMAGE", "cooldown")</param>
        /// <param name="additiveMult">Value to add to multiplier (0.5 = +50%)</param>
        /// <returns>True if successful, false if spell or attribute not found</returns>
        /// <example>
        /// <code>
        /// // Increase Fireball damage by 50% (additive: 1.0 -> 1.5)
        /// SpellModificationSystem.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);
        /// </code>
        /// </example>
        public bool TryAddToModifier(SpellName name, string attribute, float additiveMult)
        {
            if (!Modifiers.TryGetValue(name, out var mods))
                return false;

            if (mods.TryGetModifier(attribute, out var modifier))
            {
                modifier.AddMultiplier(additiveMult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates a spell attribute by multiplying its multiplier (multiplicative change).
        /// </summary>
        /// <param name="name">The spell to modify</param>
        /// <param name="attribute">Attribute name (e.g., "DAMAGE", "cooldown")</param>
        /// <param name="multiplicativeMult">Value to multiply multiplier by (2.0 = double, 0.5 = half)</param>
        /// <returns>True if successful, false if spell or attribute not found</returns>
        /// <example>
        /// <code>
        /// // Double Fireball damage (multiplicative: 1.0 -> 2.0)
        /// SpellModificationSystem.TryMultiplyModifier(SpellName.Fireball, "DAMAGE", 2.0f);
        /// </code>
        /// </example>
        public bool TryMultiplyModifier(SpellName name, string attribute, float multiplicativeMult)
        {
            if (!Modifiers.TryGetValue(name, out var mods))
                return false;

            if (mods.TryGetModifier(attribute, out var modifier))
            {
                modifier.ApplyMultiplier(multiplicativeMult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the AttributeModifier for a specific spell attribute.
        /// </summary>
        /// <param name="name">The spell name</param>
        /// <param name="attribute">Attribute name</param>
        /// <param name="modifier">Output modifier if found</param>
        /// <returns>True if modifier found, false otherwise</returns>
        public bool TryGetModifier(SpellName name, string attribute, out AttributeModifier modifier)
        {
            modifier = null;
            if (!Modifiers.TryGetValue(name, out var mods))
                return false;

            return mods.TryGetModifier(attribute, out modifier);
        }

        /// <summary>
        /// Gets the current multiplier value for a spell attribute.
        /// </summary>
        /// <param name="name">The spell name</param>
        /// <param name="attribute">Attribute name</param>
        /// <param name="mult">Output multiplier if found</param>
        /// <returns>True if multiplier found, false otherwise</returns>
        public bool TryGetMultiplier(SpellName name, string attribute, out float mult)
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
        /// Resets all multipliers to 1.0 while preserving base values.
        /// </summary>
        public void ResetAllMultipliers()
        {
            foreach (var mods in Modifiers.Values)
            {
                mods.ResetAllMultipliers();
            }
        }

        /// <summary>
        /// Creates a deep copy of this table.
        /// </summary>
        public SpellModifierTable Copy()
        {
            var copy = new SpellModifierTable();
            foreach (var kvp in Modifiers)
            {
                copy.Modifiers[kvp.Key] = kvp.Value.Copy();
            }
            return copy;
        }

        /// <summary>
        /// Applies this table's modifiers to the game.
        /// </summary>
        public void ApplyToGame()
        {
            var spellManager = Globals.spell_manager;
            var player = SpellModificationSystem.GetLocalPlayer();

            if (spellManager != null)
            {
                ApplyModifiersToSpellTable(spellManager);
            }

            if (player != null)
            {
                ApplyModifiersToPlayer(player);
            }
        }

        private void ApplyModifiersToSpellTable(SpellManager spellManager)
        {
            foreach (var kvp in Modifiers)
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

        private void ApplyModifiersToPlayer(Player player)
        {
            foreach (var kvp in player.cooldowns)
            {
                SpellName name = kvp.Key;
                Cooldown playerCooldown = kvp.Value;

                if (Modifiers.TryGetValue(name, out SpellModifiers mods))
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
    }

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

        private static SpellModifierTable _defaultTable;
        private static Dictionary<string, SpellModifierTable> _namedTables = [];
        private static string _loadedTableKey = "default";

        /// <summary>
        /// Gets the immutable default spell modifier table.
        /// </summary>
        public static SpellModifierTable Default() => _defaultTable;

        /// <summary>
        /// Gets the key of the currently loaded spell modifier table.
        /// </summary>
        public static string LoadedTableKey => _loadedTableKey;

        /// <summary>
        /// Initializes the default table with game data.
        /// Called automatically by GameDataInitializer.
        /// </summary>
        /// <param name="defaultSpellTable">Default spell table from SpellManager</param>
        /// <param name="defaultClassAttributes">Default class-specific attribute values</param>
        public static void InitializeDefaultTable(Dictionary<SpellName, Spell> defaultSpellTable, Dictionary<SpellName, Dictionary<string, float>> defaultClassAttributes)
        {
            _defaultTable = new SpellModifierTable();
            _defaultTable.Initialize(defaultSpellTable, defaultClassAttributes);
            FrameworkPlugin.Log?.LogInfo($"Initialized default spell modifier table ({_defaultTable.Modifiers.Count} spells)");
            _namedTables["default"] = _defaultTable;
        }

        /// <summary>
        /// Registers a named spell modifier table.
        /// </summary>
        /// <param name="key">Unique identifier for the table</param>
        /// <param name="table">The spell modifier table to register</param>
        public static void RegisterTable(string key, SpellModifierTable table)
        {
            _namedTables[key] = table;
            FrameworkPlugin.Log?.LogInfo($"Registered spell modifier table: '{key}'");
        }

        /// <summary>
        /// Registers a named spell modifier table initialized as a copy of the default table,
        /// or returns an existing instance if one is already registered.
        /// </summary>
        /// <param name="key">Unique identifier for the table</param>
        /// <remarks>
        /// This overload automatically creates a copy of the default table for the registered table.
        /// Use this when you want to start with default values and apply modifications.
        /// </remarks>
        /// <returns>
        /// The registered spell modifier table for the given key: either an existing instance if present, or a new copy of the default table.
        /// </returns>
        public static SpellModifierTable RegisterTable(string key)
        {
            if (_namedTables.TryGetValue(key, out var table))
                return table;
            var copy = _defaultTable.Copy();
            RegisterTable(key, copy);
            return copy;
        }


        /// <summary>
        /// Gets a registered spell modifier table by key.
        /// </summary>
        /// <param name="key">The table identifier</param>
        /// <returns>The registered table, or null if not found</returns>
        public static SpellModifierTable GetTable(string key)
        {
            return _namedTables.TryGetValue(key, out var table) ? table : null;
        }

        /// <summary>
        /// Loads and applies a registered spell modifier table to the game if it exists.
        /// </summary>
        /// <param name="key">The table identifier to load</param>
        public static bool Load(string key)
        {
            if (!_namedTables.TryGetValue(key, out var table))
            {
                FrameworkPlugin.Log?.LogWarning($"SpellModificationSystem: No table found for key '{key}'");
                return false;
            }

            _loadedTableKey = key;
            table.ApplyToGame();
            FrameworkPlugin.Log?.LogInfo($"Applied spell modifier table: '{key}'");
            return true;
        }

        /// <summary>
        /// Loads and applies the default spell modifier table to the game.
        /// </summary>
        /// <returns>True if default table is initialized and applied, false otherwise</returns>
        public static bool LoadDefault()
        {
            if (_defaultTable == null)
            {
                FrameworkPlugin.Log?.LogWarning("SpellModificationSystem: Default table not initialized");
                return false;
            }

            _loadedTableKey = "default";
            _defaultTable.ApplyToGame();
            FrameworkPlugin.Log?.LogInfo("Applied default spell modifier table");
            return true;
        }

        /// <summary>
        /// Removes a registered spell modifier table.
        /// </summary>
        /// <param name="key">The table identifier to remove</param>
        public static void ClearTable(string key)
        {
            if (_namedTables.Remove(key))
            {
                FrameworkPlugin.Log?.LogInfo($"Cleared spell modifier table: '{key}'");
            }
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
                SpellName.RockBlock    => "StonewallObject",
                SpellName.FlameLeash   => "BurningLeashObject",
                SpellName.SomerAssault => "SomAssaultObject",
                SpellName.Sustain      => "SustainObjectObject",
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
                "StonewallObject"     => SpellName.RockBlock,
                "BurningLeashObject"  => SpellName.FlameLeash,
                "SomAssaultObject"    => SpellName.SomerAssault,
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
        /// Applies Harmony patches to the specified method of all SpellObject types (eg. Init).
        /// Useful for modifying spell behavior globally at initialization time.
        /// </summary>
        /// <param name="harmony">The Harmony instance to use for patching</param>
        /// <param name="methodName">The name of the method to patch</param>
        /// <param name="prefixMethod">Optional prefix method to apply before the target method</param>
        /// <param name="postfixMethod">Optional postfix method to apply after the target method</param>
        public static void PatchAllSpellObjects(Harmony harmony, string methodName, MethodInfo prefixMethod = null, MethodInfo postfixMethod = null)
        {
            foreach (SpellName name in Enum.GetValues(typeof(SpellName)))
            {
                string fullTypeName = GetSpellObjectTypeName(name);

                Type spellType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(fullTypeName))
                    .FirstOrDefault(t => t != null);

                if (spellType == null)
                    continue;

                MethodInfo method = spellType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    continue;

                HarmonyMethod prefix = prefixMethod != null ? new HarmonyMethod(prefixMethod) : null;
                HarmonyMethod postfix = postfixMethod != null ? new HarmonyMethod(postfixMethod) : null;

                harmony.Patch(method, prefix: prefix, postfix: postfix);
            }
        }
    }
}
