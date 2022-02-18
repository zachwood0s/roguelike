using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbstractInstantiatedAbility
    {
        protected AbstractAbility _ability;
        protected AbilitySystemController _owner;

        public AbstractInstantiatedAbility(AbstractAbility ability, AbilitySystemController owner)
        {
            _ability = ability;
            _owner = owner;
        }

        public virtual IEnumerator TryActivateAbility()
        {
            if (!CanActivateAbility()) yield break;

            yield return ActivateAbility();
        }

        public virtual bool CanActivateAbility()
        {
            return HasCorrectTags() && !IsCooldownActive();
        }

        /// <summary>
        /// Checks if a cooldown is still active
        /// </summary>
        /// <returns>Remaining time</returns>
        public bool IsCooldownActive()
        {
            var cooldownTags = _ability.Cooldown.GrantedTags;
            var allTagSet = new HashSet<GameTag>(_owner.AppliedGameTags);

            foreach (var cTag in cooldownTags)
            {
                if(allTagSet.Contains(cTag))
                {
                    return true;
                }
            }
            return false;
        }

        // TODO: Make faster
        protected bool HasAllTags(AbilitySystemController cont, GameTag[] tags)
            => tags.All(x => cont.AppliedGameTags.Contains(x));
        protected bool HasNoneTags(AbilitySystemController cont, GameTag[] tags)
            => !tags.Any(x => cont.AppliedGameTags.Contains(x));

        protected abstract bool HasCorrectTags();

        protected abstract IEnumerator ActivateAbility();
    }
}
