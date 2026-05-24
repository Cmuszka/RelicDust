using UnityEngine;

public class MachineGunWeapon : WeaponSystem
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float spreadAngle = 5f;
    [SerializeField] private bool aimAtSelectedMissile = true;
    [SerializeField] private bool prioritizeNearestHostileMissile = true;

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
        if (aimAtSelectedMissile && ownerShip != null && ownerShip.targeting != null && ownerShip.targeting.HasMissileTarget())
        {
            return ownerShip.targeting.GetDirectionToCurrentTarget();
        }

        if (prioritizeNearestHostileMissile && ownerShip != null && ownerShip.targeting != null)
        {
            MissileProjectile missile = ownerShip.targeting.FindNearestHostileMissile();
            if (missile != null)
            {
                return (missile.transform.position - firePoint.position).normalized;
            }
        }

        return fallbackDirection != Vector2.zero ? fallbackDirection.normalized : firePoint.up;
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
