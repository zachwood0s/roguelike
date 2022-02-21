using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilitySystem
{
    [RequireComponent(typeof(AttributeSystemController))]
    public class AbilitySystemController : MonoBehaviour
    {
        [SerializeField] private List<AbstractAbility> _abilities;
        [SerializeField] private List<AbstractAbility> _initializationAbilities;
        [SerializeField] private AttributeSystemController _attributeSystem;

        private List<AbstractInstantiatedAbility> _instantiatedAbilities = new List<AbstractInstantiatedAbility>();
        private List<(InstantiatedGameEffect inst, AttributeModifier[] modifiers)> _appliedGameEffects 
            = new List<(InstantiatedGameEffect inst, AttributeModifier[] modifiers)>();

        public IEnumerable<GameTag> AppliedGameTags =>
            _appliedGameEffects.SelectMany(x => x.inst.GameEffect.GrantedTags);

        public bool HasTagApplied(GameTag t) => AppliedGameTags.Contains(t);

        public bool HasTagApplied(GameTag t, out InstantiatedGameEffect firstInst)
        {
            foreach (var (inst, modifiers) in _appliedGameEffects)
            {
                if (inst.GameEffect.GrantedTags.Contains(t))
                {
                    firstInst = inst;
                    return true;
                }
            }
            firstInst = null;
            return false;
        }

        public void UseAbility(int i)
        {
            StartCoroutine(_instantiatedAbilities[i].TryActivateAbility());
        }

        public void ApplyGameEffectToSelf(InstantiatedGameEffect inst)
        {
            Debug.Assert(inst != null);

            if(inst.StartDelay != 0)
            {
                StartCoroutine(ApplyGameEffectToSelfAfterDelay(inst));
            }
            else
            {
                ApplyGameEffectToSelfNoDelay(inst);
            }
        }


        public IEnumerator ApplyGameEffectToSelfAfterDelay(InstantiatedGameEffect inst)
        {
            yield return new WaitForSeconds(inst.StartDelay);
            ApplyGameEffectToSelfNoDelay(inst);
            yield return null;
        }

        public void ApplyGameEffectToSelfNoDelay(InstantiatedGameEffect inst)
        {
            switch (inst.GameEffect.DurationStyle)
            {
                case DurationType.Instant:
                    _ApplyInstantGameEffect(inst);
                    break;
                case DurationType.Timed:
                    _ApplyTimedGameEffect(inst);
                    break;
            }
        }

        private void _ActivateInitializationAbilities()
        {
            foreach (var a in _initializationAbilities)
            {
                var spec = a.InstantiateAbility(this);
                StartCoroutine(spec.TryActivateAbility());
            }
        }

        private void _InstantiateAbilities()
        {
            foreach (var a in _abilities)
            {
                _instantiatedAbilities.Add(a.InstantiateAbility(this));
            }
        }

        private void _UpdateAttributeSystem()
        {
            foreach (var (e, mods) in _appliedGameEffects)
            {
                foreach (var m in mods)
                {
                    var appliedMod = m;
                    if (m.BaseMod?.EffectCurve.length != 0)
                    {
                        var time = (e.TotalTime - e.RemainingTime) / e.TotalTime;
                        appliedMod = m.ApplyAnimationCurve(
                            m.BaseMod.Value.EffectCurve, m.BaseMod.Value.ModifierOperation, time);
                    }
                    //var animatedM = m.ApplyAnimationCurve(e.)
                    _attributeSystem.UpdateAttributeModifiers(m.Attribute, appliedMod);
                }
            }
        }

        private void _TickGameEffects()
        {
            foreach (var (eff, _) in _appliedGameEffects)
            {
                if (eff.DoTick(Time.deltaTime))
                {
                    _ApplyInstantGameEffect(eff);
                }
            }
        }

        private void _ApplyInstantGameEffect(InstantiatedGameEffect e)
        {
            foreach (var mod in e.GameEffect.Modifiers)
            {
                var val = _attributeSystem.GetAttributeValue(mod.Attribute);
                var newBase = mod.ModifierOperation switch
                {
                    ModifierType.Add => val.BaseValue + mod.Value,
                    ModifierType.Multiply => val.BaseValue * mod.Value,
                    ModifierType.Override => mod.Value,
                    ModifierType.Percent => val.BaseValue * mod.Value,
                    _ => 0
                };
                _attributeSystem.SetAttributeBaseValue(mod.Attribute, newBase);
            }
        }

        /// <summary>
        /// Collects all of the modifiers for this game effect to be applied later
        /// </summary>
        /// <param name="e"></param>
        private void _ApplyTimedGameEffect(InstantiatedGameEffect e)
        {
            Debug.Assert(e.GameEffect.DurationStyle == DurationType.Timed);

            var modifiers = from mod in e.GameEffect.Modifiers
                            select new AttributeModifier(mod.Attribute, mod, mod.ModifierOperation, mod.Value);

            _appliedGameEffects.Add((e, modifiers.ToArray()));
        }

        private void _CleanGameEffects()
        {

            for (var i = _appliedGameEffects.Count - 1; i >= 0; i--)
            {
                var (inst, modifiers) = _appliedGameEffects[i];
                if (inst.GameEffect.DurationStyle == DurationType.Timed && inst.RemainingTime <= 0)
                {
                    // This effect is about to be removed, spawn its post effects if they exist
                    foreach (var newEffect in inst.GameEffect.PostEffects)
                    {
                        inst.Target.ApplyGameEffectToSelf(
                            new InstantiatedGameEffect(newEffect, inst.Source, inst.Target));
                    }

                    _appliedGameEffects.RemoveAt(i);
                }
            }
            /*
            _appliedGameEffects.RemoveAll(
                x => x.inst.GameEffect.DurationStyle == DurationType.Timed && x.inst.RemainingTime <= 0);
            */
        }

        protected void Awake()
        {
            _InstantiateAbilities();
        }

        protected void Start()
        {
            _ActivateInitializationAbilities();
        }

        protected void Update()
        {
            _attributeSystem.ResetAttributeModifiers();
            _UpdateAttributeSystem();

            _TickGameEffects();
            _CleanGameEffects();
        }
    }
}
