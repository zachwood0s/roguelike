using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilitySystem
{
    public class AttributeSystemController : MonoBehaviour
    {
        
        [SerializeField] private List<BaseAttribute> _attributes;
        private Dictionary<BaseAttribute, AttributeValue> _attributeMapping 
            = new Dictionary<BaseAttribute, AttributeValue>();

        public IReadOnlyList<BaseAttribute> Attributes => _attributes;

        public AttributeValue GetAttributeValue(BaseAttribute attribute)
        {
            if (!_attributeMapping.TryGetValue(attribute, out var val))
            {
                val = new AttributeValue()
                {
                    Attribute = attribute,
                    Modifier = new AttributeModifier(attribute, null)
                };
                _attributeMapping[attribute] = val;
            }
            return val;
        }

        public void SetAttributeBaseValue(BaseAttribute attribute, float newVal)
        {
            //AttributeValue val;
            bool gotVal = _attributeMapping.TryGetValue(attribute, out var val);
            if (!gotVal)
            {
                Debug.Log("Here");
                return;
            }
            val.BaseValue = newVal;
            _attributeMapping[attribute] = val;
        }

        public void UpdateAttributeModifiers(BaseAttribute attr, AttributeModifier mod)
        {
            if(_attributeMapping.TryGetValue(attr, out var val))
            {
                val.Modifier = val.Modifier.Combine(mod);
                _attributeMapping[attr] = val;
            }
        }

        public void ResetAttributeModifiers()
        {
            var keys = _attributeMapping.Keys.ToList();
            foreach (var key in keys)
            {
                var val = _attributeMapping[key];
                val.Modifier = new AttributeModifier(key, val.Modifier.BaseMod);
                _attributeMapping[key] = val;
            }
        }

        private void _UpdateAttributeCurrentValues()
        {
            var keys = _attributeMapping.Keys.ToList();
            foreach (var key in keys)
            {
                var val = _attributeMapping[key];
                _attributeMapping[key] = val.Attribute.CalculateCurrentValue(val);
            }
        }

        private void _InitMapping()
        {
            foreach (var attr in _attributes)
            {
                _attributeMapping[attr] = new AttributeValue()
                {
                    Attribute = attr,
                    Modifier = new AttributeModifier(attr, null)
                };
            }
        }

        protected void Awake()
        {
            _InitMapping();
        }

        protected void LateUpdate()
        {
            _UpdateAttributeCurrentValues(); 
        }
    }
}
