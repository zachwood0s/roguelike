using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Ability System/Simple Ability")]
    public class SimpleAbility : AbstractAbility
    {
        [SerializeField] private List<GameEffect> _resultingEffects;

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

                foreach (var r in (_ability as SimpleAbility)._resultingEffects)
                {
                    _owner.ApplyGameplayEffectToSelf(new InstantiatedGameEffect(r));
                }
                yield return null;
            }
        }
    }
}
