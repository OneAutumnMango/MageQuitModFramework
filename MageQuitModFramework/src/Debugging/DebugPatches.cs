// Suppress missing XML comment warnings for this file
#pragma warning disable 1591

using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace MageQuitModFramework.Debugging
{
    [HarmonyPatch]
    public static class DebugPatches
    {
        private static void LogInstantiatedObject(Object obj)
        {
            if (obj is GameObject go)
            {
                FrameworkPlugin.Log?.LogInfo($"[Instantiate] GameObject: '{go.name}' (active: {go.activeSelf}, layer: {go.layer}, children: {go.transform.childCount})");
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    FrameworkPlugin.Log?.LogInfo($"  Renderer: '{renderer.name}' enabled={renderer.enabled} material={renderer.material?.name}");
                }
            }
            else
            {
                FrameworkPlugin.Log?.LogInfo($"[Instantiate] Object: '{obj?.GetType().Name}'");
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), [typeof(Object)])]
        [HarmonyPostfix]
        static void LogObjectOfArgs_Object(Object __result, Object __0)
        {
            FrameworkPlugin.Log?.LogInfo($"[Instantiate] Input: '{__0?.GetType().Name}' name='{(__0 is GameObject go ? go.name : __0?.ToString())}'");
            LogInstantiatedObject(__result);
        }


        [HarmonyPatch(typeof(Object), nameof(Object.Instantiate), [typeof(Object), typeof(Vector3), typeof(Quaternion)])]
        [HarmonyPostfix]
        static void LogObjectOfArgs_Object_Vector3_Quaternion(Object __result, Object __0, Vector3 __1, Quaternion __2)
        {
            FrameworkPlugin.Log?.LogInfo($"[Instantiate] Input: '{__0?.GetType().Name}' name='{(__0 is GameObject go ? go.name : __0?.ToString())}' pos={__1} rot={__2.eulerAngles}");
            LogInstantiatedObject(__result);
        }

    }

    // Show damage hitboxes
    [HarmonyPatch]
    public static class Patch_GetAllInSphere_Debug
    {
        [HarmonyPatch(typeof(GameUtility), nameof(GameUtility.GetAllInSphere))]
        [HarmonyPrefix]
        static void Prefix(Vector3 center, float radius)
        {
            DrawDebugSphere(center, radius);
        }

        private static void DrawDebugSphere(Vector3 pos, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * radius * 2f;

            var col = go.GetComponent<Collider>();
            if (col) col.enabled = false;

            var mr = go.GetComponent<MeshRenderer>();
            mr.material       = new Material(Shader.Find("Sprites/Default"));
            mr.material.color = new Color(1f, 0f, 0f, 0.25f);

            Object.Destroy(go, 0.1f);
        }
    }

    // Log damage and healing
    [HarmonyPatch]
    public static class Patch_WizardStatus_rpcApplyDamageHealing
    {
        [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyDamage))]
        [HarmonyPrefix]
        static void DamagePrefix(WizardStatus __instance, float damage, int owner, int source)
        {
            var idField = typeof(WizardStatus).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            var idValue = idField?.GetValue(__instance);

            int wizardOwner = -1;
            if (idValue != null)
            {
                var ownerField = idValue.GetType().GetField("owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ownerField != null)
                    wizardOwner = (int)ownerField.GetValue(idValue);
            }

            FrameworkPlugin.Log?.LogInfo($"[Damage Log] Wizard {wizardOwner} is about to take {damage} damage from {owner}, source {source}");
        }

        [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyDamage))]
        [HarmonyPostfix]
        static void DamagePostfix(WizardStatus __instance, float damage, int owner, int source)
        {
            FrameworkPlugin.Log?.LogInfo($"[Damage Log] Wizard's remaining health: {__instance.health}, damage taken: {damage}");
        }

        [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyHealing))]
        [HarmonyPrefix]
        static void HealingPrefix(WizardStatus __instance, float healing, int owner)
        {
            var idField = typeof(WizardStatus).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic);
            var idValue = idField?.GetValue(__instance);

            int wizardOwner = -1;
            if (idValue != null)
            {
                var ownerField = idValue.GetType().GetField("owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ownerField != null)
                    wizardOwner = (int)ownerField.GetValue(idValue);
            }

            FrameworkPlugin.Log?.LogInfo($"[Healing Log] Wizard {wizardOwner} is about to heal {healing} health from {owner}");
        }

        [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyHealing))]
        [HarmonyPostfix]
        static void HealingPostfix(WizardStatus __instance, float healing, int owner)
        {
            FrameworkPlugin.Log?.LogInfo($"[Healing Log] Wizard's current health: {__instance.health}, healing applied: {healing}");
        }
    }
}
#pragma warning restore 1591
