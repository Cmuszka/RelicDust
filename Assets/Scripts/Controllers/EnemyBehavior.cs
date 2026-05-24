using UnityEngine;

[RequireComponent(typeof(Ship))]
public abstract class EnemyBehavior : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] protected bool acquirePlayerOnStart = true;
    [SerializeField] protected float targetRefreshInterval = 1f;

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
        if (targetRefreshTimer > 0f && ship.targeting != null && ship.targeting.HasTarget())
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
        ship.targeting.SetTarget(playerShip);
    }

    protected bool HasValidTarget()
    {
        if (ship.targeting == null || !ship.targeting.HasTarget())
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

    protected float GetRotationInputToward(Vector2 desiredDirection, float aimDeadZone)
    {
        float signedAngle = Vector2.SignedAngle(transform.up, desiredDirection);
        return Mathf.Abs(signedAngle) > aimDeadZone ? Mathf.Sign(signedAngle) : 0f;
    }

    protected bool IsFacing(Vector2 direction, float allowedAngle)
    {
        return Vector2.Angle(transform.up, direction) <= allowedAngle;
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
        if (ship.weapons == null || weaponIndex < 0 || weaponIndex >= ship.weapons.Length)
        {
            return false;
        }

        WeaponSystem weapon = ship.weapons[weaponIndex];
        if (weapon == null || !weapon.CanFire())
        {
            return false;
        }

        weapon.Fire(direction);
        return true;
    }
}
