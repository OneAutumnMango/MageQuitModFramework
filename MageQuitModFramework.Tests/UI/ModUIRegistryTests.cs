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

            ModUIRegistry.RegisterMod(modName, description, () => { });

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

            ModUIRegistry.RegisterMod(modName, "Description", () => { });

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(defaultPriority, entry.Priority);
        }

        [Fact]
        public void RegisterMod_SetsCustomPriority()
        {
            var modName = "TestMod";
            var customPriority = 50;

            ModUIRegistry.RegisterMod(modName, "Description", () => { }, customPriority);

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(customPriority, entry.Priority);
        }

        [Fact]
        public void RegisterMod_OverwritesExistingMod()
        {
            var modName = "TestMod";
            var firstDescription = "First";
            var secondDescription = "Second";

            ModUIRegistry.RegisterMod(modName, firstDescription, () => { });
            ModUIRegistry.RegisterMod(modName, secondDescription, () => { });

            ModUIRegistry.TryGetMod(modName, out var entry);
            Assert.Equal(secondDescription, entry.Description);
        }

        [Fact]
        public void UnregisterMod_RemovesMod()
        {
            var modName = "TestMod";
            ModUIRegistry.RegisterMod(modName, "Description", () => { });

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
            ModUIRegistry.RegisterMod(modName, "Description", () => { });

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
            ModUIRegistry.RegisterMod("Mod1", "Description1", () => { });
            ModUIRegistry.RegisterMod("Mod2", "Description2", () => { });
            ModUIRegistry.RegisterMod("Mod3", "Description3", () => { });

            var mods = ModUIRegistry.GetAllMods().ToList();

            Assert.Equal(modCount, mods.Count);
        }

        [Fact]
        public void GetAllMods_SortsByPriority_Ascending()
        {
            ModUIRegistry.RegisterMod("HighPriority", "Desc", () => { }, 200);
            ModUIRegistry.RegisterMod("LowPriority", "Desc", () => { }, 50);
            ModUIRegistry.RegisterMod("MediumPriority", "Desc", () => { }, 100);

            var mods = ModUIRegistry.GetAllMods().ToList();

            Assert.Equal("LowPriority", mods[0].ModName);
            Assert.Equal("MediumPriority", mods[1].ModName);
            Assert.Equal("HighPriority", mods[2].ModName);
        }

        [Fact]
        public void GetAllMods_CachesSortedList()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", () => { }, 100);

            var firstCall = ModUIRegistry.GetAllMods();
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public void RegisterMod_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", () => { }, 100);
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.RegisterMod("Mod2", "Desc", () => { }, 50);
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void UnregisterMod_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", () => { });
            ModUIRegistry.RegisterMod("Mod2", "Desc", () => { });
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.UnregisterMod("Mod1");
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void Clear_RemovesAllMods()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", () => { });
            ModUIRegistry.RegisterMod("Mod2", "Desc", () => { });

            ModUIRegistry.Clear();

            var mods = ModUIRegistry.GetAllMods();
            Assert.Empty(mods);
        }

        [Fact]
        public void Clear_InvalidatesSortedCache()
        {
            ModUIRegistry.RegisterMod("Mod1", "Desc", () => { });
            var firstCall = ModUIRegistry.GetAllMods();

            ModUIRegistry.Clear();
            var secondCall = ModUIRegistry.GetAllMods();

            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void ModUIEntry_InitiallyNotExpanded()
        {
            ModUIRegistry.RegisterMod("TestMod", "Desc", () => { });

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            Assert.False(entry.IsExpanded);
        }

        [Fact]
        public void ModUIEntry_CanToggleExpanded()
        {
            ModUIRegistry.RegisterMod("TestMod", "Desc", () => { });

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            entry.IsExpanded = true;

            Assert.True(entry.IsExpanded);
        }

        [Fact]
        public void RegisterMod_StoresDrawIMGUIAction()
        {
            void buildAction() { }

            ModUIRegistry.RegisterMod("TestMod", "Desc", buildAction);

            ModUIRegistry.TryGetMod("TestMod", out var entry);
            Assert.NotNull(entry.DrawIMGUI);
        }

        [Fact]
        public void GetAllMods_OrderStabilityWithSamePriority()
        {
            ModUIRegistry.RegisterMod("ModA", "Desc", () => { }, 100);
            ModUIRegistry.RegisterMod("ModB", "Desc", () => { }, 100);
            ModUIRegistry.RegisterMod("ModC", "Desc", () => { }, 100);

            var mods = ModUIRegistry.GetAllMods().ToList();

            // All have same priority, should maintain insertion order
            Assert.Equal(3, mods.Count);
            // Order should be stable
            Assert.Equal("ModA", mods[0].ModName);
            Assert.Equal("ModB", mods[1].ModName);
            Assert.Equal("ModC", mods[2].ModName);
        }

        [Fact]
        public void ModUIEntry_PreservesIsExpandedState()
        {
            ModUIRegistry.RegisterMod("TestMod", "Desc", () => { });
            ModUIRegistry.TryGetMod("TestMod", out var entry);
            entry.IsExpanded = true;

            // Re-register should preserve the entry object
            ModUIRegistry.RegisterMod("TestMod", "Updated Desc", () => { });
            ModUIRegistry.TryGetMod("TestMod", out var updatedEntry);

            // IsExpanded state should be preserved in the same entry object
            Assert.True(updatedEntry.IsExpanded);
        }
    }
}
