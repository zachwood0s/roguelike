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
            _LinkGameEffects(_resultingEffects);
            return new SimpleInstantiatedAbility(this, owner);
        }


        private void _LinkGameEffects(List<GameEffect> effects)
        {
            _startingEffects = new List<GameEffect>();
            foreach (var e in effects)
            {
                // Clear them out
                e.PostEffects = new List<GameEffect>();
            }
            foreach (var e in effects)
            {
                if (e.IsRelativeToIndex.HasValue)
                {
                    effects[e.IsRelativeToIndex.Value].PostEffects.Add(e);
                }
                else
                {
                    _startingEffects.Add(e);
                }
            }
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
                    _owner.ApplyGameEffectToApplicable(r);
                    /*
                    if(r.AreaOfEffect == null)
                    {
                        _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(r, _owner, _owner));
                    }
                    else
                    {
                        IEnumerator _Work()
                        {
                            yield return new WaitForSeconds(r.Delay);

                            var obj = Instantiate(r.AreaOfEffect, _owner.transform);
                            var colliders = obj.GetComponents<Collider2D>();

                            var targetSet = new HashSet<Collider2D>();
                            var colliderFilter = new ContactFilter2D();
                            colliderFilter.SetLayerMask(r.LayerMask);

                            foreach (var c in colliders)
                            {
                                var res = new List<Collider2D>();
                                Physics2D.OverlapCollider(c, colliderFilter, res);
                                targetSet.UnionWith(res);
                            }

                            foreach(var t in targetSet)
                            {
                                var target = t.GetComponent<AbilitySystemController>();
                                if(target != null)
                                {
                                    target.ApplyGameEffectToSelfNoDelay(new InstantiatedGameEffect(r, _owner, target));
                                }
                            }

                            Destroy(obj);
                        }

                        _owner.StartCoroutine(_Work());
                    }
                    */
                }
                yield return null;
            }


            protected override bool HasCorrectTags()
                => HasAllTags(_owner, _ability.RequiredTags)
                && HasNoneTags(_owner, _ability.BlockedTags);
        }
    }
}
