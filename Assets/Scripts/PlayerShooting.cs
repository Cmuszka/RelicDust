using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 6f;

    private PlayerAim playerAim;
    private float fireCooldown;
    [SerializeField] private float burstFireRateMultiplier = 0.5f;

    private PlayerBurstAbility burst;

    private void Awake()
    {
        playerAim = GetComponent<PlayerAim>();
        burst = GetComponent<PlayerBurstAbility>();
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        float currentFireRate = fireRate;

        if (burst != null && burst.GetCooldownRemaining() > 2f)
        {
            currentFireRate *= burstFireRateMultiplier;
        }

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / currentFireRate;
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float spread = 5f;
        Vector2 direction = playerAim.AimDirection;

        direction = RotateVector(direction, Random.Range(-spread, spread));

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angleDegrees)
{
    float radians = angleDegrees * Mathf.Deg2Rad;
    float sin = Mathf.Sin(radians);
    float cos = Mathf.Cos(radians);

    float x = vector.x * cos - vector.y * sin;
    float y = vector.x * sin + vector.y * cos;

    return new Vector2(x, y).normalized;
}
}