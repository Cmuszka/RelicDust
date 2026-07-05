using UnityEngine;

public class MachineGunWeapon : WeaponSystem
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float spreadAngle = 5f;
    [SerializeField] private bool aimAtSelectedMissile = true;
    [SerializeField] private bool prioritizeNearestHostileMissile = true;
    [SerializeField] private bool autoFireAtNearbyMissiles = true;
    [SerializeField] private float pointDefenseRange = 12f;
    [SerializeField] private bool autoFireAtNearbyShips = true;
    [SerializeField] private float closeShipAutoFireRange = 8f;
    [SerializeField] private bool leadAutoFireTargets = true;

    private MissileProjectile pointDefenseTarget;
    private Ship closeShipTarget;

    protected override void Update()
    {
        base.Update();

        if (ownerShip == null || ownerShip.targeting == null || firePoint == null)
        {
            return;
        }

        if (!autoFireAtNearbyMissiles && !autoFireAtNearbyShips)
        {
            return;
        }

        pointDefenseTarget = GetSelectedMissileAutoFireTarget();
        closeShipTarget = pointDefenseTarget == null ? GetSelectedShipAutoFireTarget() : null;

        if (pointDefenseTarget == null && closeShipTarget == null || !CanFire())
        {
            return;
        }

        Vector2 targetPosition = GetAutoFireAimPoint();

        Fire((targetPosition - (Vector2)firePoint.position).normalized);
    }

    public override void Fire(Vector2 direction)
    {
        if (!CanFire() || projectilePrefab == null || firePoint == null)
        {
            return;
        }

        StartCooldown();

        Vector2 baseDirection = GetFireDirection(direction);
        Vector2 finalDirection = RotateVector(baseDirection, Random.Range(-spreadAngle, spreadAngle));
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(Vector3.forward, finalDirection));

        Bullet playerBullet = projectile.GetComponent<Bullet>();
        if (playerBullet != null)
        {
            playerBullet.SetDirection(finalDirection, ownerShip);
            return;
        }

        EnemyBullet enemyBullet = projectile.GetComponent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.SetDirection(finalDirection);
        }
    }

    private Vector2 GetFireDirection(Vector2 fallbackDirection)
    {
        if (pointDefenseTarget != null)
        {
            return (GetPredictedAimPoint(
                pointDefenseTarget.transform.position,
                GetTargetVelocity(pointDefenseTarget.gameObject)) - (Vector2)firePoint.position).normalized;
        }

        if (closeShipTarget != null)
        {
            return (GetPredictedAimPoint(
                closeShipTarget.transform.position,
                GetTargetVelocity(closeShipTarget.gameObject)) - (Vector2)firePoint.position).normalized;
        }

        if (aimAtSelectedMissile && ownerShip != null && ownerShip.targeting != null && ownerShip.targeting.HasMissileTarget())
        {
            return ownerShip.targeting.GetDirectionToCurrentTarget();
        }

        if (prioritizeNearestHostileMissile && ownerShip != null && ownerShip.targeting != null)
        {
            MissileProjectile missile = ownerShip.targeting.FindNearestHostileMissile(pointDefenseRange);
            if (missile != null)
            {
                return (missile.transform.position - firePoint.position).normalized;
            }
        }

        return fallbackDirection != Vector2.zero ? fallbackDirection.normalized : firePoint.up;
    }

    private Vector2 GetAutoFireAimPoint()
    {
        if (pointDefenseTarget != null)
        {
            return GetPredictedAimPoint(
                pointDefenseTarget.transform.position,
                GetTargetVelocity(pointDefenseTarget.gameObject));
        }

        if (closeShipTarget != null)
        {
            return GetPredictedAimPoint(
                closeShipTarget.transform.position,
                GetTargetVelocity(closeShipTarget.gameObject));
        }

        return firePoint.position;
    }

    private Vector2 GetPredictedAimPoint(Vector2 targetPosition, Vector2 targetVelocity)
    {
        if (!leadAutoFireTargets)
        {
            return targetPosition;
        }

        float projectileSpeed = GetProjectileSpeed();
        if (projectileSpeed <= 0f)
        {
            return targetPosition;
        }

        float distance = Vector2.Distance(firePoint.position, targetPosition);
        float travelTime = distance / projectileSpeed;
        return targetPosition + targetVelocity * travelTime;
    }

    private float GetProjectileSpeed()
    {
        if (projectilePrefab == null)
        {
            return 0f;
        }

        Bullet bullet = projectilePrefab.GetComponent<Bullet>();
        return bullet != null ? bullet.Speed : 0f;
    }

    private static Vector2 GetTargetVelocity(GameObject targetObject)
    {
        Rigidbody2D targetBody = targetObject != null ? targetObject.GetComponent<Rigidbody2D>() : null;
        return targetBody != null ? targetBody.linearVelocity : Vector2.zero;
    }

    private MissileProjectile GetSelectedMissileAutoFireTarget()
    {
        if (!autoFireAtNearbyMissiles || !ownerShip.targeting.HasMissileTarget())
        {
            return null;
        }

        MissileProjectile missile = ownerShip.targeting.currentMissileTarget;
        float distance = Vector2.Distance(firePoint.position, missile.transform.position);

        return distance <= pointDefenseRange ? missile : null;
    }

    private Ship GetSelectedShipAutoFireTarget()
    {
        if (!autoFireAtNearbyShips || !ownerShip.targeting.HasTarget())
        {
            return null;
        }

        Ship ship = ownerShip.targeting.currentTarget;
        float distance = Vector2.Distance(firePoint.position, ship.transform.position);

        return distance <= closeShipAutoFireRange ? ship : null;
    }

    private static Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }
}
