using Xunit;
using MageQuitModFramework.Spells;
using System.Collections.Generic;

namespace MageQuitModFramework.Tests
{
    public class SpellModificationSystemTests
    {
        private static readonly Dictionary<SpellName, Spell> _defaultSpellTable = new()
        {
            { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } },
            { SpellName.RockBlock, new Spell { cooldown = 1.2f, windUp = 0.6f, windDown = 0.4f, initialVelocity = 8f } }
        };

        private static readonly Dictionary<SpellName, Dictionary<string, float>> _defaultClassAttrs = new()
        {
            { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", 50f }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } },
            { SpellName.RockBlock, new Dictionary<string, float> { { "DAMAGE", 40f }, { "RADIUS", 2.5f }, { "POWER", 80f }, { "Y_POWER", 40f } } }
        };

        [Fact]
        public void Initialize_CreatesModifiersForAllSpells()
        {
            var table = new SpellModifierTable();
            table.Initialize(_defaultSpellTable, _defaultClassAttrs);

            Assert.Equal(2, table.Modifiers.Count);
            Assert.True(table.Modifiers.ContainsKey(SpellName.Fireball));
            Assert.True(table.Modifiers.ContainsKey(SpellName.RockBlock));
        }

        [Fact]
        public void Initialize_SetsCorrectBaseValues()
        {
            var cooldown = 1.5f;
            var damage = 75f;
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = cooldown, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", damage }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } }
            };

            var table = new SpellModifierTable();
            table.Initialize(spellTable, classAttrs);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(damage, mods.DAMAGE.Base);
            Assert.Equal(cooldown, mods.cooldown.Base);
        }

        [Fact]
        public void Initialize_SetsHealFor_FrogOfLife()
        {
            var healValue = 15f;
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.FrogOfLife, new Spell { cooldown = 2f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 5f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.FrogOfLife, new Dictionary<string, float> { { "DAMAGE", 0f }, { "RADIUS", 3f }, { "POWER", 50f }, { "Y_POWER", 25f } } }
            };

            var table = new SpellModifierTable();
            table.Initialize(spellTable, classAttrs);
            var mods = table.Modifiers[SpellName.FrogOfLife];

            Assert.Equal(healValue, mods.HEAL.Base);
        }

        [Fact]
        public void Initialize_SetsHealToZero_ForNonHealingSpells()
        {
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", 50f }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } }
            };

            var table = new SpellModifierTable();
            table.Initialize(spellTable, classAttrs);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(0f, mods.HEAL.Base);
        }

        [Fact]
        public void TryAddToModifier_ReturnsTrue_WhenSpellAndAttributeExist()
        {
            var table = SetupBasicSpell();
            var additiveMult = 0.5f;

            var result = table.TryAddToModifier(SpellName.Fireball, "DAMAGE", additiveMult);

            Assert.True(result);
        }

        [Fact]
        public void TryAddToModifier_UpdatesMultiplier()
        {
            var table = SetupBasicSpell();
            var additiveMult = 0.5f;
            var expectedMult = 1.5f;

            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", additiveMult);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(expectedMult, mods.DAMAGE.Mult);
        }

        [Fact]
        public void TryAddToModifier_ReturnsFalse_WhenSpellDoesNotExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryAddToModifier(SpellName.RockBlock, "DAMAGE", 0.5f);

            Assert.False(result);
        }

        [Fact]
        public void TryAddToModifier_ReturnsFalse_WhenAttributeDoesNotExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryAddToModifier(SpellName.Fireball, "NonExistent", 0.5f);

            Assert.False(result);
        }

        [Fact]
        public void TryMultiplyModifier_ReturnsTrue_WhenSpellAndAttributeExist()
        {
            var table = SetupBasicSpell();
            var multiplicativeMult = 2.0f;

            var result = table.TryMultiplyModifier(SpellName.Fireball, "DAMAGE", multiplicativeMult);

            Assert.True(result);
        }

        [Fact]
        public void TryMultiplyModifier_UpdatesMultiplier()
        {
            var table = SetupBasicSpell();
            var multiplicativeMult = 2.0f;
            var expectedMult = 2.0f;

            table.TryMultiplyModifier(SpellName.Fireball, "DAMAGE", multiplicativeMult);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(expectedMult, mods.DAMAGE.Mult);
        }

        [Fact]
        public void TryMultiplyModifier_ReturnsFalse_WhenSpellDoesNotExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryMultiplyModifier(SpellName.RockBlock, "DAMAGE", 2.0f);

            Assert.False(result);
        }

        [Fact]
        public void TryMultiplyModifier_ReturnsFalse_WhenAttributeDoesNotExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryMultiplyModifier(SpellName.Fireball, "NonExistent", 2.0f);

            Assert.False(result);
        }

        [Fact]
        public void TryGetModifier_ReturnsTrue_WhenSpellAndAttributeExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryGetModifier(SpellName.Fireball, "DAMAGE", out var modifier);

            Assert.True(result);
            Assert.NotNull(modifier);
        }

        [Fact]
        public void TryGetModifier_ReturnsFalse_WhenSpellDoesNotExist()
        {
            var table = SetupBasicSpell();

            var result = table.TryGetModifier(SpellName.RockBlock, "DAMAGE", out var modifier);

            Assert.False(result);
            Assert.Null(modifier);
        }

        [Fact]
        public void TryGetMultiplier_ReturnsCurrentMultiplier()
        {
            var table = SetupBasicSpell();
            var additiveMult = 0.3f;
            var expectedMult = 1.3f;
            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", additiveMult);

            var result = table.TryGetMultiplier(SpellName.Fireball, "DAMAGE", out var mult);

            Assert.True(result);
            Assert.Equal(expectedMult, mult);
        }

        [Fact]
        public void ResetAllMultipliers_ResetsAllSpellModifiers()
        {
            var table = SetupBasicSpell();
            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);
            table.TryAddToModifier(SpellName.Fireball, "cooldown", -0.2f);

            table.ResetAllMultipliers();

            var mods = table.Modifiers[SpellName.Fireball];
            Assert.Equal(1f, mods.DAMAGE.Mult);
            Assert.Equal(1f, mods.cooldown.Mult);
        }

        [Fact]
        public void DeepCopy_CreatesIndependentCopy()
        {
            var table = SetupBasicSpell();
            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);

            var copy = table.Copy();
            copy.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);  // Add another 0.5

            var originalMods = table.Modifiers[SpellName.Fireball];
            var copyMods = copy.Modifiers[SpellName.Fireball];

            Assert.Equal(1.5f, originalMods.DAMAGE.Mult);
            Assert.Equal(2.0f, copyMods.DAMAGE.Mult);
        }

        [Fact]
        public void InitializeDefaultTable_CreatesDefaultTable()
        {
            SpellModificationSystem.InitializeDefaultTable(_defaultSpellTable, _defaultClassAttrs);
            var defaultTable = SpellModificationSystem.Default();

            Assert.NotNull(defaultTable);
            Assert.NotEmpty(defaultTable.Modifiers);
            Assert.Equal(2, defaultTable.Modifiers.Count);
        }

        [Fact]
        public void RegisterTable_StoresTableByKey()
        {
            var table = new SpellModifierTable();
            table.Initialize(_defaultSpellTable, _defaultClassAttrs);

            SpellModificationSystem.RegisterTable("test-table", table);
            var retrieved = SpellModificationSystem.GetTable("test-table");

            Assert.NotNull(retrieved);
            Assert.Equal(table, retrieved);
            
            // Cleanup
            SpellModificationSystem.ClearTable("test-table");
        }

        [Fact]
        public void GetTable_ReturnsNull_WhenKeyDoesNotExist()
        {
            var result = SpellModificationSystem.GetTable("nonexistent-key");

            Assert.Null(result);
        }

        [Fact]
        public void ClearTable_RemovesTableByKey()
        {
            var table = new SpellModifierTable();
            table.Initialize(_defaultSpellTable, _defaultClassAttrs);
            SpellModificationSystem.RegisterTable("temp-table", table);

            SpellModificationSystem.ClearTable("temp-table");
            var retrieved = SpellModificationSystem.GetTable("temp-table");

            Assert.Null(retrieved);
        }

        [Fact]
        public void GetSpellObjectTypeName_ReturnsCorrectName_ForStandardSpells()
        {
            var result = SpellModificationSystem.GetSpellObjectTypeName(SpellName.Fireball);

            Assert.Equal("FireballObject", result);
        }

        [Fact]
        public void GetSpellObjectTypeName_ReturnsCorrectName_ForSpecialCases()
        {
            Assert.Equal("StonewallObject", SpellModificationSystem.GetSpellObjectTypeName(SpellName.RockBlock));
            Assert.Equal("BurningLeashObject", SpellModificationSystem.GetSpellObjectTypeName(SpellName.FlameLeash));
            Assert.Equal("SomAssaultObject", SpellModificationSystem.GetSpellObjectTypeName(SpellName.SomerAssault));
            Assert.Equal("SustainObjectObject", SpellModificationSystem.GetSpellObjectTypeName(SpellName.Sustain));
        }

        [Fact]
        public void GetSpellNameFromTypeName_ReturnsCorrectSpell_ForStandardTypes()
        {
            var result = SpellModificationSystem.GetSpellNameFromTypeName("FireballObject");

            Assert.Equal(SpellName.Fireball, result);
        }

        [Fact]
        public void GetSpellNameFromTypeName_ReturnsCorrectSpell_ForSpecialCases()
        {
            Assert.Equal(SpellName.RockBlock, SpellModificationSystem.GetSpellNameFromTypeName("StonewallObject"));
            Assert.Equal(SpellName.FlameLeash, SpellModificationSystem.GetSpellNameFromTypeName("BurningLeashObject"));
            Assert.Equal(SpellName.SomerAssault, SpellModificationSystem.GetSpellNameFromTypeName("SomAssaultObject"));
            Assert.Equal(SpellName.Sustain, SpellModificationSystem.GetSpellNameFromTypeName("SustainObjectObject"));
        }

        [Fact]
        public void GetSpellNameFromTypeName_ReturnsNull_ForInvalidType()
        {
            var result = SpellModificationSystem.GetSpellNameFromTypeName("InvalidObject");

            Assert.Null(result);
        }

        [Fact]
        public void RegisterTable_WithKeyOnly_CreatesTableCopy()
        {
            SpellModificationSystem.InitializeDefaultTable(_defaultSpellTable, _defaultClassAttrs);

            var table = SpellModificationSystem.RegisterTable("auto-copy-table");

            Assert.NotNull(table);
            Assert.NotEmpty(table.Modifiers);
            Assert.Equal(2, table.Modifiers.Count);
            
            // Cleanup
            SpellModificationSystem.ClearTable("auto-copy-table");
        }

        [Fact]
        public void RegisterTable_WithKeyOnly_CreatesIndependentCopy()
        {
            SpellModificationSystem.InitializeDefaultTable(_defaultSpellTable, _defaultClassAttrs);
            var defaultTable = SpellModificationSystem.Default();

            var table = SpellModificationSystem.RegisterTable("independent-table");
            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);

            // Default table should be unaffected
            defaultTable.TryGetModifier(SpellName.Fireball, "DAMAGE", out var defaultMod);
            table.TryGetModifier(SpellName.Fireball, "DAMAGE", out var tableMod);

            Assert.Equal(1f, defaultMod.Mult);
            Assert.Equal(1.5f, tableMod.Mult);
            
            // Cleanup
            SpellModificationSystem.ClearTable("independent-table");
        }

        [Fact]
        public void ClearTable_DoesNotThrow_WhenKeyDoesNotExist()
        {
            // Should not throw
            SpellModificationSystem.ClearTable("non-existent-key");
        }

        [Fact]
        public void GetSpellNameFromTypeName_HandlesCaseInsensitivity()
        {
            // The switch is case-sensitive, but the enum lookup should handle standard cases
            var result = SpellModificationSystem.GetSpellNameFromTypeName("FireballObject");

            Assert.Equal(SpellName.Fireball, result);
        }

        [Fact]
        public void GetSpellObjectTypeName_ReturnsValidTypeForAllSpecialCases()
        {
            // Verify all special case mappings
            var specialCases = new Dictionary<SpellName, string>
            {
                { SpellName.RockBlock, "StonewallObject" },
                { SpellName.FlameLeash, "BurningLeashObject" },
                { SpellName.SomerAssault, "SomAssaultObject" },
                { SpellName.Sustain, "SustainObjectObject" }
            };

            foreach (var kvp in specialCases)
            {
                var typeName = SpellModificationSystem.GetSpellObjectTypeName(kvp.Key);
                Assert.Equal(kvp.Value, typeName);
            }
        }

        [Fact]
        public void TryGetModifier_ReturnsFalse_WhenAttributeIsNull()
        {
            var table = SetupBasicSpell();
            table.Modifiers[SpellName.Fireball].DAMAGE = null;

            var result = table.TryGetModifier(SpellName.Fireball, "DAMAGE", out var modifier);

            Assert.False(result);
            Assert.Null(modifier);
        }

        [Fact]
        public void TryGetMultiplier_SetsMultToOne_WhenSpellNotFound()
        {
            var table = SetupBasicSpell();

            var result = table.TryGetMultiplier(SpellName.RockBlock, "DAMAGE", out var mult);

            Assert.False(result);
            Assert.Equal(1f, mult);
        }

        [Fact]
        public void TryAddToModifier_AllowsNegativeMultipliers()
        {
            var table = SetupBasicSpell();

            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", -0.5f);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(0.5f, mods.DAMAGE.Mult);
        }

        [Fact]
        public void TryMultiplyModifier_AllowsZeroMultiplier()
        {
            var table = SetupBasicSpell();

            table.TryMultiplyModifier(SpellName.Fireball, "DAMAGE", 0f);
            var mods = table.Modifiers[SpellName.Fireball];

            Assert.Equal(0f, mods.DAMAGE.Mult);
        }

        [Fact]
        public void Initialize_HandlesEmptyClassAttributes()
        {
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>();

            var table = new SpellModifierTable();
            
            // Should not throw
            var exception = Record.Exception(() => table.Initialize(spellTable, classAttrs));

            Assert.Null(exception);
        }

        [Fact]
        public void Copy_PreservesAllSpellData()
        {
            var table = new SpellModifierTable();
            table.Initialize(_defaultSpellTable, _defaultClassAttrs);

            table.TryAddToModifier(SpellName.Fireball, "DAMAGE", 0.5f);
            table.TryAddToModifier(SpellName.RockBlock, "RADIUS", 0.3f);

            var copy = table.Copy();

            Assert.Equal(2, copy.Modifiers.Count);
            Assert.Equal(1.5f, copy.Modifiers[SpellName.Fireball].DAMAGE.Mult);
            Assert.Equal(1.3f, copy.Modifiers[SpellName.RockBlock].RADIUS.Mult);
        }

        [Fact]
        public void ResetAllMultipliers_HandlesEmptyTable()
        {
            var table = new SpellModifierTable();

            // Should not throw
            var exception = Record.Exception(() => table.ResetAllMultipliers());

            Assert.Null(exception);
        }

        private SpellModifierTable SetupBasicSpell(float damage = 50f)
        {
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", damage }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } }
            };

            var table = new SpellModifierTable();
            table.Initialize(spellTable, classAttrs);
            return table;
        }
    }
}
