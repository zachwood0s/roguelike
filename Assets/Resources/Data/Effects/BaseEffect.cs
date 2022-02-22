using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Your Attack deals more damage and inflicts Weak.
/// </summary>

public class BaseEffect 
{
    // Option 1: General events
    // When What
    // - When you attack
    // - When foes are slain
    // - When you take damage

    // Option 2: Status effect based
    // Attack inflicts weak
    // When weak entities dies, splash damage dealt
    // Structure:

    // Weak Damage <=> Weak Status effect
    // Electrocution Damage <=> Electrocuted

    // Actives: (what) inflicts (status effect | damage type) on (what) (time period) => Attack inflicts weak
    // Conditions: When (status effect | damage type) entities (action), (action) [to (range)] => When weak entities die, damage dealt to entites in 10m range
    // Passives:  (status effect | damage type) entities (adverb) => weak entities are smaller

    // Actions:
    // Deal (status effect | damage type) damagjje
    // Spawn (n) (entity_id)
    
    // Does What

    // Inflicts What


    // Boil time periods down to short, medium, long


    // Modify what?
    // - Health +-
    //  * Healing +
    //  * Damage -
    // - Speed +-
    //  * Quickens +
    //  * Slows -
    // - DamageOutput +-
    //  * Strong +
    //  * Weak -

    // Thought:

    // Generic Attribute:
    // - Current Value
    // - Base Value

    // Abilities:
    // - Holds lists of effects
    // - Acts as the timeline / gatekeeper for an ability

    // - Charges
    // - Timeout


    // Effects
    // - Acts on attributes
    
    // - Effects What?
    // - Effects How
}
