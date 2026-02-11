using Xunit;
using MageQuitModFramework.Spells;

namespace MageQuitModFramework.Tests
{
    public class AttributeModifierTests
    {
        [Fact]
        public void Constructor_SetsBaseValue()
        {
            var baseValue = 100f;
            
            var modifier = new AttributeModifier(baseValue);
            
            Assert.Equal(baseValue, modifier.Base);
        }

        [Fact]
        public void Constructor_SetsMultiplier()
        {
            var baseValue = 100f;
            var mult = 1.5f;
            
            var modifier = new AttributeModifier(baseValue, mult);
            
            Assert.Equal(mult, modifier.Mult);
        }

        [Fact]
        public void Constructor_DefaultMultiplierIsOne()
        {
            var modifier = new AttributeModifier(100f);
            
            Assert.Equal(1f, modifier.Mult);
        }

        [Fact]
        public void Value_ReturnsBaseTimesMultiplier()
        {
            var baseValue = 50f;
            var mult = 2f;
            var expected = baseValue * mult;
            
            var modifier = new AttributeModifier(baseValue, mult);
            
            Assert.Equal(expected, modifier.Value);
        }

        [Fact]
        public void ResetMultiplier_SetsMultiplierToOne()
        {
            var modifier = new AttributeModifier(100f, 3f);
            
            modifier.ResetMultiplier();
            
            Assert.Equal(1f, modifier.Mult);
        }

        [Fact]
        public void ResetMultiplier_DoesNotChangeBase()
        {
            var baseValue = 100f;
            var modifier = new AttributeModifier(baseValue, 3f);
            
            modifier.ResetMultiplier();
            
            Assert.Equal(baseValue, modifier.Base);
        }

        [Fact]
        public void ApplyMultiplier_MultipliesCurrentMultiplier()
        {
            var initialMult = 2f;
            var applyMult = 1.5f;
            var expectedMult = 3f;
            var modifier = new AttributeModifier(100f, initialMult);
            
            modifier.ApplyMultiplier(applyMult);
            
            Assert.Equal(expectedMult, modifier.Mult);
        }

        [Fact]
        public void ApplyMultiplier_UpdatesValue()
        {
            var baseValue = 10f;
            var initialMult = 2f;
            var applyMult = 3f;
            var expectedValue = 60f;
            var modifier = new AttributeModifier(baseValue, initialMult);
            
            modifier.ApplyMultiplier(applyMult);
            
            Assert.Equal(expectedValue, modifier.Value);
        }

        [Fact]
        public void AddMultiplier_AddsToCurrentMultiplier()
        {
            var initialMult = 1f;
            var addMult = 0.5f;
            var expectedMult = 1.5f;
            var modifier = new AttributeModifier(100f, initialMult);
            
            modifier.AddMultiplier(addMult);
            
            Assert.Equal(expectedMult, modifier.Mult);
        }

        [Fact]
        public void AddMultiplier_CanBeNegative()
        {
            var initialMult = 1.5f;
            var addMult = -0.3f;
            var expectedMult = 1.2f;
            var modifier = new AttributeModifier(100f, initialMult);
            
            modifier.AddMultiplier(addMult);
            
            Assert.Equal(expectedMult, modifier.Mult, precision: 5);
        }

        [Fact]
        public void ImplicitConversion_ReturnsValue()
        {
            var baseValue = 25f;
            var mult = 4f;
            var expected = baseValue * mult;
            var modifier = new AttributeModifier(baseValue, mult);
            
            float value = modifier;
            
            Assert.Equal(expected, value);
        }

        [Fact]
        public void MultipleOperations_CalculatesCorrectly()
        {
            var baseValue = 10f;
            var modifier = new AttributeModifier(baseValue, 2f);
            
            modifier.ApplyMultiplier(1.5f);
            modifier.AddMultiplier(0.5f);
            
            Assert.Equal(3.5f, modifier.Mult);
            Assert.Equal(baseValue * 3.5f, modifier.Value);
        }

        [Fact]
        public void SetBase_UpdatesValue()
        {
            var initialBase = 100f;
            var newBase = 200f;
            var mult = 2f;
            var modifier = new AttributeModifier(initialBase, mult);
            
            modifier.Base = newBase;
            
            Assert.Equal(newBase * mult, modifier.Value);
        }

        [Fact]
        public void SetMult_UpdatesValue()
        {
            var baseValue = 50f;
            var newMult = 3f;
            var modifier = new AttributeModifier(baseValue);
            
            modifier.Mult = newMult;
            
            Assert.Equal(baseValue * newMult, modifier.Value);
        }
    }
}
