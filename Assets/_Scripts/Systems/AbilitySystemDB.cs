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
    [SerializeField] private BaseAttribute _knockbackVel;

    [SerializeField] private BaseAttribute _damageSimple;
    [SerializeField] private BaseAttribute _damageBurn;

    // Tags
    [SerializeField] private GameTag _dodgeRollCooldown;
    [SerializeField] private GameTag _dodgeRollDodging;
    [SerializeField] private GameTag _invunerable;
    [SerializeField] private GameTag _swordAttack1Attacking;
    [SerializeField] private GameTag _swordAttack2Attacking;
    [SerializeField] private GameTag _swordAttack3Attacking;
    [SerializeField] private GameTag _knockedBack;

    [SerializeField] private GameTag _burning;
    [SerializeField] private GameTag _stunned;

    // Attributes
    public BaseAttribute Health => _health;
    public BaseAttribute MoveSpeed => _moveSpeed;
    public BaseAttribute CurrentMoveSpeed => _currentMoveSpeed;
    public BaseAttribute KnockbackVel => _knockbackVel;
    public BaseAttribute DamageSimple => _damageSimple;
    public BaseAttribute DamageBurn => _damageBurn;

    // Tags
    public GameTag DodgeRollCooldown => _dodgeRollCooldown;
    public GameTag DodgeRollDodging => _dodgeRollDodging;
    public GameTag Invunerable => _invunerable;
    public GameTag SwordAttack1Attacking => _swordAttack1Attacking;
    public GameTag SwordAttack2Attacking => _swordAttack2Attacking;
    public GameTag SwordAttack3Attacking => _swordAttack3Attacking;
    public GameTag Knockedback => _knockedBack;

    public GameTag Burning => _burning;
    public GameTag Stunned => _stunned;
}
