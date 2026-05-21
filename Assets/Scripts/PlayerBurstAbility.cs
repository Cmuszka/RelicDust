using UnityEngine;

public class PlayerBurstAbility : MonoBehaviour
{
    [Header("Burst Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletCount = 12;
    [SerializeField] private float cooldownDuration = 3f;
    [SerializeField] private KeyCode activationKey = KeyCode.Q;

    private float cooldownTimer;

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(activationKey) && cooldownTimer <= 0f)
        {
            ActivateBurst();
            cooldownTimer = cooldownDuration;
        }
    }

    private void ActivateBurst()
    {
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep + Random.Range(-2f, 2f);
            Vector2 direction = AngleToDirection(angle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.transform.localScale *= 1.3f;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(direction);
            }
        }
    }

    private Vector2 AngleToDirection(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        float x = Mathf.Cos(radians);
        float y = Mathf.Sin(radians);

        return new Vector2(x, y).normalized;
    }

    public float GetCooldownRemaining()
    {
        return cooldownTimer;
    }

    public float GetCooldownDuration()
    {
        return cooldownDuration;
    }
}