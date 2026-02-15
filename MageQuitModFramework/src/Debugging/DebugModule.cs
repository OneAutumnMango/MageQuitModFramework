// Suppress missing XML comment warnings for this file
#pragma warning disable 1591

using HarmonyLib;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.Debugging
{
    public class InstantiateLogModule : BaseModule
    {
        public override string ModuleName => "Object Instantiation Logging";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(DebugPatches));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }

    public class DamageHitboxModule : BaseModule
    {
        public override string ModuleName => "Damage Hitboxes";

        protected override void OnLoad(Harmony harmony)
        {
            PatchGroup(harmony, typeof(Patch_GetAllInSphere_Debug));
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
            PatchGroup(harmony, typeof(Patch_WizardStatus_rpcApplyDamageHealing));
        }

        protected override void OnUnload(Harmony harmony)
        {
            harmony.UnpatchSelf();
        }
    }
}
#pragma warning restore 1591
