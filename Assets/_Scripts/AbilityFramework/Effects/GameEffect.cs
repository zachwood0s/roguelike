using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbilitySystem
{
    [Serializable]
    public class GameEffect
    {
        public DurationType DurationStyle;
        public SN<int> IsRelativeToIndex;
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

        // Gets filled in during ability instantiation
        [NonSerialized] public List<GameEffect> PostEffects = new List<GameEffect>();

        public static List<GameEffect> LinkGameEffects(List<GameEffect> effects)
        {
            var res = new List<GameEffect>();
            foreach (var e in effects)
            {
                // Clear them out
                e.PostEffects = new List<GameEffect>();
            }
            foreach (var e in effects)
            {
                if (e.IsRelativeToIndex.HasValue)
                {
                    effects[e.IsRelativeToIndex.Value].PostEffects.Add(e);
                }
                else
                {
                    res.Add(e);
                }
            }
            return res;
        }
    }


    [Serializable]
    public struct EffectModifier
    {
        public BaseAttribute Attribute;
        public AnimationCurve EffectCurve;
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
        Add, Multiply, Override, Percent
    }
}
