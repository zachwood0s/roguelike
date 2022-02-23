using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;

public class DamageAttributeSourceConsumer : AbstractAttributeSourceConsumer
{
    [SerializeField] private float burnDamageDec;
    public override EffectModifier CalculateActualEffectAmount(AbilitySystemController abilities, AttributeSystemController attributes, EffectModifier mod)
    {
        if (mod.Attribute != AbilitySystemDB.Instance.DamageSimple && mod.Attribute != AbilitySystemDB.Instance.DamageBurn)
            return mod;

        if (abilities.HasTagApplied(AbilitySystemDB.Instance.Burning))
        {
            // Temp: do less damage while burning
            mod.Value *= burnDamageDec;
        }
        return mod;
    }
}
