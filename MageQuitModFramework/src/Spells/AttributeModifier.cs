using System;

namespace MageQuitModFramework.Spells
{
    /// <summary>
    /// Represents a modifiable spell attribute using Base × Mult pattern.
    /// </summary>
    /// <param name="baseValue">Original game value (immutable)</param>
    /// <param name="mult">Multiplier, starts at 1.0</param>
    public class AttributeModifier(float baseValue, float mult = 1f)
    {
        /// <summary>
        /// Original base value from game data.
        /// </summary>
        public float Base { get; set; } = baseValue;
        
        /// <summary>
        /// Multiplier applied to base value. Starts at 1.0.
        /// </summary>
        public float Mult { get; set; } = mult;
        
        /// <summary>
        /// Computed value: Base × Mult.
        /// </summary>
        public float Value => Base * Mult;

        /// <summary>
        /// Resets multiplier back to 1.0.
        /// </summary>
        public void ResetMultiplier()
        {
            Mult = 1f;
        }

        /// <summary>
        /// Multiplies the current multiplier by a value.
        /// </summary>
        /// <param name="multiplier">Multiplier to apply (2.0 = double current mult)</param>
        public void ApplyMultiplier(float multiplier)
        {
            Mult *= multiplier;
        }

        /// <summary>
        /// Adds a value to the multiplier (additive).
        /// </summary>
        /// <param name="additiveMultiplier">Value to add (0.5 = +50%, mult becomes 1.5 if was 1.0)</param>
        /// <example>
        /// <code>
        /// var mod = new AttributeModifier(10f);
        /// mod.AddMultiplier(0.5f);  // Mult: 1.0 → 1.5, Value: 10 → 15
        /// mod.AddMultiplier(0.25f); // Mult: 1.5 → 1.75, Value: 15 → 17.5
        /// </code>
        /// </example>
        public void AddMultiplier(float additiveMultiplier)
        {
            Mult += additiveMultiplier;
        }

        /// <summary>
        /// Implicit conversion to float returns the computed Value.
        /// </summary>
        public static implicit operator float(AttributeModifier mod) => mod.Value;
    }
}
