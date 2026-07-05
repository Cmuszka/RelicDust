using UnityEngine;

public class EnemyInterceptorController : EnemyBehavior
{
    private enum AttackRunState
    {
        Approach,
        Pass,
        Disengage,
        Reposition
    }

    [Header("Interceptor")]
    [SerializeField] private float engageRange = 16f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float passRange = 2f;
    [SerializeField] private float disengageDistance = 10f;
    [SerializeField] private float repositionDistance = 13f;
    [SerializeField] private float repositionSideOffset = 5f;
    [SerializeField] private float approachSpeedMatchDistance = 8f;
    [SerializeField] private float approachSpeedSlowdownStrength = 0.55f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private float firingArc = 18f;
    [SerializeField] private int weaponIndex;

    private AttackRunState attackRunState = AttackRunState.Approach;
    private Vector2 repositionPoint;
    private int repositionSide = 1;

    protected override void TickBehavior()
    {
        Vector2 toTarget = GetDirectionToTarget();
        float distance = GetDistanceToTarget();
        UpdateAttackRunState(distance);

        Vector2 moveDirection = GetMoveDirection(toTarget, distance);
        float thrust = GetThrust(distance);

        if (attackRunState == AttackRunState.Reposition && IsWeaponReloading(weaponIndex) && TryMoveToCover())
        {
            return;
        }

        Move(moveDirection, thrust);

        if (distance <= attackRange && CanFireAtTarget(toTarget, firingArc))
        {
            TryFireWeapon(weaponIndex, toTarget);
        }
    }

    private void UpdateAttackRunState(float distance)
    {
        switch (attackRunState)
        {
            case AttackRunState.Approach:
                if (distance <= passRange)
                {
                    attackRunState = AttackRunState.Pass;
                }
                break;
            case AttackRunState.Pass:
                if (distance >= disengageDistance)
                {
                    attackRunState = AttackRunState.Disengage;
                }
                break;
            case AttackRunState.Disengage:
                if (distance >= repositionDistance)
                {
                    PickRepositionPoint();
                    attackRunState = AttackRunState.Reposition;
                }
                break;
            case AttackRunState.Reposition:
                if (Vector2.Distance(transform.position, repositionPoint) <= 1.5f || distance > engageRange)
                {
                    attackRunState = AttackRunState.Approach;
                }
                break;
        }
    }

    private Vector2 GetMoveDirection(Vector2 toTarget, float distance)
    {
        switch (attackRunState)
        {
            case AttackRunState.Pass:
                return transform.up;
            case AttackRunState.Disengage:
                return -toTarget;
            case AttackRunState.Reposition:
                return repositionPoint - (Vector2)transform.position;
            default:
                return toTarget;
        }
    }

    private float GetThrust(float distance)
    {
        if (attackRunState != AttackRunState.Approach || distance > approachSpeedMatchDistance)
        {
            return 1f;
        }

        Rigidbody2D ownBody = GetComponent<Rigidbody2D>();
        Vector2 targetVelocity = GetTargetVelocity();
        float ownSpeed = ownBody != null ? ownBody.linearVelocity.magnitude : 0f;
        float targetSpeed = targetVelocity.magnitude;

        if (ownSpeed <= targetSpeed)
        {
            return 1f;
        }

        float speedDifference = ownSpeed - targetSpeed;
        float slowdown = Mathf.Clamp01(speedDifference / Mathf.Max(targetSpeed, 1f)) * approachSpeedSlowdownStrength;
        return Mathf.Clamp01(1f - slowdown);
    }

    private void PickRepositionPoint()
    {
        if (ship.targeting == null || ship.targeting.currentTarget == null)
        {
            repositionPoint = transform.position;
            return;
        }

        repositionSide *= -1;
        Vector2 targetPosition = ship.targeting.currentTarget.transform.position;
        Vector2 toSelf = ((Vector2)transform.position - targetPosition).normalized;
        if (toSelf == Vector2.zero)
        {
            toSelf = -transform.up;
        }

        Vector2 side = Vector2.Perpendicular(toSelf) * repositionSide;
        repositionPoint = targetPosition + toSelf * repositionDistance + side * repositionSideOffset;
    }

    private void Move(Vector2 moveDirection, float thrust)
    {
        if (moveDirection == Vector2.zero)
        {
            StopMoving();
            return;
        }

        moveDirection.Normalize();

        if (steering != null)
        {
            steering.SetDesiredDirection(moveDirection, thrust);
        }
        else
        {
            float signedAngle = Vector2.SignedAngle(transform.up, moveDirection);
            float thrustInput = Mathf.Abs(signedAngle) <= 45f ? thrust : 0f;
            SetMovement(thrustInput, GetRotationInputToward(moveDirection, aimDeadZone));
        }
    }
}
