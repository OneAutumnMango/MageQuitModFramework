using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using MageQuitModFramework;
using MageQuitModFramework.UI;
using MageQuitModFramework.Core;

namespace ExampleMod
{
    [BepInPlugin("com.example.spellmod", "Example Spell Mod", "1.0.0")]
    [BepInDependency("com.magequit.modframework", BepInDependency.DependencyFlags.HardDependency)]
    public class ExampleModPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;
        private Harmony _harmony;
        private bool _customBehaviorEnabled = false;

        private void Awake()
        {
            _log = Logger;
            _log.LogInfo("Example Mod loading...");

            _harmony = new Harmony("com.example.spellmod");
            _harmony.PatchAll();

            RegisterWithFramework();

            _log.LogInfo("Example Mod loaded!");
        }

        private void RegisterWithFramework()
        {
            ModUIRegistry.RegisterMod(
                "Example Mod",
                "A simple example mod demonstrating framework usage",
                DrawModUI,
                priority: 50
            );
        }

        private void DrawModUI()
        {
            UnityEngine.GUILayout.Label("Example Mod Settings");
            UnityEngine.GUILayout.Space(10);
            
            UnityEngine.GUILayout.Label("This mod demonstrates:");
            UnityEngine.GUILayout.Label("• Spell modification via GameModificationHelpers");
            UnityEngine.GUILayout.Label("• UI registration with ModUIRegistry");
            UnityEngine.GUILayout.Label("• Simple interactive controls");
            UnityEngine.GUILayout.Space(10);
            
            UnityEngine.GUILayout.BeginHorizontal();
            UnityEngine.GUILayout.Label("Enable Custom Behavior:", UnityEngine.GUILayout.Width(200));
            bool newValue = UnityEngine.GUILayout.Toggle(_customBehaviorEnabled, "");
            if (newValue != _customBehaviorEnabled)
            {
                _customBehaviorEnabled = newValue;
                _log.LogInfo($"Custom behavior toggled: {_customBehaviorEnabled}");
            }
            UnityEngine.GUILayout.EndHorizontal();
        }

        [HarmonyPatch(typeof(SpellManager), "Awake")]
        public static class ExamplePatch
        {
            static void Postfix(SpellManager __instance)
            {
                _log.LogInfo("SpellManager awakened - modifying spells...");
                
                // Example: Make fireball faster
                GameModificationHelpers.ModifySpellTableEntry(__instance, SpellName.Fireball, spell =>
                {
                    spell.cooldown *= 0.75f;  // 25% faster cooldown
                    spell.initialVelocity *= 1.2f;  // 20% faster projectile
                    _log.LogInfo($"Modified Fireball: cooldown={spell.cooldown}, velocity={spell.initialVelocity}");
                });

                // Example: Access private fields
                var privateValue = GameModificationHelpers.GetPrivateField<float>(__instance, "somePrivateField");
                GameModificationHelpers.SetPrivateField(__instance, "somePrivateField", privateValue * 1.5f);
            }
        }
    }
}
