using Xunit;
using MageQuitModFramework.UI;
using System.Linq;

namespace MageQuitModFramework.Tests.Framework.UI
{
    public class ModUIRegistryTests
    {
        public ModUIRegistryTests()
        {
            ModUIRegistry.Clear();
        }

        [Fact]
        public void RegisterMod_AddsModToRegistry()
        {
            var modName = "TestMod";
            var description = "Test Description";

            ModUIRegistry.RegisterMod(modName, description, _ => { });

            var result = ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.True(result);
            Assert.Equal(modName, entry.ModName);
            Assert.Equal(description, entry.Description);
        }

        [Fact]
        public void RegisterMod_SetsDefaultPriority()
        {
            var modName = "TestMod";
            var defaultPriority = 100;

            ModUIRegistry.RegisterMod(modName, "Description", _ => { });

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(defaultPriority, entry.Priority);
        }

        [Fact]
        public void RegisterMod_SetsCustomPriority()
        {
            var modName = "TestMod";
            var customPriority = 50;

            ModUIRegistry.RegisterMod(modName, "Description", _ => { }, customPriority);

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(customPriority, entry.Priority);
        }

        [Fact]
        public void RegisterMod_OverwritesExistingMod()
        {
            var modName = "TestMod";
            var firstDescription = "First";
            var secondDescription = "Second";

            ModUIRegistry.RegisterMod(modName, firstDescription, _ => { });
            ModUIRegistry.RegisterMod(modName, secondDescription, _ => { });

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(secondDescription, entry.Description);
        }

        [Fact]
        public void UnregisterMod_RemovesMod()
        {
            var modName = "TestMod";
            ModUIRegistry.RegisterMod(modName, "Description", _ => { });

            ModUIRegistry.UnregisterMod(modName);

            var result = ModUIRegistry.TryGetMod(modName, out _);
            Assert.False(result);
        }

        [Fact]
        public void UnregisterMod_DoesNotThrow_WhenModDoesNotExist()
        {
            ModUIRegistry.UnregisterMod("NonExistent");
        }

        [Fact]
        public void TryGetMod_ReturnsTrue_WhenModExists()
        {
            var modName = "TestMod";
            ModUIRegistry.RegisterMod(modName, "Description", _ => { });

            var result = ModUIRegistry.TryGetMod(modName, out var entry);

            Assert.True(result);
            Assert.NotNull(entry);
        }

        [Fact]
        public void TryGetMod_ReturnsFalse_WhenModDoesNotExist()
        {
            var result = ModUIRegistry.TryGetMod("NonExistent", out var entry);

            Assert.False(result);
            Assert.Null(entry);
        }

        [Fact]
        public void GetAllMods_ReturnsEmptyList_WhenNoModsRegistered()
        {
            var mods = ModUIRegistry.GetAllMods();

            Assert.Empty(mods);
        }

        [Fact]
        public void GetAllMods_ReturnsAllRegisteredMods()
        {
            var modCount = 3;
            ModUIRegistry.RegisterMod("Mod1", "Description1", _ => { });
            ModUIRegistry.RegisterMod("Mod2", "Description2", _ => { });
            ModUIRegistry.RegisterMod("Mod3", "Description3", _ => { });

            var mods = ModUIRegistry.GetAllMods().ToList();

            Assert.Equal(modCount, mods.Count);
        }

        [Fact]
        public void GetAllMods_SortsByPriority_Ascending()
        {
            ModUIRegistry.RegisterMod("HighPriority", "Desc", _ => { }, 200);
            ModUIRegistry.RegisterMod("LowPriority", "Desc", _ => { }, 50);
            ModUIRegistry.RegisterMod("MediumPriority", "Desc", _ => { }, 100);

            var mods = ModUIRegistry.GetAllMods().ToList();

            Assert.Equal("LowPriority", mods[0].ModName);
            Assert.Equal("MediumPriority", mods[1].ModName);
            Assert.Equal("HighPriority", mods[2].ModName);
        }

        [Fact]
        public void GetAllMods_CachesSortedList()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", _ => { }, 100);
            
            var firstCall = ModUIRegistry.GetAllMods();
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public void RegisterMod_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", _ => { }, 100);
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.RegisterMod("Mod2", "Desc", _ => { }, 50);
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void UnregisterMod_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", _ => { });
            ModUIRegistry.RegisterMod("Mod2", "Desc", _ => { });
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.UnregisterMod("Mod1");
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void Clear_RemovesAllMods()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", _ => { });
            ModUIRegistry.RegisterMod("Mod2", "Desc", _ => { });

            ModUIRegistry.Clear();

            var mods = ModUIRegistry.GetAllMods();
            Assert.Empty(mods);
        }

        [Fact]
        public void Clear_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", _ => { });
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.Clear();
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void ModUIEntry_InitiallyNotExpanded()
        {
            ModUIRegistry.RegisterMod("TestMod", "Desc", _ => { });

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            Assert.False(entry.IsExpanded);
        }

        [Fact]
        public void ModUIEntry_CanToggleExpanded()
        {
            ModUIRegistry.RegisterMod("TestMod", "Desc", _ => { });

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            entry.IsExpanded = true;

            Assert.True(entry.IsExpanded);
        }

        [Fact]
        public void RegisterMod_StoresBuildOptionsUIAction()
        {
            void buildAction(UnityEngine.Transform t) { }

            ModUIRegistry.RegisterMod("TestMod", "Desc", buildAction);

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            Assert.NotNull(entry.BuildOptionsUI);
        }
    }
}
