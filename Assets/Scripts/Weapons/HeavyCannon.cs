using UnityEngine;

public class HeavyCannon : WeaponSystem
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float recoilForce = 5f;

    [Header("Target Assist")]
    [SerializeField] private bool autoFireAtAlignedTarget;
    [SerializeField] private float autoFireAlignmentAngle = 4f;

    protected override void Update()
    {
        base.Update();

        if (!autoFireAtAlignedTarget || ownerShip == null || ownerShip.targeting == null)
        {
            return;
        }

        Vector2 fireDirection = firePoint != null ? firePoint.up : transform.up;

        if (!CanFire() || !ownerShip.targeting.IsDirectionAlignedWithTarget(fireDirection, autoFireAlignmentAngle))
        {
            return;
        }

        Fire();
    }

    public override void Fire(Vector2 direction)
    {
        if (!CanFire() || projectilePrefab == null || firePoint == null) return;

        StartCooldown();

        Vector2 fireDirection = firePoint.up;
        GameObject projectile =
            Instantiate(projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(Vector3.forward, fireDirection));

        Bullet playerBullet = projectile.GetComponent<Bullet>();
        if (playerBullet != null)
        {
            playerBullet.SetDirection(fireDirection, ownerShip);
        }

        EnemyBullet enemyBullet = projectile.GetComponent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.SetDirection(fireDirection);
        }

        Rigidbody2D rb = ownerShip != null ? ownerShip.GetComponent<Rigidbody2D>() : null;
        if (rb != null)
        {
            rb.AddForce(-fireDirection * recoilForce, ForceMode2D.Impulse);
        }
    }

    public override void Fire()
    {
        Fire(firePoint != null ? firePoint.up : transform.up);
    }
}
