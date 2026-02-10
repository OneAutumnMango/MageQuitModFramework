using Xunit;
using MageQuitModFramework.Loading;
using HarmonyLib;
using System.Linq;

namespace MageQuitModFramework.Tests.Framework.Loading
{
    public class ModuleManagerTests
    {
        private class TestModule : IModModule
        {
            public string ModuleName { get; set; }
            public bool IsLoaded { get; set; }
            public int LoadCount { get; private set; }
            public int UnloadCount { get; private set; }

            public TestModule(string name)
            {
                ModuleName = name;
            }

            public void Load(Harmony harmony)
            {
                IsLoaded = true;
                LoadCount++;
            }

            public void Unload(Harmony harmony)
            {
                IsLoaded = false;
                UnloadCount++;
            }
        }

        public ModuleManagerTests()
        {
            ModuleManager.Clear();
        }

        [Fact]
        public void Initialize_SetsHarmonyInstance()
        {
            var harmony = new Harmony("test.harmony");

            ModuleManager.Initialize(harmony);
        }

        [Fact]
        public void RegisterModule_AddsModuleToRegistry()
        {
            var module = new TestModule("TestMod");

            ModuleManager.RegisterModule(module);

            var moduleNames = ModuleManager.GetAllModuleNames();
            Assert.Contains("TestMod", moduleNames);
        }

        [Fact]
        public void RegisterModule_DoesNotAddDuplicate()
        {
            var module1 = new TestModule("TestMod");
            var module2 = new TestModule("TestMod");

            ModuleManager.RegisterModule(module1);
            ModuleManager.RegisterModule(module2);

            var moduleCount = ModuleManager.GetAllModuleNames().Count();
            Assert.Equal(1, moduleCount);
        }

        [Fact]
        public void LoadModule_ReturnsTrue_WhenModuleExists()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);

            var result = ModuleManager.LoadModule("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void LoadModule_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = ModuleManager.LoadModule("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void LoadModule_CallsModuleLoadMethod()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);
            var expectedLoadCount = 1;

            ModuleManager.LoadModule("TestMod");

            Assert.Equal(expectedLoadCount, module.LoadCount);
        }

        [Fact]
        public void LoadModule_SetsIsLoadedToTrue()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);

            ModuleManager.LoadModule("TestMod");

            Assert.True(module.IsLoaded);
        }

        [Fact]
        public void UnloadModule_ReturnsTrue_WhenModuleExists()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);
            ModuleManager.LoadModule("TestMod");

            var result = ModuleManager.UnloadModule("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void UnloadModule_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = ModuleManager.UnloadModule("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void UnloadModule_CallsModuleUnloadMethod()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);
            ModuleManager.LoadModule("TestMod");
            var expectedUnloadCount = 1;

            ModuleManager.UnloadModule("TestMod");

            Assert.Equal(expectedUnloadCount, module.UnloadCount);
        }

        [Fact]
        public void UnloadModule_SetsIsLoadedToFalse()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);
            ModuleManager.LoadModule("TestMod");

            ModuleManager.UnloadModule("TestMod");

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsTrue_WhenModuleIsLoaded()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);
            ModuleManager.LoadModule("TestMod");

            var result = ModuleManager.IsModuleLoaded("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleIsNotLoaded()
        {
            var module = new TestModule("TestMod");
            ModuleManager.RegisterModule(module);

            var result = ModuleManager.IsModuleLoaded("TestMod");

            Assert.False(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = ModuleManager.IsModuleLoaded("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void GetAllModuleNames_ReturnsEmptyList_WhenNoModulesRegistered()
        {
            var moduleNames = ModuleManager.GetAllModuleNames();

            Assert.Empty(moduleNames);
        }

        [Fact]
        public void GetAllModuleNames_ReturnsAllRegisteredModules()
        {
            ModuleManager.RegisterModule(new TestModule("Mod1"));
            ModuleManager.RegisterModule(new TestModule("Mod2"));
            ModuleManager.RegisterModule(new TestModule("Mod3"));
            var expectedCount = 3;

            var moduleNames = ModuleManager.GetAllModuleNames().ToList();

            Assert.Equal(expectedCount, moduleNames.Count);
            Assert.Contains("Mod1", moduleNames);
            Assert.Contains("Mod2", moduleNames);
            Assert.Contains("Mod3", moduleNames);
        }

        [Fact]
        public void GetLoadedModuleNames_ReturnsOnlyLoadedModules()
        {
            var module1 = new TestModule("Mod1");
            var module2 = new TestModule("Mod2");
            var module3 = new TestModule("Mod3");
            ModuleManager.RegisterModule(module1);
            ModuleManager.RegisterModule(module2);
            ModuleManager.RegisterModule(module3);
            ModuleManager.LoadModule("Mod1");
            ModuleManager.LoadModule("Mod3");
            var expectedCount = 2;

            var loadedModules = ModuleManager.GetLoadedModuleNames().ToList();

            Assert.Equal(expectedCount, loadedModules.Count);
            Assert.Contains("Mod1", loadedModules);
            Assert.Contains("Mod3", loadedModules);
            Assert.DoesNotContain("Mod2", loadedModules);
        }

        [Fact]
        public void GetLoadedModuleNames_ReturnsEmptyList_WhenNoModulesLoaded()
        {
            ModuleManager.RegisterModule(new TestModule("Mod1"));
            ModuleManager.RegisterModule(new TestModule("Mod2"));

            var loadedModules = ModuleManager.GetLoadedModuleNames();

            Assert.Empty(loadedModules);
        }

        [Fact]
        public void Clear_RemovesAllModules()
        {
            ModuleManager.RegisterModule(new TestModule("Mod1"));
            ModuleManager.RegisterModule(new TestModule("Mod2"));

            ModuleManager.Clear();

            var moduleNames = ModuleManager.GetAllModuleNames();
            Assert.Empty(moduleNames);
        }
    }
}
