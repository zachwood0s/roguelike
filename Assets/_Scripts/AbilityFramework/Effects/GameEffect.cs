using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [Serializable]
    public struct GameEffect
    {
        public DurationType DurationStyle;
        public float Delay;
        public float Duration;

        public EffectModifier[] Modifiers;

        /// <summary>
        /// Tags that are granted when this effect is triggered and taken away
        /// when the effect ends
        /// </summary>
        public GameTag[] GrantedTags;

        /// <summary>
        /// The time between update ticks
        /// </summary>
        public float Period;
    }

    [Serializable]
    public struct EffectModifier
    {
        public BaseAttribute Attribute;
        public ModifierType ModifierOperation;
        public float Value;
    }

    public enum DurationType
    {
        Instant,
        Timed
    }

    public enum ModifierType
    {
        Add, Multiply, Override
    }
}
