using UnityEngine;

public class EnemyBomberController : EnemyBehavior
{
    [Header("Bomber")]
    [SerializeField] private float preferredRange = 12f;
    [SerializeField] private float rangeTolerance = 2f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private float launchArc = 35f;
    [SerializeField] private int missileWeaponIndex;

    protected override void TickBehavior()
    {
        Vector2 toTarget = GetDirectionToTarget();
        float distance = GetDistanceToTarget();

        if (IsWeaponReloading(missileWeaponIndex) && TryMoveToCover())
        {
            return;
        }

        if (steering != null)
        {
            if (Mathf.Abs(distance - preferredRange) <= rangeTolerance)
            {
                steering.FacePosition(ship.targeting.currentTarget.transform.position);
            }
            else
            {
                steering.KeepRangeFrom(ship.targeting.currentTarget.transform.position, preferredRange, rangeTolerance);
            }
        }
        else
        {
            Vector2 moveDirection = distance < preferredRange - rangeTolerance ? -toTarget : toTarget;
            float rotationInput = GetRotationInputToward(moveDirection, aimDeadZone);
            float thrustInput = Mathf.Abs(Vector2.SignedAngle(transform.up, moveDirection)) <= 35f ? 1f : 0f;

            if (Mathf.Abs(distance - preferredRange) <= rangeTolerance)
            {
                thrustInput = 0f;
            }

            SetMovement(thrustInput, rotationInput);
        }

        if (distance <= ship.targeting.targetingRange && CanFireAtTarget(toTarget, launchArc))
        {
            TryFireWeapon(missileWeaponIndex, toTarget);
        }
    }
}
