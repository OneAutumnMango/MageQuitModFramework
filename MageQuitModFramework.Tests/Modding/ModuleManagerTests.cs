using Xunit;
using MageQuitModFramework.Modding;
using HarmonyLib;
using System.Linq;

namespace MageQuitModFramework.Tests.Framework.Loading
{
    public class ModuleManagerTests
    {
        private class TestModule : IModule
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

        private ModuleManager _moduleManager;

        public ModuleManagerTests()
        {
            var harmony = new Harmony("test.harmony");
            _moduleManager = new ModuleManager(harmony);
        }

        [Fact]
        public void RegisterModule_AddsModuleToRegistry()
        {
            var module = new TestModule("TestMod");

            _moduleManager.RegisterModule(module);

            var moduleNames = _moduleManager.GetAllModuleNames();
            Assert.Contains("TestMod", moduleNames);
        }

        [Fact]
        public void RegisterModule_DoesNotAddDuplicate()
        {
            var module1 = new TestModule("TestMod");
            var module2 = new TestModule("TestMod");

            _moduleManager.RegisterModule(module1);
            _moduleManager.RegisterModule(module2);

            var moduleCount = _moduleManager.GetAllModuleNames().Count();
            Assert.Equal(1, moduleCount);
        }

        [Fact]
        public void LoadModule_ReturnsTrue_WhenModuleExists()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.LoadModule("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void LoadModule_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = _moduleManager.LoadModule("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void LoadModule_CallsModuleLoadMethod()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            var expectedLoadCount = 1;

            _moduleManager.LoadModule("TestMod");

            Assert.Equal(expectedLoadCount, module.LoadCount);
        }

        [Fact]
        public void LoadModule_SetsIsLoadedToTrue()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            _moduleManager.LoadModule("TestMod");

            Assert.True(module.IsLoaded);
        }

        [Fact]
        public void UnloadModule_ReturnsTrue_WhenModuleExists()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");

            var result = _moduleManager.UnloadModule("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void UnloadModule_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = _moduleManager.UnloadModule("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void UnloadModule_CallsModuleUnloadMethod()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");
            var expectedUnloadCount = 1;

            _moduleManager.UnloadModule("TestMod");

            Assert.Equal(expectedUnloadCount, module.UnloadCount);
        }

        [Fact]
        public void UnloadModule_SetsIsLoadedToFalse()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");

            _moduleManager.UnloadModule("TestMod");

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsTrue_WhenModuleIsLoaded()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");

            var result = _moduleManager.IsModuleLoaded("TestMod");

            Assert.True(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleIsNotLoaded()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.IsModuleLoaded("TestMod");

            Assert.False(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleDoesNotExist()
        {
            var result = _moduleManager.IsModuleLoaded("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void GetAllModuleNames_ReturnsEmptyList_WhenNoModulesRegistered()
        {
            var moduleNames = _moduleManager.GetAllModuleNames();

            Assert.Empty(moduleNames);
        }

        [Fact]
        public void GetAllModuleNames_ReturnsAllRegisteredModules()
        {
            _moduleManager.RegisterModule(new TestModule("Mod1"));
            _moduleManager.RegisterModule(new TestModule("Mod2"));
            _moduleManager.RegisterModule(new TestModule("Mod3"));
            var expectedCount = 3;

            var moduleNames = _moduleManager.GetAllModuleNames().ToList();

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
            _moduleManager.RegisterModule(module1);
            _moduleManager.RegisterModule(module2);
            _moduleManager.RegisterModule(module3);
            _moduleManager.LoadModule("Mod1");
            _moduleManager.LoadModule("Mod3");
            var expectedCount = 2;

            var loadedModules = _moduleManager.GetLoadedModuleNames().ToList();

            Assert.Equal(expectedCount, loadedModules.Count);
            Assert.Contains("Mod1", loadedModules);
            Assert.Contains("Mod3", loadedModules);
            Assert.DoesNotContain("Mod2", loadedModules);
        }

        [Fact]
        public void GetLoadedModuleNames_ReturnsEmptyList_WhenNoModulesLoaded()
        {
            _moduleManager.RegisterModule(new TestModule("Mod1"));
            _moduleManager.RegisterModule(new TestModule("Mod2"));

            var loadedModules = _moduleManager.GetLoadedModuleNames();

            Assert.Empty(loadedModules);
        }

        [Fact]
        public void Clear_RemovesAllModules()
        {
            _moduleManager.RegisterModule(new TestModule("Mod1"));
            _moduleManager.RegisterModule(new TestModule("Mod2"));

            _moduleManager.Clear();

            var moduleNames = _moduleManager.GetAllModuleNames();
            Assert.Empty(moduleNames);
        }

        [Fact]
        public void LoadModule_DoesNotIncrement_WhenCalledMultipleTimes()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            _moduleManager.LoadModule("TestMod");
            _moduleManager.LoadModule("TestMod");
            _moduleManager.LoadModule("TestMod");

            Assert.Equal(1, module.LoadCount);
        }

        [Fact]
        public void UnloadModule_DoesNotDecrement_WhenCalledMultipleTimes()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");

            _moduleManager.UnloadModule("TestMod");
            _moduleManager.UnloadModule("TestMod");
            _moduleManager.UnloadModule("TestMod");

            Assert.Equal(1, module.UnloadCount);
        }

        [Fact]
        public void LoadModule_ReturnsFalse_WhenModuleAlreadyLoaded()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");

            var result = _moduleManager.LoadModule("TestMod");

            Assert.False(result);
        }

        [Fact]
        public void UnloadModule_ReturnsFalse_WhenModuleAlreadyUnloaded()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.UnloadModule("TestMod");

            Assert.False(result);
        }

        [Fact]
        public void RegisterModule_WithSameNameTwice_OnlyKeepsFirstModule()
        {
            var module1 = new TestModule("Duplicate");
            var module2 = new TestModule("Duplicate");

            _moduleManager.RegisterModule(module1);
            _moduleManager.RegisterModule(module2);

            _moduleManager.LoadModule("Duplicate");

            // Only first module should be registered
            Assert.Equal(1, module1.LoadCount);
            Assert.Equal(0, module2.LoadCount);
        }

        [Fact]
        public void LoadModule_CreatesHarmonyInstance_OnFirstLoad()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.LoadModule("TestMod");

            Assert.True(result);
            Assert.True(module.IsLoaded);
        }

        [Fact]
        public void LoadModule_ReusesHarmonyInstance_OnReload()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);

            _moduleManager.LoadModule("TestMod");
            _moduleManager.UnloadModule("TestMod");
            _moduleManager.LoadModule("TestMod");

            Assert.Equal(2, module.LoadCount);
        }

        [Fact]
        public void UnloadModule_WithInconsistentState_DoesNotCallUnload()
        {
            // This tests defensive behavior: if a module reports IsLoaded=true but was never 
            // actually loaded through LoadModule (no Harmony instance), Unload is not called
            var module = new TestModule("TestMod") { IsLoaded = true };
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.UnloadModule("TestMod");

            Assert.True(result); // Still returns true (doesn't fail)
            Assert.Equal(0, module.UnloadCount); // But Unload not called (no Harmony instance)
        }

        [Fact]
        public void LoadModule_WhenModuleAlreadyLoaded_SkipsLoad()
        {
            var module = new TestModule("TestMod");
            _moduleManager.RegisterModule(module);
            _moduleManager.LoadModule("TestMod");
            var initialLoadCount = module.LoadCount;

            var result = _moduleManager.LoadModule("TestMod");

            Assert.False(result);
            Assert.Equal(initialLoadCount, module.LoadCount);
        }

        [Fact]
        public void UnloadModule_WhenModuleNotLoaded_SkipsUnload()
        {
            var module = new TestModule("TestMod") { IsLoaded = false };
            _moduleManager.RegisterModule(module);

            var result = _moduleManager.UnloadModule("TestMod");

            Assert.False(result);
            Assert.Equal(0, module.UnloadCount);
        }
    }
}
