using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Ability System/Simple Ability")]
    public class SimpleAbility : AbstractAbility
    {
        public override AbstractInstantiatedAbility InstantiateAbility(AbilitySystemController owner)
            => new SimpleInstantiatedAbility(this, owner);

        public class SimpleInstantiatedAbility : AbstractInstantiatedAbility
        {
            public SimpleInstantiatedAbility(AbstractAbility a, AbilitySystemController sys)
                : base(a, sys) { }

            protected override IEnumerator ActivateAbility()
            {
                _owner.ApplyGameplayEffectToSelf(new InstantiatedGameEffect(_ability.Cooldown));
                _owner.ApplyGameplayEffectToSelf(new InstantiatedGameEffect(_ability.Cost));

                _owner.ApplyGameplayEffectToSelf(new InstantiatedGameEffect(_ability.ResultingEffect));
                yield return null;
            }
        }
    }
}
