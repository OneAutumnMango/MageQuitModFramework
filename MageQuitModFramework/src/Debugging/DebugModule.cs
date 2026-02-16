// Suppress missing XML comment warnings for this file
#pragma warning disable 1591

using HarmonyLib;
using MageQuitModFramework.Modding;
using MageQuitModFramework.Spells;

namespace MageQuitModFramework.Debugging
{
    public class InstantiateLogModule : BaseModule
    {
        public override string ModuleName => "Object Instantiation Logging";

        protected override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll(typeof(DebugPatches));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }

    public class HitboxModule : BaseModule
    {
        public override string ModuleName => "Hitbox Display";

        protected override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll(typeof(Patch_GetAllInSphere_Debug));
            SpellModificationSystem.PatchAllSpellObjects(
                harmony, "FixedUpdate",
                postfixMethod: AccessTools.Method(typeof(Patch_GetAllInSphere_Debug), nameof(Patch_GetAllInSphere_Debug.ShowSpellHitbox)));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }

    public class DamageHealingLogModule : BaseModule
    {
        public override string ModuleName => "Damage and Heal Logging";

        protected override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll(typeof(Patch_WizardStatus_rpcApplyDamageHealing));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
#pragma warning restore 1591
