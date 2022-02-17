using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbstractAbility : ScriptableObject
    {
        [SerializeField] protected string _abilityName;
        [SerializeField] protected int _numberOfUses;
        [SerializeField] protected GameEffect _cost;
        [SerializeField] protected GameEffect _cooldown;
        [SerializeField] protected GameEffect _resultingEffect;

        public GameEffect Cooldown => _cooldown;
        public GameEffect Cost => _cost;
        public GameEffect ResultingEffect => _resultingEffect;

        public abstract AbstractInstantiatedAbility InstantiateAbility(AbilitySystemController owner);
    }
}

