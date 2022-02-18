using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;

public class AbilitySystemDB : Singleton<AbilitySystemDB>
{
    // Attributes
    [SerializeField] private BaseAttribute _health;
    [SerializeField] private BaseAttribute _moveSpeed;
    [SerializeField] private BaseAttribute _currentMoveSpeed;

    // Tags
    [SerializeField] private GameTag _dodgeRollCooldown;
    [SerializeField] private GameTag _dodgeRollDodging;
    [SerializeField] private GameTag _invunerable;
    [SerializeField] private GameTag _swordAttack1Attacking;
    [SerializeField] private GameTag _swordAttack2Attacking;
    [SerializeField] private GameTag _swordAttack3Attacking;

    // Attributes
    public BaseAttribute Health => _health;
    public BaseAttribute MoveSpeed => _moveSpeed;
    public BaseAttribute CurrentMoveSpeed => _currentMoveSpeed;

    // Tags
    public GameTag DodgeRollCooldown => _dodgeRollCooldown;
    public GameTag DodgeRollDodging => _dodgeRollDodging;
    public GameTag Invunerable => _invunerable;
    public GameTag SwordAttack1Attacking => _swordAttack1Attacking;
    public GameTag SwordAttack2Attacking => _swordAttack2Attacking;
    public GameTag SwordAttack3Attacking => _swordAttack3Attacking;

}
