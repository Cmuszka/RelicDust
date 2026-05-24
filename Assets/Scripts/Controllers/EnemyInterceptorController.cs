using UnityEngine;

public class EnemyInterceptorController : EnemyBehavior
{
    [Header("Interceptor")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float overshootRange = 1.5f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private float firingArc = 18f;
    [SerializeField] private int weaponIndex;

    protected override void TickBehavior()
    {
        Vector2 toTarget = GetDirectionToTarget();
        float distance = GetDistanceToTarget();
        Vector2 moveDirection = distance < overshootRange ? -toTarget : toTarget;

        if (steering != null)
        {
            steering.SetDesiredDirection(moveDirection, 1f);
        }
        else
        {
            float signedAngle = Vector2.SignedAngle(transform.up, moveDirection);
            float thrustInput = Mathf.Abs(signedAngle) <= 45f ? 1f : 0f;
            SetMovement(thrustInput, GetRotationInputToward(moveDirection, aimDeadZone));
        }

        if (distance <= attackRange && IsFacing(toTarget, firingArc))
        {
            TryFireWeapon(weaponIndex, toTarget);
        }
    }
}
