using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MissileProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float turnSpeed = 240f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private bool canHitNeutralShips;

    private Rigidbody2D rb;
    private Ship ownerShip;
    private Ship targetShip;
    private MissileProjectile targetMissile;
    private ShipTeam ownerTeam = ShipTeam.Neutral;
    private Vector2 direction;
    private bool hasHit;

    public ShipTeam Team => ownerTeam;
    public Ship OwnerShip => ownerShip;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 launchDirection, Ship newOwnerShip, Ship newTargetShip)
    {
        direction = launchDirection != Vector2.zero ? launchDirection.normalized : transform.up;
        ownerShip = newOwnerShip;
        targetShip = newTargetShip;
        targetMissile = null;
        ownerTeam = ownerShip != null ? ownerShip.Team : ShipTeam.Neutral;

        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 launchDirection, Ship newOwnerShip, MissileProjectile newTargetMissile)
    {
        direction = launchDirection != Vector2.zero ? launchDirection.normalized : transform.up;
        ownerShip = newOwnerShip;
        targetShip = null;
        targetMissile = newTargetMissile;
        ownerTeam = ownerShip != null ? ownerShip.Team : ShipTeam.Neutral;

        Destroy(gameObject, lifetime);
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
            shipHealth.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
            hasHit = true;
            Destroy(gameObject);
        }
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

    private bool ShouldIgnoreShip(Ship hitShip)
    {
        if (hitShip == null)
        {
            return false;
        }

        if (hitShip == ownerShip)
        {
            return true;
        }

        if (ownerShip == null)
        {
            return false;
        }

        if (hitShip.Team == ShipTeam.Neutral)
        {
            return !canHitNeutralShips;
        }

        return hitShip.Team == ownerTeam;
    }
}
