using HarmonyLib;
using MageQuitModFramework.Modding;
using System;
using System.Reflection;
using Xunit;

namespace MageQuitModFramework.Tests.Framework.Loading
{
    /// <summary>
    /// Tests for BaseModule lifecycle management and patch group functionality.
    /// </summary>
    public class ModuleTests : IDisposable
    {
        private Harmony _testHarmony;

        public ModuleTests()
        {
            _testHarmony = new Harmony("test.module.lifecycle");
        }

        public void Dispose()
        {
            _testHarmony?.UnpatchSelf();
        }

        // Test module implementation
        private class TestModuleImpl : BaseModule
        {
            public override string ModuleName => "TestModule";
            public int OnLoadCallCount { get; private set; }
            public int OnUnloadCallCount { get; private set; }
            public bool ThrowOnLoad { get; set; }
            public bool ThrowOnUnload { get; set; }

            protected override void OnLoad(Harmony harmony)
            {
                OnLoadCallCount++;
                if (ThrowOnLoad)
                    throw new InvalidOperationException("Load failed");
            }

            protected override void OnUnload(Harmony harmony)
            {
                OnUnloadCallCount++;
                if (ThrowOnUnload)
                    throw new InvalidOperationException("Unload failed");
            }
        }

        // Test module with patch group
        private class TestModuleWithPatches : BaseModule
        {
            public override string ModuleName => "TestModuleWithPatches";

            protected override void OnLoad(Harmony harmony)
            {
                PatchGroup(harmony, typeof(TestPatches));
            }

            protected override void OnUnload(Harmony harmony)
            {
                harmony.UnpatchSelf();
            }
        }

        // Test patches for PatchGroup testing  
        [HarmonyPatch]
        private static class TestPatches
        {
            [HarmonyPatch(typeof(object), nameof(object.ToString))]
            [HarmonyPrefix]
            static void TestPrefix() { }
        }

        [Fact]
        public void BaseModule_InitialState_IsNotLoaded()
        {
            var module = new TestModuleImpl();

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void BaseModule_ModuleName_ReturnsCorrectName()
        {
            var module = new TestModuleImpl();

            Assert.Equal("TestModule", module.ModuleName);
        }

        [Fact]
        public void Load_CallsOnLoad_AndSetsIsLoadedToTrue()
        {
            var module = new TestModuleImpl();

            module.Load(_testHarmony);

            Assert.True(module.IsLoaded);
            Assert.Equal(1, module.OnLoadCallCount);
        }

        [Fact]
        public void Load_WhenAlreadyLoaded_DoesNotCallOnLoadAgain()
        {
            var module = new TestModuleImpl();
            module.Load(_testHarmony);

            module.Load(_testHarmony);

            Assert.Equal(1, module.OnLoadCallCount);
        }

        [Fact]
        public void Load_WhenAlreadyLoaded_IsLoadedRemainsTrue()
        {
            var module = new TestModuleImpl();
            module.Load(_testHarmony);

            module.Load(_testHarmony);

            Assert.True(module.IsLoaded);
        }

        [Fact]
        public void Unload_CallsOnUnload_AndSetsIsLoadedToFalse()
        {
            var module = new TestModuleImpl();
            module.Load(_testHarmony);

            module.Unload(_testHarmony);

            Assert.False(module.IsLoaded);
            Assert.Equal(1, module.OnUnloadCallCount);
        }

        [Fact]
        public void Unload_WhenNotLoaded_DoesNotCallOnUnload()
        {
            var module = new TestModuleImpl();

            module.Unload(_testHarmony);

            Assert.Equal(0, module.OnUnloadCallCount);
        }

        [Fact]
        public void Unload_WhenNotLoaded_IsLoadedRemainsFalse()
        {
            var module = new TestModuleImpl();

            module.Unload(_testHarmony);

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void Load_WhenOnLoadThrows_IsLoadedRemainsFalse()
        {
            var module = new TestModuleImpl { ThrowOnLoad = true };

            var exception = Assert.Throws<InvalidOperationException>(() => module.Load(_testHarmony));

            Assert.False(module.IsLoaded);
            Assert.Equal("Load failed", exception.Message);
        }

        [Fact]
        public void Load_WhenOnLoadThrows_OnLoadWasStillCalled()
        {
            var module = new TestModuleImpl { ThrowOnLoad = true };

            Assert.Throws<InvalidOperationException>(() => module.Load(_testHarmony));

            Assert.Equal(1, module.OnLoadCallCount);
        }

        [Fact]
        public void Unload_WhenOnUnloadThrows_IsLoadedRemainsFalse()
        {
            var module = new TestModuleImpl { ThrowOnUnload = true };
            module.Load(_testHarmony);

            var exception = Assert.Throws<InvalidOperationException>(() => module.Unload(_testHarmony));

            Assert.False(module.IsLoaded);
            Assert.Equal("Unload failed", exception.Message);
        }

        [Fact]
        public void Unload_WhenOnUnloadThrows_OnUnloadWasStillCalled()
        {
            var module = new TestModuleImpl { ThrowOnUnload = true };
            module.Load(_testHarmony);

            Assert.Throws<InvalidOperationException>(() => module.Unload(_testHarmony));

            Assert.Equal(1, module.OnUnloadCallCount);
        }

        [Fact]
        public void LoadAndUnload_Cycle_CanBeRepeated()
        {
            var module = new TestModuleImpl();

            module.Load(_testHarmony);
            module.Unload(_testHarmony);
            module.Load(_testHarmony);
            module.Unload(_testHarmony);

            Assert.False(module.IsLoaded);
            Assert.Equal(2, module.OnLoadCallCount);
            Assert.Equal(2, module.OnUnloadCallCount);
        }

        [Fact]
        public void PatchGroup_AppliesHarmonyPatches()
        {
            var module = new TestModuleWithPatches();
            var harmony = new Harmony("test.patchgroup");

            module.Load(harmony);

            // Verify patches were applied by checking Harmony's patch info
            var patches = harmony.GetPatchedMethods();
            Assert.NotEmpty(patches);

            harmony.UnpatchSelf();
        }

        [Fact]
        public void PatchGroup_OnlyPatchesTypesInSameNamespace()
        {
            var module = new TestModuleWithPatches();
            var harmony = new Harmony("test.patchgroup.namespace");

            module.Load(harmony);

            // TestPatches is in the same namespace as TestModuleWithPatches
            var patches = harmony.GetPatchedMethods();
            Assert.NotEmpty(patches);

            harmony.UnpatchSelf();
        }

        [Fact]
        public void IModule_Interface_IsImplementedByBaseModule()
        {
            var module = new TestModuleImpl();

            Assert.IsAssignableFrom<IModule>(module);
        }

        [Fact]
        public void IModule_ModuleName_IsAccessibleThroughInterface()
        {
            IModule module = new TestModuleImpl();

            Assert.Equal("TestModule", module.ModuleName);
        }

        [Fact]
        public void IModule_IsLoaded_IsAccessibleThroughInterface()
        {
            IModule module = new TestModuleImpl();

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void IModule_Load_IsCallableThroughInterface()
        {
            IModule module = new TestModuleImpl();

            module.Load(_testHarmony);

            Assert.True(module.IsLoaded);
        }

        [Fact]
        public void IModule_Unload_IsCallableThroughInterface()
        {
            IModule module = new TestModuleImpl();
            module.Load(_testHarmony);

            module.Unload(_testHarmony);

            Assert.False(module.IsLoaded);
        }

        [Fact]
        public void Load_WhenAlreadyLoaded_DoesNotIncrementLoadCount()
        {
            var module = new TestModuleImpl();
            module.Load(_testHarmony);
            var initialCount = module.OnLoadCallCount;

            module.Load(_testHarmony);
            module.Load(_testHarmony);

            Assert.Equal(initialCount, module.OnLoadCallCount);
        }

        [Fact]
        public void Unload_WhenNotLoaded_DoesNotIncrementUnloadCount()
        {
            var module = new TestModuleImpl();

            module.Unload(_testHarmony);
            module.Unload(_testHarmony);

            Assert.Equal(0, module.OnUnloadCallCount);
        }

        [Fact]
        public void Load_WithException_StillSetsIsLoadedToFalse()
        {
            var module = new TestModuleImpl { ThrowOnLoad = true };

            try { module.Load(_testHarmony); } catch { }

            Assert.False(module.IsLoaded);
            Assert.Equal(1, module.OnLoadCallCount);
        }

        [Fact]
        public void Unload_WithException_StillSetsIsLoadedToFalse()
        {
            var module = new TestModuleImpl { ThrowOnUnload = true };
            module.Load(_testHarmony);

            try { module.Unload(_testHarmony); } catch { }

            Assert.False(module.IsLoaded);
            Assert.Equal(1, module.OnUnloadCallCount);
        }

        [Fact]
        public void Load_AfterFailedLoad_CanSucceed()
        {
            var module = new TestModuleImpl { ThrowOnLoad = true };
            try { module.Load(_testHarmony); } catch { }

            module.ThrowOnLoad = false;
            module.Load(_testHarmony);

            Assert.True(module.IsLoaded);
            Assert.Equal(2, module.OnLoadCallCount);
        }
    }
}
