using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MageQuitModFramework.Core
{
    public static class SpellModificationSystem
    {
        public static readonly string[] ClassAttributeKeys = ["DAMAGE", "RADIUS", "POWER", "Y_POWER"];
        public static readonly string[] SpellTableKeys = ["cooldown", "windUp", "windDown", "initialVelocity"];
        public static readonly string[] CustomKeys = ["HEAL"];

        public static Dictionary<SpellName, SpellModifiers> SpellModifierTable { get; private set; } = [];

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

        public static bool TryGetModifier(SpellName name, string attribute, out AttributeModifier modifier)
        {
            modifier = null;
            if (!SpellModifierTable.TryGetValue(name, out var mods))
                return false;

            return mods.TryGetModifier(attribute, out modifier);
        }

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
                        var subspell = Loading.GameDataInitializer.DefaultSpellTable[name].additionalCasts[i];  // SpellMod table doesnt store additional casts
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
                        var subspell = Loading.GameDataInitializer.DefaultSpellTable[name].additionalCasts[i];  // SpellMod table doesnt store additional casts
                        playerCooldown.subCooldowns[i].cooldown = subspell.cooldown * mods.cooldown.Mult;
                    }
                }
            }
        }

        public static void ApplyModifiersToGame(SpellManager spellManager, Player player)
        {
            ApplyModifiersToSpellTable(spellManager);
            ApplyModifiersToPlayer(player);
        }

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
    }
}
