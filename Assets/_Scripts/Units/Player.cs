using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private AbilitySystemController _abilitySystem;
    [SerializeField] private AttributeSystemController _attributeSystem;

    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Vector2 _dodgeDirection;
    private Vector2 _inputVec;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_abilitySystem.HasTagApplied(AbilitySystemDB.Instance.DodgeRollDodging))
        {
            // Currently dodging
            _moveDirection = _dodgeDirection;
        }
        else
        {
            _moveDirection = _inputVec;
        }
    }

    void FixedUpdate()
    {
        var speed = _attributeSystem.GetAttributeValue(AbilitySystemDB.Instance.MoveSpeed).CurrentValue;
        _rigidbody.velocity = _moveDirection * speed;
    }

    protected void OnMove(InputValue input)
    {
        _inputVec = input.Get<Vector2>().normalized;
    }

    protected void OnDodge(InputValue input)
    {
        _dodgeDirection = _moveDirection;
        _abilitySystem.UseAbility(0);
    }
}
