using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Ability System/Simple Ability")]
    public class SimpleAbility : AbstractAbility
    {
        [SerializeField] private List<GameEffect> _resultingEffects;

        // Keep a starting effects list so we don't have to modify the serialized 
        // resulting effects list during effect linking
        [NonSerialized] private List<GameEffect> _startingEffects;

        public override AbstractInstantiatedAbility InstantiateAbility(AbilitySystemController owner)
        {
            _startingEffects = GameEffect.LinkGameEffects(_resultingEffects);
            return new SimpleInstantiatedAbility(this, owner);
        }

        public class SimpleInstantiatedAbility : AbstractInstantiatedAbility
        {
            public SimpleInstantiatedAbility(AbstractAbility a, AbilitySystemController sys)
                : base(a, sys) { }

            protected override IEnumerator ActivateAbility()
            {
                _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(_ability.Cooldown, _owner, _owner));
                _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(_ability.Cost, _owner, _owner));

                var sa = _ability as SimpleAbility;

                foreach (var r in sa._startingEffects)
                {
                    _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(r, _owner, _owner));
                }
                yield return null;
            }


            protected override bool HasCorrectTags()
                => HasAllTags(_owner, _ability.RequiredTags)
                && HasNoneTags(_owner, _ability.BlockedTags);
        }
    }
}
