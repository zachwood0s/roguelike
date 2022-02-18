using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private AbilitySystemController _abilitySystem;
    [SerializeField] private AttributeSystemController _attributeSystem;
    [SerializeField] private Animator _animator;

    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Vector2 _fixedDirecton;
    private Vector2 _inputVec;
    private SpriteRenderer _sprite;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_abilitySystem.HasTagApplied(AbilitySystemDB.Instance.DodgeRollDodging) ||
            _abilitySystem.HasTagApplied(AbilitySystemDB.Instance.SwordAttack1Attacking) ||
            _abilitySystem.HasTagApplied(AbilitySystemDB.Instance.SwordAttack2Attacking) ||
            _abilitySystem.HasTagApplied(AbilitySystemDB.Instance.SwordAttack3Attacking))
        {
            // Currently dodging
            _moveDirection = _fixedDirecton;
        }
        else
        {
            _moveDirection = _inputVec;
        }
        if(_moveDirection.x < 0)
        {
            _sprite.flipX = true;
        }
        else if(_moveDirection.x > 0)
        {
            _sprite.flipX = false;
        }

    }

    void FixedUpdate()
    {
        var speed = _attributeSystem.GetAttributeValue(AbilitySystemDB.Instance.MoveSpeed).CurrentValue;
        var velDir = _moveDirection * speed;
        _attributeSystem.SetAttributeBaseValue(
            AbilitySystemDB.Instance.CurrentMoveSpeed, Mathf.Abs(velDir.magnitude));
        _rigidbody.velocity = velDir;

    }

    protected void OnAttack1(InputValue input)
    {
        _fixedDirecton = _moveDirection;
        _abilitySystem.UseAbility(1);
        _abilitySystem.UseAbility(2);
        _abilitySystem.UseAbility(3);
    }

    protected void OnMove(InputValue input)
    {
        _inputVec = input.Get<Vector2>().normalized;
    }

    protected void OnDodge(InputValue input)
    {
        _fixedDirecton = _moveDirection;
        _abilitySystem.UseAbility(0);
    }
}
