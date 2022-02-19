using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackHandler : MonoBehaviour
{
    [SerializeField] private AbilitySystemController _abilitySystem;
    [SerializeField] private AttributeSystemController _attributeSystem;
    [SerializeField] private Rigidbody2D _rigidbody2D;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_abilitySystem.HasTagApplied(AbilitySystemDB.Instance.Knockedback, out var effect))
        {
            var targetVel = effect.Source.GetComponent<Rigidbody2D>().velocity;
            var knockbackDir = targetVel.normalized;
            if (knockbackDir == Vector2.zero)
            {
                knockbackDir = (effect.Target.transform.position - effect.Source.transform.position).normalized;
            }
            knockbackDir = SnapTo(knockbackDir, 45);//new Vector2(MyRound(knockbackDir.x), MyRound(knockbackDir.y));
            var knockbackVel = _attributeSystem.GetAttributeValue(AbilitySystemDB.Instance.KnockbackVel).CurrentValue;

            _rigidbody2D.velocity = knockbackDir * knockbackVel;
        }
        else
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
        
    }
    Vector3 SnapTo(Vector3 v3, float snapAngle)
    {
        float angle = Vector3.Angle(v3, Vector3.up);
        if (angle < snapAngle / 2.0f)          // Cannot do cross product 
            return Vector3.up * v3.magnitude;  //   with angles 0 & 180
        if (angle > 180.0f - snapAngle / 2.0f)
            return Vector3.down * v3.magnitude;

        float t = Mathf.Round(angle / snapAngle);
        float deltaAngle = (t * snapAngle) - angle;

        Vector3 axis = Vector3.Cross(Vector3.up, v3);
        Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
        return q * v3;
    }
    /*
    float MyRound(float f)
    {
        if (Mathf.Abs(f) < Mathf.Cos(Mathf.PI / 8))
            return 0;
        else
            return Mathf.Sign(f);
    }
    */

}
