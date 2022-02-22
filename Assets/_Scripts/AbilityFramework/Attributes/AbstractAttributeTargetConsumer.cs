using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbstractAttributeTargetConsumer : MonoBehaviour
    {
        public abstract void ConsumeEffects(AttributeSystemController attrs, AbilitySystemController cont);
    }
}
