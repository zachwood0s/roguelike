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
    private Vector2 _dodgeDirection;
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
        if (_abilitySystem.HasTagApplied(AbilitySystemDB.Instance.DodgeRollDodging))
        {
            // Currently dodging
            _moveDirection = _dodgeDirection;
        }
        else
        {
            _moveDirection = _inputVec;
        }
        _sprite.flipX = _moveDirection.x < 0;

    }

    void FixedUpdate()
    {
        var speed = _attributeSystem.GetAttributeValue(AbilitySystemDB.Instance.MoveSpeed).CurrentValue;
        var velDir = _moveDirection * speed;
        _attributeSystem.SetAttributeBaseValue(
            AbilitySystemDB.Instance.CurrentMoveSpeed, Mathf.Abs(velDir.magnitude));
        _rigidbody.velocity = velDir;

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
