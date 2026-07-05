using UnityEngine;

public class EnemyCarrierController : EnemyBehavior
{
    [Header("Carrier")]
    [SerializeField] private float retreatRange = 10f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 6f;
    [SerializeField] private int supportWeaponIndex = -1;

    private float spawnTimer;

    protected override void Awake()
    {
        base.Awake();
        spawnTimer = spawnInterval;
    }

    protected override void TickBehavior()
    {
        Vector2 toTarget = GetDirectionToTarget();
        float distance = GetDistanceToTarget();
        Vector2 moveDirection = distance < retreatRange ? -toTarget : toTarget;

        if (supportWeaponIndex >= 0 && IsWeaponReloading(supportWeaponIndex) && TryMoveToCover())
        {
            HandleSpawning();
            return;
        }

        if (steering != null)
        {
            if (distance < retreatRange)
            {
                steering.SetDesiredDirection(moveDirection, 1f);
            }
            else
            {
                steering.FaceDirection(toTarget);
            }
        }
        else
        {
            float signedAngle = Vector2.SignedAngle(transform.up, moveDirection);
            float thrustInput = Mathf.Abs(signedAngle) <= 40f && distance < retreatRange ? 1f : 0f;
            SetMovement(thrustInput, GetRotationInputToward(moveDirection, aimDeadZone));
        }

        HandleSpawning();

        if (supportWeaponIndex >= 0 && CanFireAtTarget(toTarget, 30f))
        {
            TryFireWeapon(supportWeaponIndex, toTarget);
        }
    }

    private void HandleSpawning()
    {
        if (spawnPrefab == null || ship == null || ship.IsDestroyed)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
        {
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        Instantiate(spawnPrefab, spawnPosition, spawnRotation);
        spawnTimer = spawnInterval;
    }
}
