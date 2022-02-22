using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    [CreateAssetMenu(fileName = "HitboxAbility", menuName = "Ability System/Hitbox Ability")]
    public class AbilityWithHitbox : AbstractAbility
    {
        [SerializeField] private List<GameEffect> _alwaysSelfEffects;

        [NonSerialized] private List<GameEffect> _startingEffects;

        [SerializeField] private List<HitboxEffect> _hitboxEffects;

        public override AbstractInstantiatedAbility InstantiateAbility(AbilitySystemController owner)
        {
            _startingEffects = GameEffect.LinkGameEffects(_alwaysSelfEffects);
            return new HitboxInstantiatedAbility(this, owner);
        }

        public class HitboxInstantiatedAbility : AbstractInstantiatedAbility
        {
            public HitboxInstantiatedAbility(AbstractAbility a, AbilitySystemController sys)
                : base(a, sys) { }

            protected override IEnumerator ActivateAbility()
            {
                _DoSelfEffects();
                _StartHitboxes();
                yield return null;
            }

            protected override bool HasCorrectTags()
                => HasAllTags(_owner, _ability.RequiredTags)
                && HasNoneTags(_owner, _ability.BlockedTags);

            private void _DoSelfEffects()
            {
                _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(_ability.Cooldown, _owner, _owner));
                _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(_ability.Cost, _owner, _owner));

                var sa = _ability as AbilityWithHitbox;
                foreach (var r in sa._startingEffects)
                {
                    _owner.ApplyGameEffectToSelf(new InstantiatedGameEffect(r, _owner, _owner));
                }
            }

            private void _StartHitboxes()
            {
                var sa = _ability as AbilityWithHitbox;
                foreach (var h in sa._hitboxEffects)
                {
                    _owner.StartCoroutine(_DoHitbox(h));
                }
            }

            private IEnumerator _DoHitbox(HitboxEffect e)
            {
                // Wait for delay
                yield return new WaitForSeconds(e.Delay);

                var desc = e.HitboxDescription;
                var obj = Instantiate(desc.Prefab, _owner.transform);
                var colliders = obj.GetComponents<Collider2D>();

                var seenSet = new HashSet<Collider2D>();
                var colliderFilter = new ContactFilter2D();
                colliderFilter.SetLayerMask(desc.LayerMask);
                colliderFilter.useTriggers = true;

                float timeElapsed = 0;

                while (timeElapsed < desc.AliveDuration)
                {
                    timeElapsed += Time.deltaTime;

                    var targetSet = new HashSet<Collider2D>();
                    foreach (var c in colliders)
                    {
                        var res = new List<Collider2D>();
                        Physics2D.OverlapCollider(c, colliderFilter, res);

                        // Add the results to the targetSet
                        foreach (var t in res)
                        {
                            if (!seenSet.Contains(t))
                            {
                                targetSet.Add(t);
                            }
                        }
                    }

                    if (targetSet.Count > 0)
                    {
                        // Hit happened, apply all self on hit effects
                        foreach (var selfEffect in e.SelfOnHitEffects)
                        {
                            _owner.ApplyGameEffectToSelfNoDelay(
                                new InstantiatedGameEffect(selfEffect, _owner, _owner));
                        }
                    }
                    
                    foreach (var t in targetSet)
                    {
                        // Apply all on hit effects to the target
                        var target = t.GetComponentInParent<AbilitySystemController>();
                        if (target != null)
                        {
                            foreach (var targetEffect in e.TargetOnHitEffects)
                            {
                                target.ApplyGameEffectToSelfNoDelay(
                                    new InstantiatedGameEffect(targetEffect, _owner, target));
                            }
                        }

                    }

                    seenSet.UnionWith(targetSet);


                    // Wait for next frame
                    yield return null;
                }

                Destroy(obj);

                yield return null;
            }
        }
    }

    [Serializable]
    public class HitboxEffect
    {
        public float Delay;
        public HitboxParams HitboxDescription;
        public List<GameEffect> SelfOnHitEffects;
        public List<GameEffect> TargetOnHitEffects;

        [NonSerialized] public List<GameEffect> SelfStartingOnHitEffects;
        [NonSerialized] public List<GameEffect> TargetStartingOnHitEffects;
    }

    [Serializable]
    public class HitboxParams
    {
        public GameObject Prefab;
        public float AliveDuration;
        public LayerMask LayerMask;
    }
}
