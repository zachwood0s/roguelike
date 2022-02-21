using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbstractEffectTargetConsumer : MonoBehaviour
    {
        public abstract void ConsumeEffects(AbilitySystemController cont);
    }
}
