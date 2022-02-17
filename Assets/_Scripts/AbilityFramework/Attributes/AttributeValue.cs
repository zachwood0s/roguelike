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
        public float Add;
        public float Multiply;
        public float Override;

        public AttributeModifier(BaseAttribute attr, ModifierType t, float magnitude)
        {
            Attribute = attr;
            Add = Multiply = Override = 0;

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
            }
        }

        public AttributeModifier Combine(AttributeModifier other)
        {
            other.Add += Add;
            other.Multiply += Multiply;
            other.Override = Override;
            return other;
        }
    }
}
