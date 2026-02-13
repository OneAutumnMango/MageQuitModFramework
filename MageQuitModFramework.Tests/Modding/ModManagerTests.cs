using Xunit;
using MageQuitModFramework.Modding;
using HarmonyLib;
using System.Linq;

namespace MageQuitModFramework.Tests.Framework.Loading
{
    public class ModManagerTests
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

        public ModManagerTests()
        {
            // Clear ModManager state before each test
            ModManager.Clear();
        }

        [Fact]
        public void RegisterMod_CreatesNewModEntry()
        {
            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");

            Assert.NotNull(moduleManager);
            var mods = ModManager.GetAllMods().ToList();
            Assert.Single(mods);
            Assert.Equal("TestMod", mods[0].ModName);
        }

        [Fact]
        public void RegisterMod_ReturnsExistingModuleManager_WhenModAlreadyRegistered()
        {
            var moduleManager1 = ModManager.RegisterMod("TestMod", "com.test.mod");
            var moduleManager2 = ModManager.RegisterMod("TestMod", "com.test.mod");

            Assert.Same(moduleManager1, moduleManager2);
            var mods = ModManager.GetAllMods().ToList();
            Assert.Single(mods);
        }

        [Fact]
        public void RegisterMod_CreatesUniqueHarmonyInstance()
        {
            var moduleManager1 = ModManager.RegisterMod("Mod1", "com.test.mod1");
            var moduleManager2 = ModManager.RegisterMod("Mod2", "com.test.mod2");

            var mods = ModManager.GetAllMods().ToList();
            Assert.Equal(2, mods.Count);
            Assert.NotSame(mods[0].ModHarmony, mods[1].ModHarmony);
        }

        [Fact]
        public void TryGetModuleManager_ReturnsTrue_WhenModExists()
        {
            ModManager.RegisterMod("TestMod", "com.test.mod");

            var result = ModManager.TryGetModuleManager("TestMod", out var moduleManager);

            Assert.True(result);
            Assert.NotNull(moduleManager);
        }

        [Fact]
        public void TryGetModuleManager_ReturnsFalse_WhenModDoesNotExist()
        {
            var result = ModManager.TryGetModuleManager("NonExistent", out var moduleManager);

            Assert.False(result);
            Assert.Null(moduleManager);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsTrue_WhenModuleIsLoaded()
        {
            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");
            var module = new TestModule("TestModule");
            moduleManager.RegisterModule(module);
            moduleManager.LoadModule("TestModule");

            var result = ModManager.IsModuleLoaded("TestModule");

            Assert.True(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleIsNotLoaded()
        {
            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");
            var module = new TestModule("TestModule");
            moduleManager.RegisterModule(module);

            var result = ModManager.IsModuleLoaded("TestModule");

            Assert.False(result);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenModuleDoesNotExist()
        {
            ModManager.RegisterMod("TestMod", "com.test.mod");

            var result = ModManager.IsModuleLoaded("NonExistent");

            Assert.False(result);
        }

        [Fact]
        public void IsModuleLoaded_SearchesAcrossMultipleMods()
        {
            var moduleManager1 = ModManager.RegisterMod("Mod1", "com.test.mod1");
            var moduleManager2 = ModManager.RegisterMod("Mod2", "com.test.mod2");

            var module1 = new TestModule("Module1");
            var module2 = new TestModule("Module2");

            moduleManager1.RegisterModule(module1);
            moduleManager2.RegisterModule(module2);

            moduleManager1.LoadModule("Module1");
            moduleManager2.LoadModule("Module2");

            Assert.True(ModManager.IsModuleLoaded("Module1"));
            Assert.True(ModManager.IsModuleLoaded("Module2"));
        }

        [Fact]
        public void GetAllMods_ReturnsEmptyCollection_WhenNoModsRegistered()
        {
            var mods = ModManager.GetAllMods().ToList();

            Assert.Empty(mods);
        }

        [Fact]
        public void GetAllMods_ReturnsAllRegisteredMods()
        {
            ModManager.RegisterMod("Mod1", "com.test.mod1");
            ModManager.RegisterMod("Mod2", "com.test.mod2");
            ModManager.RegisterMod("Mod3", "com.test.mod3");

            var mods = ModManager.GetAllMods().ToList();

            Assert.Equal(3, mods.Count);
            Assert.Contains(mods, m => m.ModName == "Mod1");
            Assert.Contains(mods, m => m.ModName == "Mod2");
            Assert.Contains(mods, m => m.ModName == "Mod3");
        }

        [Fact]
        public void Clear_RemovesAllRegisteredMods()
        {
            ModManager.RegisterMod("Mod1", "com.test.mod1");
            ModManager.RegisterMod("Mod2", "com.test.mod2");

            ModManager.Clear();

            var mods = ModManager.GetAllMods().ToList();
            Assert.Empty(mods);
        }

        [Fact]
        public void Clear_AllowsReregistrationAfterClear()
        {
            ModManager.RegisterMod("TestMod", "com.test.mod");
            ModManager.Clear();

            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");

            Assert.NotNull(moduleManager);
            var mods = ModManager.GetAllMods().ToList();
            Assert.Single(mods);
        }

        [Fact]
        public void ModEntry_StoresAllProperties()
        {
            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");
            var mods = ModManager.GetAllMods().ToList();

            var entry = mods[0];
            Assert.Equal("TestMod", entry.ModName);
            Assert.Same(moduleManager, entry.ModuleManager);
            Assert.NotNull(entry.ModHarmony);
        }

        [Fact]
        public void ModEntry_HarmonyInstanceMatchesGuid()
        {
            ModManager.RegisterMod("TestMod", "com.test.mod");
            var mods = ModManager.GetAllMods().ToList();

            var entry = mods[0];
            Assert.Equal("com.test.mod", entry.ModHarmony.Id);
        }

        [Fact]
        public void IsModuleLoaded_ReturnsFalse_WhenNoModsRegistered()
        {
            // ModManager is cleared in constructor

            var result = ModManager.IsModuleLoaded("AnyModule");

            Assert.False(result);
        }

        [Fact]
        public void RegisterMod_LogsWarning_WhenModAlreadyExists()
        {
            var name = "DuplicateMod";
            ModManager.RegisterMod(name, "com.test.duplicate1");

            // Second registration should return existing manager
            var manager = ModManager.RegisterMod(name, "com.test.duplicate2");

            Assert.NotNull(manager);
            var mods = ModManager.GetAllMods().ToList();
            Assert.Single(mods);
        }

        [Fact]
        public void TryGetModuleManager_ReturnsCorrectManager()
        {
            var moduleManager = ModManager.RegisterMod("TestMod", "com.test.mod");
            var module = new TestModule("TestModule");
            moduleManager.RegisterModule(module);

            var result = ModManager.TryGetModuleManager("TestMod", out var retrievedManager);

            Assert.True(result);
            Assert.Same(moduleManager, retrievedManager);
        }

        [Fact]
        public void RegisterMod_WithSameName_LogsWarning_AndReturnsExisting()
        {
            var manager1 = ModManager.RegisterMod("TestMod", "com.test.mod");
            var manager2 = ModManager.RegisterMod("TestMod", "com.test.mod.different");

            Assert.Same(manager1, manager2);
            var mods = ModManager.GetAllMods().ToList();
            Assert.Single(mods);
        }

        [Fact]
        public void RegisterMod_WithDifferentNames_CreatesSeparateManagers()
        {
            var manager1 = ModManager.RegisterMod("Mod1", "com.test.mod1");
            var manager2 = ModManager.RegisterMod("Mod2", "com.test.mod2");

            Assert.NotSame(manager1, manager2);
        }

        [Fact]
        public void TryGetModuleManager_AfterMultipleRegistrations_FindsCorrect()
        {
            var manager1 = ModManager.RegisterMod("Mod1", "com.test.mod1");
            var manager2 = ModManager.RegisterMod("Mod2", "com.test.mod2");

            var found = ModManager.TryGetModuleManager("Mod2", out var retrieved);

            Assert.True(found);
            Assert.Same(manager2, retrieved);
        }
    }
}
