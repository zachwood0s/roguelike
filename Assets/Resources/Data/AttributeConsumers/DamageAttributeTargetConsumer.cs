using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;
using System.Linq;

public class DamageAttributeTargetConsumer : AbstractAttributeTargetConsumer
{
    public override void ConsumeEffects(AttributeSystemController attrs, AbilitySystemController abilities)
    {
        // Get all damage values
        var damageSimple = attrs.GetAttributeValue(AbilitySystemDB.Instance.DamageSimple);
        var damageBurn = attrs.GetAttributeValue(AbilitySystemDB.Instance.DamageBurn);

        var healthLost = damageSimple.CurrentValue + damageBurn.CurrentValue;
        var curHealth = attrs.GetAttributeValue(AbilitySystemDB.Instance.Health);

        // Modify health
        attrs.SetAttributeBaseValue(AbilitySystemDB.Instance.Health, curHealth.CurrentValue - healthLost);

        if (damageBurn.CurrentValue > 0)
        {
            // Apply burn
            abilities.ApplyGameEffectToSelfNoDelay(InstantiateBurn(abilities, damageBurn.CurrentValue));
        }

        // Consume all of the damage attributes
        attrs.SetAttributeBaseValue(AbilitySystemDB.Instance.DamageSimple, 0);
        attrs.SetAttributeBaseValue(AbilitySystemDB.Instance.DamageBurn, 0);
    }

    public InstantiatedGameEffect InstantiateBurn(AbilitySystemController ability, float damageBurn)
    {
        var burnEff = (GameEffect) StandardEffectsDB.Instance.BurnEffect.Clone();
        var burnMod = new EffectModifier()
        {
            Attribute = AbilitySystemDB.Instance.DamageSimple,
            ModifierOperation = AbilitySystem.ModifierType.Add,
            EffectCurve = new AnimationCurve(),
            Value = damageBurn * StandardEffectsDB.Instance.BurnPercBase
        };

        burnEff.Modifiers = new[] { burnMod };
        return new InstantiatedGameEffect(burnEff, ability, ability);
    }
}