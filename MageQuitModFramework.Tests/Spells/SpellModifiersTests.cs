using Xunit;
using MageQuitModFramework.Spells;

namespace MageQuitModFramework.Tests
{
    public class SpellModifiersTests
    {
        [Fact]
        public void TryGetModifier_ReturnsTrue_WhenPropertyExists()
        {
            var damage = new AttributeModifier(100f);
            var modifiers = new SpellModifiers { DAMAGE = damage };

            var result = modifiers.TryGetModifier("DAMAGE", out var modifier);

            Assert.True(result);
            Assert.Same(damage, modifier);
        }

        [Fact]
        public void TryGetModifier_ReturnsFalse_WhenPropertyDoesNotExist()
        {
            var modifiers = new SpellModifiers();

            var result = modifiers.TryGetModifier("NonExistent", out var modifier);

            Assert.False(result);
            Assert.Null(modifier);
        }

        [Fact]
        public void TryGetModifier_ReturnsFalse_WhenPropertyIsNull()
        {
            var modifiers = new SpellModifiers { DAMAGE = null };

            var result = modifiers.TryGetModifier("DAMAGE", out var modifier);

            Assert.False(result);
            Assert.Null(modifier);
        }

        [Fact]
        public void TrySetModifier_ReturnsTrue_WhenPropertyExists()
        {
            var modifiers = new SpellModifiers();
            var newModifier = new AttributeModifier(200f);

            var result = modifiers.TrySetModifier("RADIUS", newModifier);

            Assert.True(result);
            Assert.Same(newModifier, modifiers.RADIUS);
        }

        [Fact]
        public void TrySetModifier_ReturnsFalse_WhenPropertyDoesNotExist()
        {
            var modifiers = new SpellModifiers();
            var newModifier = new AttributeModifier(200f);

            var result = modifiers.TrySetModifier("NonExistent", newModifier);

            Assert.False(result);
        }

        [Fact]
        public void TrySetModifier_UpdatesExistingModifier()
        {
            var oldModifier = new AttributeModifier(100f);
            var newModifier = new AttributeModifier(200f);
            var modifiers = new SpellModifiers { POWER = oldModifier };

            modifiers.TrySetModifier("POWER", newModifier);

            Assert.Same(newModifier, modifiers.POWER);
            Assert.NotSame(oldModifier, modifiers.POWER);
        }

        [Fact]
        public void ResetAllMultipliers_ResetsAllModifiers()
        {
            var modifiers = new SpellModifiers
            {
                DAMAGE          = new AttributeModifier(100f, 2f),
                RADIUS          = new AttributeModifier(50f, 1.5f),
                POWER           = new AttributeModifier(200f, 3f),
                Y_POWER         = new AttributeModifier(150f, 2.5f),
                cooldown        = new AttributeModifier(1f, 0.5f),
                windUp          = new AttributeModifier(2f, 1.2f),
                windDown        = new AttributeModifier(3f, 1.8f),
                initialVelocity = new AttributeModifier(10f, 4f),
                HEAL            = new AttributeModifier(75f, 2.2f)
            };

            modifiers.ResetAllMultipliers();

            Assert.Equal(1f, modifiers.DAMAGE.Mult);
            Assert.Equal(1f, modifiers.RADIUS.Mult);
            Assert.Equal(1f, modifiers.POWER.Mult);
            Assert.Equal(1f, modifiers.Y_POWER.Mult);
            Assert.Equal(1f, modifiers.cooldown.Mult);
            Assert.Equal(1f, modifiers.windUp.Mult);
            Assert.Equal(1f, modifiers.windDown.Mult);
            Assert.Equal(1f, modifiers.initialVelocity.Mult);
            Assert.Equal(1f, modifiers.HEAL.Mult);
        }

        [Fact]
        public void ResetAllMultipliers_DoesNotChangeBaseValues()
        {
            var damageBase = 100f;
            var radiusBase = 50f;
            var modifiers = new SpellModifiers
            {
                DAMAGE          = new AttributeModifier(damageBase, 2f),
                RADIUS          = new AttributeModifier(radiusBase, 1.5f),
                POWER           = new AttributeModifier(200f, 3f),
                Y_POWER         = new AttributeModifier(150f, 2.5f),
                cooldown        = new AttributeModifier(1f, 0.5f),
                windUp          = new AttributeModifier(2f, 1.2f),
                windDown        = new AttributeModifier(3f, 1.8f),
                initialVelocity = new AttributeModifier(10f, 4f),
                HEAL            = new AttributeModifier(75f, 2.2f)
            };

            modifiers.ResetAllMultipliers();

            Assert.Equal(damageBase, modifiers.DAMAGE.Base);
            Assert.Equal(radiusBase, modifiers.RADIUS.Base);
        }

        [Fact]
        public void TryGetModifier_WorksForAllProperties()
        {
            var damage   = new AttributeModifier(1f);
            var radius   = new AttributeModifier(2f);
            var power    = new AttributeModifier(3f);
            var yPower   = new AttributeModifier(4f);
            var cooldown = new AttributeModifier(5f);
            var windUp   = new AttributeModifier(6f);
            var windDown = new AttributeModifier(7f);
            var velocity = new AttributeModifier(8f);
            var heal     = new AttributeModifier(9f);

            var modifiers = new SpellModifiers
            {
                DAMAGE          = damage,
                RADIUS          = radius,
                POWER           = power,
                Y_POWER         = yPower,
                cooldown        = cooldown,
                windUp          = windUp,
                windDown        = windDown,
                initialVelocity = velocity,
                HEAL            = heal
            };

            Assert.True(modifiers.TryGetModifier("DAMAGE", out var d) && d == damage);
            Assert.True(modifiers.TryGetModifier("RADIUS", out var r) && r == radius);
            Assert.True(modifiers.TryGetModifier("POWER", out var p) && p == power);
            Assert.True(modifiers.TryGetModifier("Y_POWER", out var yp) && yp == yPower);
            Assert.True(modifiers.TryGetModifier("cooldown", out var cd) && cd == cooldown);
            Assert.True(modifiers.TryGetModifier("windUp", out var wu) && wu == windUp);
            Assert.True(modifiers.TryGetModifier("windDown", out var wd) && wd == windDown);
            Assert.True(modifiers.TryGetModifier("initialVelocity", out var iv) && iv == velocity);
            Assert.True(modifiers.TryGetModifier("HEAL", out var h) && h == heal);
        }

        [Fact]
        public void Copy_CreatesDeepCopy()
        {
            var original = new SpellModifiers
            {
                DAMAGE          = new AttributeModifier(100f, 2f),
                RADIUS          = new AttributeModifier(50f, 1.5f),
                POWER           = new AttributeModifier(200f, 3f),
                Y_POWER         = new AttributeModifier(150f, 2.5f),
                cooldown        = new AttributeModifier(1f, 0.5f),
                windUp          = new AttributeModifier(2f, 1.2f),
                windDown        = new AttributeModifier(3f, 1.8f),
                initialVelocity = new AttributeModifier(10f, 4f),
                HEAL            = new AttributeModifier(75f, 2.2f)
            };

            var copy = original.Copy();

            Assert.Equal(original.DAMAGE.Base, copy.DAMAGE.Base);
            Assert.Equal(original.DAMAGE.Mult, copy.DAMAGE.Mult);
            Assert.Equal(original.RADIUS.Value, copy.RADIUS.Value);
            Assert.NotSame(original.DAMAGE, copy.DAMAGE);
        }

        [Fact]
        public void Copy_ChangesToCopyDoNotAffectOriginal()
        {
            var original = new SpellModifiers
            {
                DAMAGE = new AttributeModifier(100f, 2f),
                RADIUS = new AttributeModifier(50f, 1.5f),
                POWER  = new AttributeModifier(200f, 3f),
                Y_POWER = new AttributeModifier(150f, 2.5f),
                cooldown = new AttributeModifier(1f, 0.5f),
                windUp = new AttributeModifier(2f, 1.2f),
                windDown = new AttributeModifier(3f, 1.8f),
                initialVelocity = new AttributeModifier(10f, 4f),
                HEAL = new AttributeModifier(75f, 2.2f)
            };

            var copy = original.Copy();
            copy.DAMAGE.AddMultiplier(1f);

            Assert.Equal(2f, original.DAMAGE.Mult);
            Assert.Equal(3f, copy.DAMAGE.Mult);
        }
    }
}
