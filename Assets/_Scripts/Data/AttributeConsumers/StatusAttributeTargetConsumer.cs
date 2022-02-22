using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;

public class StatusAttributeTargetConsumer : AbstractAttributeTargetConsumer
{
    [SerializeField] private float _burnBlinkTime;
    private float _timeUntilNextBurnBlink = 0;
    private SpriteRenderer _renderer;

    protected void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }
    public override void ConsumeEffects(AttributeSystemController attrs, AbilitySystemController cont)
    {
        if (cont.HasTagApplied(AbilitySystemDB.Instance.Burning))
        {
            if (_timeUntilNextBurnBlink <= 0)
            {
                _timeUntilNextBurnBlink = _burnBlinkTime;
                if (_renderer.color == Color.white)
                    _renderer.color = Color.red;
                else
                    _renderer.color = Color.white;
            }

            _timeUntilNextBurnBlink -= Time.deltaTime;
        }
        else
        {
            _renderer.color = Color.white;
        }
    }
}
