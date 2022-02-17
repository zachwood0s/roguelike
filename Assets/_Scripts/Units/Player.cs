using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum State
    {
        Dodging,
        Running,
    };

    [SerializeField] private float _baseMoveSpeed;

    [SerializeField] private float _dodgeTime;
    [SerializeField] private float _dodgeSpeed;
    [SerializeField] private float _dodgeTimeout;
    [SerializeField] private float _afterDodgeSlowdownFactor;
    [SerializeField] private float _afterDodgeSlowdownTimeout;

    [SerializeField] private int _maxDodgeCount;
    [SerializeField] private AbilitySystemController _abilitySystem;
    [SerializeField] private AttributeSystemController _attributeSystem;

    private State _state; 
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Vector2 _dodgeDirection;
    private Vector2 _inputVec;
    private int _currentDodgeAmount;
    private bool _dodgeSlowdown;
    private float _moveSpeed;

    private Coroutine _dodgeRoutine;
    private Coroutine _dodgeTimeoutRoutine;
    private Coroutine _dodgeSlowdownRoutine;
    // Start is called before the first frame update
    void Start()
    {
        _state = State.Running;
        _rigidbody = GetComponent<Rigidbody2D>();
        _dodgeSlowdown = false;
        _moveSpeed = _baseMoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = _state switch
        {
            State.Running => _inputVec,
            State.Dodging => _dodgeDirection,
            _ => Vector2.zero
        };
    }

    void FixedUpdate()
    {
        var speed = _state switch
        {
            State.Running when _dodgeSlowdown => _baseMoveSpeed * _afterDodgeSlowdownFactor,
            State.Running => _moveSpeed,
            State.Dodging => _dodgeSpeed,
            _ => 0
        };
        _rigidbody.velocity = _moveDirection * speed;
    }

    protected void OnMove(InputValue input)
    {
        _inputVec = input.Get<Vector2>().normalized;
    }

    protected void OnDodge(InputValue input)
    {
        _abilitySystem.UseAbility(0);
        if (_state != State.Dodging)
        {
            _dodgeRoutine = StartCoroutine(DoDodge());
        }
    }

    protected IEnumerator DoDodge()
    {
        if (_currentDodgeAmount < _maxDodgeCount && _moveDirection != Vector2.zero)
        {
            Debug.Log("Dodging");

            // Remove the slowdown effect when new dodge starts
            if (_dodgeTimeoutRoutine != null) 
                StopCoroutine(_dodgeSlowdownRoutine);
            _dodgeDirection = _moveDirection;

            var lastState = _state;
            _state = State.Dodging;
            // Activate the slow period
            yield return new WaitForSeconds(_dodgeTime);
            _state = lastState;

            _currentDodgeAmount++;
            _dodgeSlowdown = true;
            _dodgeSlowdownRoutine = StartCoroutine(DoDodgeSlowdown());
            _dodgeTimeoutRoutine = StartCoroutine(DoDodgeTimeout());
        }
    }
    protected IEnumerator DoDodgeSlowdown()
    {
        yield return new WaitForSeconds(_afterDodgeSlowdownTimeout);
        _dodgeSlowdown = false;
    }

    protected IEnumerator DoDodgeTimeout()
    {
        // Wait to refill the dodge charge
        yield return new WaitForSeconds(_dodgeTimeout);

        _currentDodgeAmount--;
        Debug.Log("Dodge Ready");
    }
}
