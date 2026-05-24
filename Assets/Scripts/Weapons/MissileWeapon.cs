using UnityEngine;

public class MissileWeapon : WeaponSystem
{
    [SerializeField] private MissileProjectile missilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private bool requireTargetLock = true;

    public override void Fire(Vector2 direction)
    {
        if (!CanFire() || missilePrefab == null || firePoint == null)
        {
            return;
        }

        Ship target = GetTarget();
        if (requireTargetLock && target == null)
        {
            return;
        }

        StartCooldown();

        Vector2 launchDirection = firePoint.up;
        MissileProjectile missile = Instantiate(
            missilePrefab,
            firePoint.position,
            Quaternion.LookRotation(Vector3.forward, launchDirection));

        missile.Initialize(launchDirection, ownerShip, target);
    }

    public override void Fire()
    {
        Fire(firePoint != null ? firePoint.up : transform.up);
    }

    private Ship GetTarget()
    {
        if (ownerShip == null || ownerShip.targeting == null || !ownerShip.targeting.HasTarget())
        {
            return null;
        }

        return ownerShip.targeting.currentTarget;
    }
}
