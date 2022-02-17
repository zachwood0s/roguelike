using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{

    [CreateAssetMenu(fileName = "Tag", menuName = "Ability System/Game Tag")]
    public class GameTag : ScriptableObject
    {
        [SerializeField] private string _debugName;
    }
}
