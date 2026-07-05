using UnityEngine;

public class EnemyTurretController : EnemyBehavior
{
    [Header("Turret")]
    [SerializeField] private float aimDeadZone = 3f;
    [SerializeField] private float firingArc = 8f;
    [SerializeField] private int weaponIndex;

    protected override void TickBehavior()
    {
        Vector2 toTarget = GetDirectionToTarget();

        if (steering != null)
        {
            steering.FaceDirection(toTarget);
        }
        else
        {
            SetMovement(0f, GetRotationInputToward(toTarget, aimDeadZone));
        }

        if (CanFireAtTarget(toTarget, firingArc))
        {
            TryFireWeapon(weaponIndex, toTarget);
        }
    }
}
