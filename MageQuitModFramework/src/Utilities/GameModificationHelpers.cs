using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MageQuitModFramework.Utilities
{
    /// <summary>
    /// Helper utilities for common game modification operations including IL transpiling,
    /// reflection-based field access, and spell table manipulation.
    /// </summary>
    public static class GameModificationHelpers
    {
        /// <summary>
        /// Replaces float constant values in IL instructions for Harmony transpilers.
        /// </summary>
        /// <param name="instructions">The original IL instruction sequence</param>
        /// <param name="oldValue">The float constant to find and replace</param>
        /// <param name="newValue">The replacement float constant value</param>
        /// <param name="tolerance">Comparison tolerance for floating point equality (default 1e-6)</param>
        /// <returns>Modified instruction sequence with replaced float constants</returns>
        public static IEnumerable<CodeInstruction> ReplaceFloatConstant(IEnumerable<CodeInstruction> instructions, float oldValue, float newValue, float tolerance = 1e-6f)
        {
            foreach (var instr in instructions)
            {
                if (instr.opcode == OpCodes.Ldc_R4 && instr.operand is float f && Math.Abs(f - oldValue) < tolerance)
                {
                    instr.operand = newValue;
                }
                yield return instr;
            }
        }

        /// <summary>
        /// Replaces integer constant values in IL instructions for Harmony transpilers.
        /// Handles all IL integer constant opcodes including Ldc_I4, Ldc_I4_S, and Ldc_I4_0 through Ldc_I4_8.
        /// </summary>
        /// <param name="instructions">The original IL instruction sequence</param>
        /// <param name="oldValue">The integer constant to find and replace</param>
        /// <param name="newValue">The replacement integer constant value</param>
        /// <returns>Modified instruction sequence with replaced integer constants</returns>
        public static IEnumerable<CodeInstruction> ReplaceIntConstant(IEnumerable<CodeInstruction> instructions, int oldValue, int newValue)
        {
            foreach (var instr in instructions)
            {
                bool matches = false;

                if (instr.opcode == OpCodes.Ldc_I4 && instr.operand is int i && i == oldValue)
                    matches = true;
                else if (instr.opcode == OpCodes.Ldc_I4_S && instr.operand is sbyte sb && sb == oldValue)
                    matches = true;
                else if (oldValue >= 0 && oldValue <= 8)
                {
                    var expectedOpCode = oldValue switch
                    {
                        0 => OpCodes.Ldc_I4_0,
                        1 => OpCodes.Ldc_I4_1,
                        2 => OpCodes.Ldc_I4_2,
                        3 => OpCodes.Ldc_I4_3,
                        4 => OpCodes.Ldc_I4_4,
                        5 => OpCodes.Ldc_I4_5,
                        6 => OpCodes.Ldc_I4_6,
                        7 => OpCodes.Ldc_I4_7,
                        8 => OpCodes.Ldc_I4_8,
                        _ => OpCodes.Nop
                    };
                    if (instr.opcode == expectedOpCode)
                        matches = true;
                }

                if (matches)
                {
                    if (newValue >= 0 && newValue <= 8)
                    {
                        instr.opcode = newValue switch
                        {
                            0 => OpCodes.Ldc_I4_0,
                            1 => OpCodes.Ldc_I4_1,
                            2 => OpCodes.Ldc_I4_2,
                            3 => OpCodes.Ldc_I4_3,
                            4 => OpCodes.Ldc_I4_4,
                            5 => OpCodes.Ldc_I4_5,
                            6 => OpCodes.Ldc_I4_6,
                            7 => OpCodes.Ldc_I4_7,
                            8 => OpCodes.Ldc_I4_8,
                            _ => OpCodes.Ldc_I4
                        };
                        instr.operand = null;
                    }
                    else
                    {
                        instr.opcode = OpCodes.Ldc_I4;
                        instr.operand = newValue;
                    }
                }

                yield return instr;
            }
        }

        /// <summary>
        /// Sets a private field value on an object using reflection.
        /// </summary>
        /// <typeparam name="T">The type of the field value</typeparam>
        /// <param name="instance">The object instance containing the field</param>
        /// <param name="fieldName">The name of the field to set</param>
        /// <param name="value">The value to assign to the field</param>
        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var fieldRef = AccessTools.FieldRefAccess<T>(instance.GetType(), fieldName);
            fieldRef(instance) = value;
        }

        /// <summary>
        /// Gets a private field value from an object using reflection.
        /// </summary>
        /// <typeparam name="T">The type of the field value</typeparam>
        /// <param name="instance">The object instance containing the field</param>
        /// <param name="fieldName">The name of the field to get</param>
        /// <returns>The current value of the field</returns>
        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            var fieldRef = AccessTools.FieldRefAccess<T>(instance.GetType(), fieldName);
            return fieldRef(instance);
        }

        /// <summary>
        /// Sets multiple fields on an object at once using a dictionary of field names to values.
        /// </summary>
        /// <param name="instance">The object instance to modify</param>
        /// <param name="fieldValues">Dictionary mapping field names to their new values</param>
        public static void SetMultipleFields(object instance, Dictionary<string, object> fieldValues)
        {
            foreach (var kvp in fieldValues)
            {
                AccessTools.Field(instance.GetType(), kvp.Key)?.SetValue(instance, kvp.Value);
            }
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
                string fullTypeName = Spells.SpellModificationSystem.GetSpellObjectTypeName(name);

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

        /// <summary>
        /// Applies a dictionary of field values to an object instance using reflection.
        /// Attempts to set both public and non-public instance fields.
        /// </summary>
        /// <param name="instance">The object to modify</param>
        /// <param name="values">Dictionary mapping field names to float values</param>
        public static void ApplyFieldValuesToInstance(object instance, Dictionary<string, float> values)
        {
            if (instance == null || values == null) return;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = instance.GetType();

            foreach (var kvp in values)
            {
                FieldInfo field = type.GetField(kvp.Key, flags);
                field?.SetValue(instance, kvp.Value);
            }
        }
    }
}
