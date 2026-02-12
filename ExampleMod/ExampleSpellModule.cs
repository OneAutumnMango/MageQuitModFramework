using System;
using BepInEx.Logging;
using HarmonyLib;
using MageQuitModFramework;
using MageQuitModFramework.Modding;
using MageQuitModFramework.Utilities;

namespace ExampleMod.Modules
{
    /// <summary>
    /// Example module that modifies spell properties to demonstrate the module system.
    /// </summary>
    public class ExampleSpellModule : BaseModule
    {
        private static ManualLogSource _log;

        public override string ModuleName => "Example Spell Modifications";

        public ExampleSpellModule(ManualLogSource log)
        {
            _log = log;
        }

        protected override void OnLoad(Harmony harmony)
        {
            _log.LogInfo("Loading spell modification module...");
            harmony.PatchAll(typeof(SpellModificationPatches));
            _log.LogInfo("Spell modification patches applied");
        }

        protected override void OnUnload(Harmony harmony)
        {
            _log.LogInfo("Unloading spell modification module...");
            harmony.UnpatchSelf();
            _log.LogInfo("Spell modification patches removed");
        }

        /// <summary>
        /// Contains Harmony patches for spell modifications.
        /// </summary>
        private static class SpellModificationPatches
        {
            [HarmonyPatch(typeof(SpellManager), "Awake")]
            [HarmonyPostfix]
            public static void SpellManager_Awake_Postfix(SpellManager __instance)
            {
                _log.LogInfo("SpellManager awakened - modifying spells...");

                // Example: Make fireball faster
                GameModificationHelpers.ModifySpellTableEntry(__instance, SpellName.Fireball, spell =>
                {
                    spell.cooldown *= 0.5f;  // 50% faster cooldown
                    spell.initialVelocity *= 2f;  // 100% faster projectile
                    _log.LogInfo($"Modified Fireball: cooldown={spell.cooldown}, velocity={spell.initialVelocity}");
                });

                // Example: Access private fields, change AI draft weights
                var draftWeights = GameModificationHelpers.GetPrivateField<int[]>(__instance, "ai_draft_weights");
                if (draftWeights != null && draftWeights.Length > 0)
                {
                    draftWeights[0] = Math.Max(1, draftWeights[0] / 2);
                    GameModificationHelpers.SetPrivateField(__instance, "ai_draft_weights", draftWeights);
                    _log.LogInfo($"Updated ai_draft_weights[0] to {draftWeights[0]}");
                }
            }
        }
    }
}
