using UnityEngine;

public class EnemyStrafeController : EnemyBehavior
{
    [Header("Hover Strafe")]
    [SerializeField] private float preferredRange = 7f;
    [SerializeField] private float rangeTolerance = 1.5f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private float firingArc = 15f;
    [SerializeField] private float attackWindowDuration = 1.25f;
    [SerializeField] private float repositionDuration = 1.75f;
    [SerializeField] private int weaponIndex;

    private float behaviorTimer;
    private bool isAttacking = true;

    protected override void TickBehavior()
    {
        behaviorTimer -= Time.deltaTime;
        if (behaviorTimer <= 0f)
        {
            isAttacking = !isAttacking;
            behaviorTimer = isAttacking ? attackWindowDuration : repositionDuration;
        }

        Vector2 toTarget = GetDirectionToTarget();
        float distance = GetDistanceToTarget();
        Vector2 moveDirection = GetMoveDirection(toTarget, distance);
        Vector2 facingDirection = isAttacking ? toTarget : moveDirection;

        if (steering != null)
        {
            if (isAttacking)
            {
                steering.FaceDirection(facingDirection);
            }
            else
            {
                steering.SetDesiredDirection(moveDirection, 0.75f);
            }
        }
        else
        {
            float rotationInput = GetRotationInputToward(facingDirection, aimDeadZone);
            float thrustInput = GetThrustInput(moveDirection);
            SetMovement(thrustInput, rotationInput);
        }

        if (isAttacking && IsFacing(toTarget, firingArc))
        {
            TryFireWeapon(weaponIndex, toTarget);
        }
    }

    private Vector2 GetMoveDirection(Vector2 toTarget, float distance)
    {
        if (distance > preferredRange + rangeTolerance)
        {
            return toTarget;
        }

        if (distance < preferredRange - rangeTolerance)
        {
            return -toTarget;
        }

        return transform.up;
    }

    private float GetThrustInput(Vector2 moveDirection)
    {
        if (isAttacking)
        {
            return 0f;
        }

        float signedAngle = Vector2.SignedAngle(transform.up, moveDirection);
        return Mathf.Abs(signedAngle) <= 35f ? 0.75f : 0f;
    }
}
