using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MissileProjectile : Projectile
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float turnSpeed = 240f;

    private Rigidbody2D rb;
    private Ship targetShip;
    private MissileProjectile targetMissile;
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 launchDirection, Ship newOwnerShip, Ship newTargetShip)
    {
        direction = launchDirection != Vector2.zero ? launchDirection.normalized : transform.up;
        InitializeOwner(newOwnerShip);
        targetShip = newTargetShip;
        targetMissile = null;
    }

    public void Initialize(Vector2 launchDirection, Ship newOwnerShip, MissileProjectile newTargetMissile)
    {
        direction = launchDirection != Vector2.zero ? launchDirection.normalized : transform.up;
        InitializeOwner(newOwnerShip);
        targetShip = null;
        targetMissile = newTargetMissile;
    }

    private void FixedUpdate()
    {
        UpdateDirection();
        rb.linearVelocity = direction * speed;
        rb.MoveRotation(Vector2.SignedAngle(Vector2.up, direction));
    }

    private void UpdateDirection()
    {
        Vector2? targetPosition = GetTargetPosition();
        if (!targetPosition.HasValue)
        {
            return;
        }

        Vector2 desiredDirection = (targetPosition.Value - (Vector2)transform.position).normalized;
        float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float desiredAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, turnSpeed * Time.fixedDeltaTime);
        float radians = newAngle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private Vector2? GetTargetPosition()
    {
        if (targetMissile != null)
        {
            return targetMissile.transform.position;
        }

        if (targetShip != null && !targetShip.IsDestroyed)
        {
            return targetShip.transform.position;
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        Ship hitShip = other.GetComponentInParent<Ship>();
        if (ShouldIgnoreShip(hitShip))
        {
            return;
        }

        ShipHealth shipHealth = other.GetComponentInParent<ShipHealth>();
        if (shipHealth != null)
        {
            TryDamageShipHealth(other);
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            TryDamageEnemyHealth(other);
            return;
        }

        TryDamageable(other);
    }

    public void DestroyMissile()
    {
        if (hasHit)
        {
            return;
        }

        hasHit = true;
        Destroy(gameObject);
    }
}
