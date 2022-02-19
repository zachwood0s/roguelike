using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public struct AttributeValue
    {
        public BaseAttribute Attribute;
        public float CurrentValue;
        public float BaseValue;
        public AttributeModifier Modifier;
    }

    public struct AttributeModifier
    {
        public BaseAttribute Attribute;
        public EffectModifier? BaseMod;
        public float Add;
        public float Multiply;
        public float Override;
        public float Percent;

        public AttributeModifier(BaseAttribute attr, EffectModifier? mod)
        {
            Attribute = attr;
            BaseMod = mod;
            Add = Multiply = Override = 0;
            Percent = 1;
        }

        public AttributeModifier(BaseAttribute attr, EffectModifier? mod, ModifierType t, float magnitude) 
            : this(attr, mod)
        {
            switch(t)
            {
                case ModifierType.Add:
                    Add = magnitude;
                    break;
                case ModifierType.Multiply:
                    Multiply = magnitude;
                    break;
                case ModifierType.Override:
                    Override = magnitude;
                    break;
                case ModifierType.Percent:
                    Percent = magnitude;
                    break;
            }
        }

        public AttributeModifier Combine(AttributeModifier other)
        {
            other.Add += Add;
            other.Multiply += Multiply;
            other.Override = Override;
            other.Percent *= Percent;
            return other;
        }

        public AttributeModifier ApplyAnimationCurve(AnimationCurve curve, ModifierType t, float time)
        {

            var multiplyer = curve.Evaluate(time);
            switch(t)
            {
                case ModifierType.Add:
                    Add *= multiplyer;
                    break;
                case ModifierType.Multiply:
                    Multiply *= multiplyer;
                    break;
                case ModifierType.Percent:
                    Percent *= multiplyer;
                    break;
            }
            return this;
        }
    }
}
