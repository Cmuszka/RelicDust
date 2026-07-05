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

        Ship shipTarget = GetShipTarget();
        MissileProjectile missileTarget = GetMissileTarget();
        if (requireTargetLock && shipTarget == null && missileTarget == null)
        {
            return;
        }

        StartCooldown();

        Vector2 launchDirection = firePoint.up;
        MissileProjectile missile = Instantiate(
            missilePrefab,
            firePoint.position,
            Quaternion.LookRotation(Vector3.forward, launchDirection));

        if (missileTarget != null)
        {
            missile.Initialize(launchDirection, ownerShip, missileTarget);
        }
        else
        {
            missile.Initialize(launchDirection, ownerShip, shipTarget);
        }
    }

    public override void Fire()
    {
        Fire(firePoint != null ? firePoint.up : transform.up);
    }

    private Ship GetShipTarget()
    {
        if (ownerShip == null || ownerShip.targeting == null || !ownerShip.targeting.HasTarget())
        {
            return null;
        }

        return ownerShip.targeting.currentTarget;
    }

    private MissileProjectile GetMissileTarget()
    {
        if (ownerShip == null || ownerShip.targeting == null || !ownerShip.targeting.HasMissileTarget())
        {
            return null;
        }

        return ownerShip.targeting.currentMissileTarget;
    }
}
