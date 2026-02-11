using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MageQuitModFramework.Utilities
{
    public static class GameModificationHelpers
    {
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

        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var fieldRef = AccessTools.FieldRefAccess<T>(instance.GetType(), fieldName);
            fieldRef(instance) = value;
        }

        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            var fieldRef = AccessTools.FieldRefAccess<T>(instance.GetType(), fieldName);
            return fieldRef(instance);
        }

        public static void SetMultipleFields(object instance, Dictionary<string, object> fieldValues)
        {
            foreach (var kvp in fieldValues)
            {
                AccessTools.Field(instance.GetType(), kvp.Key)?.SetValue(instance, kvp.Value);
            }
        }

        public static Player GetLocalPlayer()
        {
            return PlayerManager.players.Values.FirstOrDefault(p => p.localPlayerNumber >= 0);
        }

        public static bool IsGameDataReady()
        {
            return Globals.spell_manager != null && Globals.spell_manager.spell_table != null;
        }

        public static void ModifySpellTableEntry(SpellManager manager, SpellName spellName, Action<Spell> modifier)
        {
            if (manager?.spell_table == null)
                return;

            if (manager.spell_table.TryGetValue(spellName, out Spell spell))
            {
                modifier(spell);
            }
        }

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
