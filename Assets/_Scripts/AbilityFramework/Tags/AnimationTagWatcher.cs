using AbilitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTagWatcher : MonoBehaviour
{
    [Serializable]
    public struct AnimationTagMapping
    {
        public GameTag Tag;
        public string Trigger;
    }

    [Serializable]
    public struct AnimationAttributeMapping
    {
        public BaseAttribute Attribute;
        public string FloatValue;
    }

    [SerializeField] private Animator _animator;
    [SerializeField] private AbilitySystemController _abiltySystem;
    [SerializeField] private AttributeSystemController _attributeSystem;
    [SerializeField] private List<AnimationTagMapping> _tagTriggerMapping;
    [SerializeField] private List<AnimationTagMapping> _tagBoolMapping;
    [SerializeField] private List<AnimationAttributeMapping> _attributeMapping;

    // Update is called once per frame
    protected void LateUpdate()
    {
        var tags = new HashSet<GameTag>(_abiltySystem.AppliedGameTags);

        foreach (var map in _tagTriggerMapping)
        {
            if(tags.Contains(map.Tag))
                _animator.SetTrigger(map.Trigger);
        }

        foreach (var map in _tagBoolMapping)
        {
            _animator.SetBool(map.Trigger, tags.Contains(map.Tag));
        }

        foreach (var map in _attributeMapping)
        {
            _animator.SetFloat(map.FloatValue, _attributeSystem.GetAttributeValue(map.Attribute).CurrentValue);
        }
        
    }
}
