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
    [BepInDependency("com.spellcast.modframework", BepInDependency.DependencyFlags.HardDependency)]
    public class ExampleModPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;
        private Harmony _harmony;

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
                BuildModUI,
                priority: 50
            );
        }

        private void BuildModUI(Transform parent)
        {
            var titleText = UIComponents.CreateText(parent, "ExampleTitle",
                "Example Mod Settings", 16);
            titleText.fontStyle = FontStyle.Bold;

            var descPanel = UIComponents.CreatePanel(parent, "DescPanel", 350, 60);
            descPanel.GetComponent<Image>().color = StyleManager.PanelColor;

            var descText = UIComponents.CreateText(descPanel.transform, "Desc",
                "This example mod demonstrates basic framework usage:\n" +
                "- Spell modification via GameModificationHelpers\n" +
                "- UI registration with ModUIRegistry\n" +
                "- Simple interactive controls", 12);

            var togglePanel = UIComponents.CreatePanel(parent, "TogglePanel", 350, 40);
            togglePanel.GetComponent<Image>().color = StyleManager.PanelColor;

            var toggleText = UIComponents.CreateText(togglePanel.transform, "ToggleLabel",
                "Enable Custom Behavior:", 14);
            
            var toggle = togglePanel.AddComponent<Toggle>();
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((value) =>
            {
                _log.LogInfo($"Custom behavior toggled: {value}");
            });
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
