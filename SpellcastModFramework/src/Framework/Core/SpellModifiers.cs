using System.Reflection;

namespace SpellcastModFramework.Core
{
    public class SpellModifiers
    {
        public AttributeModifier DAMAGE { get; set; }
        public AttributeModifier RADIUS { get; set; }
        public AttributeModifier POWER { get; set; }
        public AttributeModifier Y_POWER { get; set; }
        public AttributeModifier cooldown { get; set; }
        public AttributeModifier windUp { get; set; }
        public AttributeModifier windDown { get; set; }
        public AttributeModifier initialVelocity { get; set; }
        public AttributeModifier HEAL { get; set; }

        public void ResetAllMultipliers()
        {
            DAMAGE.ResetMultiplier();
            RADIUS.ResetMultiplier();
            POWER.ResetMultiplier();
            Y_POWER.ResetMultiplier();
            cooldown.ResetMultiplier();
            windUp.ResetMultiplier();
            windDown.ResetMultiplier();
            initialVelocity.ResetMultiplier();
            HEAL.ResetMultiplier();
        }

        public bool TryGetModifier(string attributeName, out AttributeModifier modifier)
        {
            modifier = null;
            var prop = typeof(SpellModifiers).GetProperty(attributeName);
            if (prop?.GetValue(this) is AttributeModifier attrMod)
            {
                modifier = attrMod;
                return true;
            }
            return false;
        }

        public bool TrySetModifier(string attributeName, AttributeModifier modifier)
        {
            var prop = typeof(SpellModifiers).GetProperty(attributeName);
            if (prop != null && prop.PropertyType == typeof(AttributeModifier))
            {
                prop.SetValue(this, modifier);
                return true;
            }
            return false;
        }
    }
}
