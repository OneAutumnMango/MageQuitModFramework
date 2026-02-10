using Xunit;
using MageQuitModFramework.Core;
using System.Collections.Generic;

namespace MageQuitModFramework.Tests
{
    public class SpellModificationSystemTests
    {
        [Fact]
        public void Initialize_CreatesModifiersForAllSpells()
        {
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } },
                { SpellName.RockBlock, new Spell { cooldown = 1.2f, windUp = 0.6f, windDown = 0.4f, initialVelocity = 8f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", 50f }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } },
                { SpellName.RockBlock, new Dictionary<string, float> { { "DAMAGE", 40f }, { "RADIUS", 2.5f }, { "POWER", 80f }, { "Y_POWER", 40f } } }
            };

            SpellModificationSystem.Initialize(spellTable, classAttrs);

            Assert.Equal(2, SpellModificationSystem.SpellModifierTable.Count);
            Assert.True(SpellModificationSystem.SpellModifierTable.ContainsKey(SpellName.Fireball));
            Assert.True(SpellModificationSystem.SpellModifierTable.ContainsKey(SpellName.RockBlock));
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

            SpellModificationSystem.Initialize(spellTable, classAttrs);
            var mods = SpellModificationSystem.SpellModifierTable[SpellName.Fireball];

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

            SpellModificationSystem.Initialize(spellTable, classAttrs);
            var mods = SpellModificationSystem.SpellModifierTable[SpellName.FrogOfLife];

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

            SpellModificationSystem.Initialize(spellTable, classAttrs);
            var mods = SpellModificationSystem.SpellModifierTable[SpellName.Fireball];

            Assert.Equal(0f, mods.HEAL.Base);
        }

        [Fact]
        public void TryUpdateModifier_ReturnsTrue_WhenSpellAndAttributeExist()
        {
            SetupBasicSpell();
            var additiveMult = 0.5f;

            var result = SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", additiveMult);

            Assert.True(result);
        }

        [Fact]
        public void TryUpdateModifier_UpdatesMultiplier()
        {
            SetupBasicSpell();
            var additiveMult = 0.5f;
            var expectedMult = 1.5f;

            SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", additiveMult);
            var mods = SpellModificationSystem.SpellModifierTable[SpellName.Fireball];

            Assert.Equal(expectedMult, mods.DAMAGE.Mult);
        }

        [Fact]
        public void TryUpdateModifier_ReturnsFalse_WhenSpellDoesNotExist()
        {
            SetupBasicSpell();

            var result = SpellModificationSystem.TryUpdateModifier(SpellName.RockBlock, "DAMAGE", 0.5f);

            Assert.False(result);
        }

        [Fact]
        public void TryUpdateModifier_ReturnsFalse_WhenAttributeDoesNotExist()
        {
            SetupBasicSpell();

            var result = SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "NonExistent", 0.5f);

            Assert.False(result);
        }

        [Fact]
        public void TryGetModifier_ReturnsTrue_WhenSpellAndAttributeExist()
        {
            SetupBasicSpell();

            var result = SpellModificationSystem.TryGetModifier(SpellName.Fireball, "DAMAGE", out var modifier);

            Assert.True(result);
            Assert.NotNull(modifier);
        }

        [Fact]
        public void TryGetModifier_ReturnsFalse_WhenSpellDoesNotExist()
        {
            SetupBasicSpell();

            var result = SpellModificationSystem.TryGetModifier(SpellName.RockBlock, "DAMAGE", out var modifier);

            Assert.False(result);
            Assert.Null(modifier);
        }

        [Fact]
        public void TryGetDefaultValue_ReturnsBaseValue()
        {
            var damage = 75f;
            SetupBasicSpell(damage);

            var result = SpellModificationSystem.TryGetDefaultValue(SpellName.Fireball, "DAMAGE", out var value);

            Assert.True(result);
            Assert.Equal(damage, value);
        }

        [Fact]
        public void TryGetMultiplier_ReturnsCurrentMultiplier()
        {
            SetupBasicSpell();
            var additiveMult = 0.3f;
            var expectedMult = 1.3f;
            SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", additiveMult);

            var result = SpellModificationSystem.TryGetMultiplier(SpellName.Fireball, "DAMAGE", out var mult);

            Assert.True(result);
            Assert.Equal(expectedMult, mult);
        }

        [Fact]
        public void ResetAllMultipliers_ResetsAllSpellModifiers()
        {
            SetupBasicSpell();
            SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "DAMAGE", 0.5f);
            SpellModificationSystem.TryUpdateModifier(SpellName.Fireball, "cooldown", -0.2f);

            SpellModificationSystem.ResetAllMultipliers();

            var mods = SpellModificationSystem.SpellModifierTable[SpellName.Fireball];
            Assert.Equal(1f, mods.DAMAGE.Mult);
            Assert.Equal(1f, mods.cooldown.Mult);
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

        private void SetupBasicSpell(float damage = 50f)
        {
            var spellTable = new Dictionary<SpellName, Spell>
            {
                { SpellName.Fireball, new Spell { cooldown = 1f, windUp = 0.5f, windDown = 0.3f, initialVelocity = 10f } }
            };
            var classAttrs = new Dictionary<SpellName, Dictionary<string, float>>
            {
                { SpellName.Fireball, new Dictionary<string, float> { { "DAMAGE", damage }, { "RADIUS", 3f }, { "POWER", 100f }, { "Y_POWER", 50f } } }
            };

            SpellModificationSystem.Initialize(spellTable, classAttrs);
        }
    }
}
