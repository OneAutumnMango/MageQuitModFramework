// Suppress missing XML comment warnings for this file
#pragma warning disable 1591

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MageQuitModFramework.Spells;
using MageQuitModFramework.Utilities;

namespace MageQuitModFramework.Debugging;

[HarmonyPatch]
public static class PlayerUtils
{
    public static bool Invincible { get; private set; } = false;
    public static bool GodMode { get; private set; } = false;
    public const float BaseDamage = 200f;

    public static void KillSelf()
    {
        var player = SpellModificationSystem.GetLocalPlayer();
        if (player == null)
        {
            FrameworkPlugin.Log?.LogInfo("[PlayerKiller] No local player found.");
            return;
        }

        var status = GameUtility.GetWizard(player.playerNumber).GetComponent<WizardStatus>();

        if (status == null)
        {
            FrameworkPlugin.Log?.LogInfo("[PlayerKiller] No wizard status found for the local player.");
            return;
        }

        status.DieRightNow(9999, -1);
    }

    private static void SetHP(float percentageHP)
    {
        var player = SpellModificationSystem.GetLocalPlayer();
        if (player == null)
        {
            FrameworkPlugin.Log?.LogInfo("[SetHP] No local player found.");
            return;
        }

        var status = GameUtility.GetWizard(player.playerNumber).GetComponent<WizardStatus>();

        if (status == null)
        {
            FrameworkPlugin.Log?.LogInfo("[SetHP] No wizard status found for the local player.");
            return;
        }

        status.health = status.maxHealth * percentageHP;
    }

    public static void FullHP()
    {
        SetHP(1f);
    }

    public static void HalfHP()
    {
        SetHP(0.5f);
    }

    public static void QuarterHP()
    {
        SetHP(0.25f);
    }

    public static void ToggleInvincibility()
    {
        Invincible = !Invincible;
    }

    [HarmonyPatch(typeof(WizardStatus), nameof(WizardStatus.rpcApplyDamage))]
    [HarmonyPrefix]
    public static bool Invincibility(WizardController ___wc)
    {
        if (Invincible && ___wc != null && (!___wc.isAI || ___wc.isClone))
            return false;

        return true;
    }

    public static void ToggleGodMode()
    {
        GodMode = !GodMode;
    }

    public static void PatchAll(Harmony harmony)
    {
        MethodInfo prefix = typeof(PlayerUtils).GetMethod(
            nameof(Prefix_SpellObjectInit),
            BindingFlags.Static | BindingFlags.NonPublic
        );
        SpellModificationSystem.PatchAllSpellObjects(harmony, "Init", prefix);
    }

    private static void Prefix_SpellObjectInit(object __instance)
    {
        if (!GodMode) return;

        var spell = SpellModificationSystem.GetSpellNameFromTypeName(__instance.GetType().Name);
        if (!spell.HasValue) return;

        // Skip spells that don't deal damage
        var defaultTable = SpellModificationSystem.Default();
        if (defaultTable != null &&
            defaultTable.TryGetModifier(spell.Value, "DAMAGE", out var defaultMod) &&
            defaultMod.Base == 0)
            return;

        GameModificationHelpers.ApplyFieldValuesToInstance(
            __instance,
            new Dictionary<string, float> { ["DAMAGE"] = BaseDamage }
        );
    }
}
#pragma warning restore 1591
