using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Ability System/Attribute")]
    public class BaseAttribute : ScriptableObject
    {
        public string Name;

        public virtual AttributeValue CalculateCurrentValue(AttributeValue val)
        {
            val.CurrentValue = (val.BaseValue + val.Modifier.Add) * (val.Modifier.Multiply + 1);

            // May need a better way of keeping track of if things are overwritten.
            // Probably some sort of bitmask when combining
            if (val.Modifier.Override != 0)
            {
                val.CurrentValue = val.Modifier.Override;
            }
            return val;
        }
    }
}
