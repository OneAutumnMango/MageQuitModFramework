using System;

namespace SpellcastModFramework.Core
{
    public class AttributeModifier(float baseValue, float mult = 1f)
    {
        public float Base { get; set; } = baseValue;
        public float Mult { get; set; } = mult;
        public float Value => Base * Mult;

        public void ResetMultiplier()
        {
            Mult = 1f;
        }

        public void ApplyMultiplier(float multiplier)
        {
            Mult *= multiplier;
        }

        public void AddMultiplier(float additiveMultiplier)
        {
            Mult += additiveMultiplier;
        }

        public static implicit operator float(AttributeModifier mod) => mod.Value;
    }
}
