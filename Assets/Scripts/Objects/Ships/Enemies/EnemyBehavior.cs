using UnityEngine;

[RequireComponent(typeof(Ship))]
public abstract class EnemyBehavior : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] protected bool acquirePlayerOnStart = true;
    [SerializeField] protected float targetRefreshInterval = 1f;

    [Header("Line Of Sight")]
    [SerializeField] protected bool requireLineOfSightToFire = true;
    [SerializeField] protected LayerMask lineOfSightBlockers;
    [SerializeField] protected float lineOfSightPadding = 0.25f;

    [Header("Cover")]
    [SerializeField] protected bool usesCoverWhileReloading;
    [SerializeField] protected LayerMask coverMask;
    [SerializeField] protected float coverSearchRadius = 12f;
    [SerializeField] protected float coverOffsetFromObstacle = 2.5f;

    protected Ship ship;
    protected ShipSteeringComputer steering;
    private float targetRefreshTimer;

    protected virtual void Awake()
    {
        ship = GetComponent<Ship>();
        steering = GetComponent<ShipSteeringComputer>();
    }

    protected virtual void Start()
    {
        TryAcquirePlayerTarget();
    }

    protected virtual void Update()
    {
        RefreshTargetIfNeeded();

        if (!HasValidTarget())
        {
            StopMoving();
            return;
        }

        TickBehavior();
    }

    protected abstract void TickBehavior();

    protected void RefreshTargetIfNeeded()
    {
        if (!acquirePlayerOnStart)
        {
            return;
        }

        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer > 0f && ship.targeting != null && ship.targeting.HasAssignedTarget())
        {
            return;
        }

        TryAcquirePlayerTarget();
        targetRefreshTimer = targetRefreshInterval;
    }

    protected void TryAcquirePlayerTarget()
    {
        if (ship.targeting == null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        Ship playerShip = playerObject != null ? playerObject.GetComponent<Ship>() : null;
        ship.targeting.SetTargetIgnoringRange(playerShip);
    }

    protected bool HasValidTarget()
    {
        if (ship.targeting == null || !ship.targeting.HasAssignedTarget())
        {
            return false;
        }

        Ship target = ship.targeting.currentTarget;
        return target != null && !target.IsDestroyed && (target.health == null || !target.health.IsDead);
    }

    protected Vector2 GetDirectionToTarget()
    {
        return (ship.targeting.currentTarget.transform.position - transform.position).normalized;
    }

    protected float GetDistanceToTarget()
    {
        return Vector2.Distance(transform.position, ship.targeting.currentTarget.transform.position);
    }

    protected Vector2 GetTargetVelocity()
    {
        Rigidbody2D targetBody = ship.targeting.currentTarget != null
            ? ship.targeting.currentTarget.GetComponent<Rigidbody2D>()
            : null;

        return targetBody != null ? targetBody.linearVelocity : Vector2.zero;
    }

    protected float GetRotationInputToward(Vector2 desiredDirection, float aimDeadZone)
    {
        float signedAngle = Vector2.SignedAngle(transform.up, desiredDirection);
        return Mathf.Abs(signedAngle) > aimDeadZone ? Mathf.Sign(signedAngle) : 0f;
    }

    protected bool IsFacing(Vector2 direction, float allowedAngle)
    {
        return Vector2.Angle(transform.up, direction) <= allowedAngle;
    }

    protected bool HasLineOfSightToTarget()
    {
        if (ship.targeting == null || ship.targeting.currentTarget == null)
        {
            return false;
        }

        Vector2 origin = transform.position;
        Vector2 targetPosition = ship.targeting.currentTarget.transform.position;
        Vector2 toTarget = targetPosition - origin;
        float distance = toTarget.magnitude;
        if (distance <= lineOfSightPadding)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            toTarget.normalized,
            Mathf.Max(distance - lineOfSightPadding, 0f),
            lineOfSightBlockers);

        return hit.collider == null;
    }

    protected bool CanFireAtTarget(Vector2 directionToTarget, float firingArc)
    {
        return IsFacing(directionToTarget, firingArc)
            && (!requireLineOfSightToFire || HasLineOfSightToTarget());
    }

    protected bool IsWeaponReloading(int weaponIndex)
    {
        WeaponSystem weapon = GetWeapon(weaponIndex);
        return weapon != null && weapon.CooldownRemaining > 0f;
    }

    protected bool TryMoveToCover()
    {
        if (!usesCoverWhileReloading || ship.targeting == null || ship.targeting.currentTarget == null)
        {
            return false;
        }

        Collider2D cover = FindBestCover();
        if (cover == null)
        {
            return false;
        }

        Vector2 targetPosition = ship.targeting.currentTarget.transform.position;
        Vector2 coverPosition = cover.bounds.center;
        Vector2 awayFromTarget = coverPosition - targetPosition;
        if (awayFromTarget.sqrMagnitude <= 0.001f)
        {
            awayFromTarget = (Vector2)transform.position - targetPosition;
        }

        if (awayFromTarget.sqrMagnitude <= 0.001f)
        {
            awayFromTarget = -transform.up;
        }

        awayFromTarget.Normalize();
        Vector2 coverDestination = coverPosition + awayFromTarget * coverOffsetFromObstacle;

        if (steering != null)
        {
            steering.MoveToward(coverDestination);
        }
        else
        {
            Vector2 moveDirection = coverDestination - (Vector2)transform.position;
            float signedAngle = Vector2.SignedAngle(transform.up, moveDirection);
            float thrustInput = Mathf.Abs(signedAngle) <= 45f ? 1f : 0f;
            SetMovement(thrustInput, GetRotationInputToward(moveDirection.normalized, 5f));
        }

        return true;
    }

    protected void SetMovement(float thrustInput, float rotationInput)
    {
        if (ship.movement != null)
        {
            ship.movement.SetMovementInput(thrustInput, rotationInput);
        }
    }

    protected void StopMoving()
    {
        if (steering != null)
        {
            steering.Stop();
            return;
        }

        if (ship.movement != null)
        {
            ship.movement.StopMovementInput();
        }
    }

    protected bool TryFireWeapon(int weaponIndex, Vector2 direction)
    {
        WeaponSystem weapon = GetWeapon(weaponIndex);
        if (weapon == null || !weapon.CanFire())
        {
            return false;
        }

        weapon.Fire(direction);
        return true;
    }

    protected WeaponSystem GetWeapon(int weaponIndex)
    {
        if (ship.weapons == null || weaponIndex < 0 || weaponIndex >= ship.weapons.Length)
        {
            return null;
        }

        return ship.weapons[weaponIndex];
    }

    private Collider2D FindBestCover()
    {
        Collider2D[] covers = Physics2D.OverlapCircleAll(transform.position, coverSearchRadius, coverMask);
        Collider2D bestCover = null;
        float bestScore = float.MinValue;
        Vector2 targetPosition = ship.targeting.currentTarget.transform.position;

        foreach (Collider2D cover in covers)
        {
            if (cover == null)
            {
                continue;
            }

            Vector2 coverPosition = cover.bounds.center;
            Vector2 enemyToCover = coverPosition - (Vector2)transform.position;
            Vector2 targetToCover = coverPosition - targetPosition;
            if (enemyToCover.sqrMagnitude <= 0.001f || targetToCover.sqrMagnitude <= 0.001f)
            {
                continue;
            }

            float alignmentBehindCover = Vector2.Dot(enemyToCover.normalized, targetToCover.normalized);
            float distancePenalty = enemyToCover.magnitude / Mathf.Max(coverSearchRadius, 0.01f);
            float score = alignmentBehindCover - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestCover = cover;
            }
        }

        return bestCover;
    }
}
