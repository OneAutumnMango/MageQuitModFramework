using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using MageQuitModFramework;
using MageQuitModFramework.Modding;
using MageQuitModFramework.UI;
using MageQuitModFramework.Utilities;
using ExampleMod.Modules;

namespace ExampleMod
{
    [BepInPlugin("com.example.spellmod", "Example Spell Mod", "1.0.0")]
    [BepInDependency("com.magequit.modframework", BepInDependency.DependencyFlags.HardDependency)]
    public class ExampleModPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;
        private ModuleManager _moduleManager;
        private bool _exampleToggleValue = false;
        private PhotonRpcManager _rpcManager;
        private int _rpcTestCount = 0;

        private void Awake()
        {
            _log = Logger;
            _log.LogInfo("Example Mod loading...");

            // Register mod with ModManager and get ModuleManager instance
            _moduleManager = ModManager.RegisterMod("Example Mod", "com.example.spellmod");

            // Register and load the spell modification module
            var spellModule = new ExampleSpellModule(_log);
            _moduleManager.RegisterModule(spellModule);
            _moduleManager.LoadModule(spellModule.ModuleName);

            ModUIRegistry.RegisterMod(
                "Example Mod",
                "Demonstrates framework features: modules, UI components, and Photon RPC",
                DrawModUI,
                priority: 50
            );

            _rpcManager = PhotonRpcManager.CreatePersistent("ExampleModRpcManager", 9998);
            _rpcManager.RegisterHandler("ExampleModPing", args =>
            {
                _rpcTestCount++;
                _log.LogInfo($"ExampleMod RPC received #{_rpcTestCount}: {string.Join(", ", args ?? Array.Empty<object>())}");
            });

            _log.LogInfo("Example Mod loaded!");
        }

        private void DrawModUI()
        {
            UIComponents.Section("Example Mod Settings", () =>
            {
                UIComponents.Label("This mod demonstrates:", StyleManager.Gold);
                UIComponents.Label("- Module system (toggleable spell modifications)");
                UIComponents.Label("- UI components and StyleManager");
                UIComponents.Label("- Photon RPC helpers");
                UIComponents.Label("- ModUIRegistry integration");
                UIComponents.Label("This mod MUST be loaded before opening a game or it won't work, and will patch the game until quitting");
            });

            bool newValue = UIComponents.Toggle("Example Toggle:", _exampleToggleValue);
            if (newValue != _exampleToggleValue)
            {
                _exampleToggleValue = newValue;
                _log.LogInfo($"Example toggle changed: {_exampleToggleValue}");
            }
            UIComponents.Label($"Toggle value: {_exampleToggleValue}");

            UIComponents.Space();
            UIComponents.Label($"Photon connected: {PhotonNetwork.connected}");
            UIComponents.Label($"RPC tests received: {_rpcTestCount}");

            if (UIComponents.Button("Send RPC Test", 200, StyleManager.Green))
            {
                if (_rpcManager == null)
                {
                    _log.LogWarning("RPC manager not initialized.");
                }
                else
                {
                    _rpcManager.SendRpcLocal("ExampleModPing", PhotonTargets.All, "ping", Time.time);
                    _log.LogInfo("RPC test sent.");
                }
            }
        }
    }
}
