using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;

[CreateAssetMenu(fileName = "StandardEffectsDB", menuName = "Ability System/Standard Effects DB")]
public class StandardEffectsDB : ScriptableObject
{
    private static StandardEffectsDB _instance;
    public static StandardEffectsDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load("Data/"+typeof(StandardEffectsDB).Name) as StandardEffectsDB;
            }
            return _instance;
        }
    }

    public GameEffect BurnEffect;
    public float BurnPercBase;
}
