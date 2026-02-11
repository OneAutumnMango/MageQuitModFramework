using System.Reflection;

namespace MageQuitModFramework.Spells
{
    /// <summary>
    /// Container for all modifiable attributes of a spell.
    /// Includes both SpellObject class fields (DAMAGE, RADIUS, etc.) and spell table properties (cooldown, windUp, etc.).
    /// </summary>
    public class SpellModifiers
    {
        /// <summary>Damage dealt by the spell.</summary>
        public AttributeModifier DAMAGE { get; set; }
        
        /// <summary>Effect radius of the spell.</summary>
        public AttributeModifier RADIUS { get; set; }
        
        /// <summary>Horizontal knockback power.</summary>
        public AttributeModifier POWER { get; set; }
        
        /// <summary>Vertical knockback power.</summary>
        public AttributeModifier Y_POWER { get; set; }
        
        /// <summary>Spell cooldown duration.</summary>
        public AttributeModifier cooldown { get; set; }
        
        /// <summary>Cast time before spell activates.</summary>
        public AttributeModifier windUp { get; set; }
        
        /// <summary>Recovery time after spell cast.</summary>
        public AttributeModifier windDown { get; set; }
        
        /// <summary>Initial velocity of projectile spells.</summary>
        public AttributeModifier initialVelocity { get; set; }
        
        /// <summary>Heal amount (Frog of Life only).</summary>
        public AttributeModifier HEAL { get; set; }

        /// <summary>
        /// Resets all attribute multipliers to 1.0, keeping base modifiers intact.
        /// </summary>
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

        /// <summary>
        /// Attempts to retrieve an attribute modifier by name using reflection.
        /// </summary>
        /// <param name="attributeName">The name of the attribute property</param>
        /// <param name="modifier">Output parameter for the modifier if found</param>
        /// <returns>True if the attribute exists, false otherwise</returns>
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

        /// <summary>
        /// Attempts to set an attribute modifier by name using reflection.
        /// </summary>
        /// <param name="attributeName">The name of the attribute property</param>
        /// <param name="modifier">The modifier to assign</param>
        /// <returns>True if the attribute exists and was set, false otherwise</returns>
        public bool TrySetModifier(string attributeName, AttributeModifier modifier)
        {
            var prop = typeof(SpellModifiers).GetProperty(attributeName);
            if (prop?.PropertyType == typeof(AttributeModifier))
            {
                prop.SetValue(this, modifier);
                return true;
            }
            return false;
        }
    }
}
