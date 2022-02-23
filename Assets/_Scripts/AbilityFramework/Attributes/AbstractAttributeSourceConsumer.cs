using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbstractAttributeSourceConsumer : MonoBehaviour
    {
        public abstract EffectModifier CalculateActualEffectAmount(AbilitySystemController abilities, AttributeSystemController attributes, EffectModifier mod);
    }
}
