using UnityEngine;

[RequireComponent(typeof(Ship))]
public class EnemyShipController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private bool acquirePlayerOnStart = true;
    [SerializeField] private float targetRefreshInterval = 1f;

    [Header("Movement")]
    [SerializeField] private float preferredRange = 8f;
    [SerializeField] private float rangeTolerance = 1.5f;
    [SerializeField] private float aimDeadZone = 5f;
    [SerializeField] private float thrustAlignmentAngle = 35f;
    [SerializeField] private float holdRangeThrust = 0.35f;

    [Header("Weapons")]
    [SerializeField] private int weaponIndex;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float firingArc = 12f;
    [SerializeField] private float minimumFireRange = 2f;

    private Ship ship;
    private ShipSteeringComputer steering;
    private float fireTimer;
    private float targetRefreshTimer;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        steering = GetComponent<ShipSteeringComputer>();
        fireTimer = fireInterval;
    }

    private void Start()
    {
        TryAcquirePlayerTarget();
    }

    private void Update()
    {
        RefreshTargetIfNeeded();

        if (!HasValidTarget())
        {
            StopMoving();
            return;
        }

        HandleMovement();
        HandleWeapons();
    }

    private void RefreshTargetIfNeeded()
    {
        if (!acquirePlayerOnStart)
        {
            return;
        }

        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer > 0f && ship.targeting != null && ship.targeting.HasTarget())
        {
            return;
        }

        TryAcquirePlayerTarget();
        targetRefreshTimer = targetRefreshInterval;
    }

    private void TryAcquirePlayerTarget()
    {
        if (ship.targeting == null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Ship playerShip = playerObject != null ? playerObject.GetComponent<Ship>() : null;
        ship.targeting.SetTarget(playerShip);
    }

    private void HandleMovement()
    {
        if (ship.movement == null)
        {
            return;
        }

        Vector2 toTarget = GetDirectionToTarget();
        float distanceToTarget = GetDistanceToTarget();
        Vector2 desiredMoveDirection = GetDesiredMoveDirection(toTarget, distanceToTarget);
        float signedMoveAngle = Vector2.SignedAngle(transform.up, desiredMoveDirection);

        float rotationInput = Mathf.Abs(signedMoveAngle) > aimDeadZone ? Mathf.Sign(signedMoveAngle) : 0f;
        float thrustInput = GetThrustInput(distanceToTarget, signedMoveAngle);

        if (steering != null)
        {
            if (thrustInput > 0f)
            {
                steering.SetDesiredDirection(desiredMoveDirection, thrustInput);
            }
            else
            {
                steering.FaceDirection(desiredMoveDirection);
            }
        }
        else
        {
            ship.movement.SetMovementInput(thrustInput, rotationInput);
        }
    }

    private Vector2 GetDesiredMoveDirection(Vector2 toTarget, float distanceToTarget)
    {
        if (distanceToTarget < preferredRange - rangeTolerance)
        {
            return -toTarget;
        }

        return toTarget;
    }

    private float GetThrustInput(float distanceToTarget, float signedMoveAngle)
    {
        if (Mathf.Abs(signedMoveAngle) > thrustAlignmentAngle)
        {
            return 0f;
        }

        if (distanceToTarget > preferredRange + rangeTolerance)
        {
            return 1f;
        }

        if (distanceToTarget < preferredRange - rangeTolerance)
        {
            return 1f;
        }

        return holdRangeThrust;
    }

    private void HandleWeapons()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            if (CanFireAtTarget())
            {
                FireAtTarget();
                fireTimer = fireInterval;
            }
        }
    }

    private bool CanFireAtTarget()
    {
        float distanceToTarget = GetDistanceToTarget();
        if (distanceToTarget < minimumFireRange || distanceToTarget > ship.targeting.targetingRange)
        {
            return false;
        }

        Vector2 directionToTarget = GetDirectionToTarget();
        float angleToTarget = Vector2.Angle(transform.up, directionToTarget);

        return angleToTarget <= firingArc;
    }

    private bool HasValidTarget()
    {
        if (ship.targeting == null || !ship.targeting.HasTarget())
        {
            return false;
        }

        Ship target = ship.targeting.currentTarget;
        return target != null && (target.health == null || !target.health.IsDead);
    }

    private void FireAtTarget()
    {
        if (ship.weapons == null || weaponIndex < 0 || weaponIndex >= ship.weapons.Length)
        {
            return;
        }

        Vector2 direction = GetDirectionToTarget();
        ship.weapons[weaponIndex].Fire(direction);
    }

    private Vector2 GetDirectionToTarget()
    {
        return (ship.targeting.currentTarget.transform.position - transform.position).normalized;
    }

    private float GetDistanceToTarget()
    {
        return Vector2.Distance(transform.position, ship.targeting.currentTarget.transform.position);
    }

    private void StopMoving()
    {
        if (ship.movement != null)
        {
            ship.movement.StopMovementInput();
        }

        if (steering != null)
        {
            steering.Stop();
        }
    }
}
