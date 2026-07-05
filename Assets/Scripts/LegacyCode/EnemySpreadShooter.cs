using UnityEngine;

public class EnemySpreadShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float spreadAngle = 20f;

    private Transform player;
    private float timer;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        timer = fireInterval;
    }

    private void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ShootSpread();
            timer = fireInterval;
        }
    }

    private void ShootSpread()
    {
        Vector2 baseDirection = (player.position - firePoint.position).normalized;

        FireBullet(baseDirection, -spreadAngle);
        FireBullet(baseDirection, 0f);
        FireBullet(baseDirection, spreadAngle);
    }

    private void FireBullet(Vector2 baseDirection, float angleOffset)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 finalDirection = RotateVector(baseDirection, angleOffset);

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(finalDirection);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angleDegrees)
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