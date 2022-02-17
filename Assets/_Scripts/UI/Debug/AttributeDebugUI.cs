using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributeDebugUI : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private AttributeSystemController _attributes;
    [SerializeField] private AbilitySystemController _abilities;


    // Update is called once per frame
    void Update()
    {
        string s = "Attributes:\n";
        foreach (var a in _attributes.Attributes)
        {
            s += $"{a.Name}: {_attributes.GetAttributeValue(a).CurrentValue}\n";
        }

        s += "\nTags:\n";
        foreach (var a in _abilities.AppliedGameTags)
        {
            s += $"{a.name}\n";
        }

        _text.text = s;
    }
}
