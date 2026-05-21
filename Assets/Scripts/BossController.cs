using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float targetY = 3.5f;

    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject enemyBulletPrefab;

    [Header("Phase 1")]
    [SerializeField] private float phase1FireInterval = 1.2f;

    [Header("Phase 2")]
    [SerializeField] private float phase2FireInterval = 0.7f;
    [SerializeField] private float spreadAngle = 20f;

    private Transform player;
    private float fireTimer;
    private bool enteredPosition;
    private bool phaseTwo;

    [Header("Phase 2 Movement")]
    [SerializeField] private float horizontalAmplitude = 2f;
    [SerializeField] private float horizontalFrequency = 2f;

    private float startX;

    private void Start()
    {

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        startX = transform.position.x;
        fireTimer = phase1FireInterval;
    }

    private void Update()
    {
        if (!enteredPosition)
        {
            MoveIntoPosition();
            return;
        }

        CheckPhase();
        HandleAttacks();
        if (phaseTwo)
        {
            float newX = startX + Mathf.Sin(Time.time * horizontalFrequency) * horizontalAmplitude;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }

    private void MoveIntoPosition()
    {
        Vector3 position = transform.position;
        position = Vector3.MoveTowards(position, new Vector3(position.x, targetY, position.z), moveSpeed * Time.deltaTime);
        transform.position = position;

        if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
        {
            enteredPosition = true;
        }
    }

    private void CheckPhase()
    {
        if (!phaseTwo && health.CurrentHealth <= health.MaxHealth / 2)
        {
            Debug.Log("PHASE 2");
            phaseTwo = true;
        }
    }

    private void HandleAttacks()
    {
        if (player == null) return;

        fireTimer -= Time.deltaTime;

        if (!phaseTwo)
        {
            if (fireTimer <= 0f)
            {
                FireAimedShot();
                fireTimer = phase1FireInterval;
            }
        }
        else
        {
            if (fireTimer <= 0f)
            {
                FireSpreadShot();
                fireTimer = phase2FireInterval;
            }
        }
    }

    private void FireAimedShot()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;
        SpawnBullet(direction);
    }

    private void FireSpreadShot()
    {
        Vector2 baseDirection = (player.position - firePoint.position).normalized;

        SpawnBullet(RotateVector(baseDirection, -spreadAngle));
        SpawnBullet(baseDirection);
        SpawnBullet(RotateVector(baseDirection, spreadAngle));
    }

    private void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
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

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }
}