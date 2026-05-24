using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool canHitNeutralShips = true;

    private Vector2 direction;
    private Ship ownerShip;
    private ShipTeam ownerTeam = ShipTeam.Neutral;
    private bool hasHit;

    public void SetDirection(Vector2 newDirection)
    {
        Initialize(newDirection, null);
    }

    public void SetDirection(Vector2 newDirection, Ship newOwnerShip)
    {
        Initialize(newDirection, newOwnerShip);
    }

    public void Initialize(Vector2 newDirection, Ship newOwnerShip)
    {
        direction = newDirection.normalized;
        ownerShip = newOwnerShip;
        ownerTeam = ownerShip != null ? ownerShip.Team : ShipTeam.Neutral;

        CancelInvoke();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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

        if (TryDamageMissile(other))
        {
            return;
        }

        if (ownerShip == null && TryDamageEnemyHealth(other))
        {
            return;
        }

        if (TryDamageShipHealth(other))
        {
            return;
        }

        TryDamageEnemyHealth(other);
    }

    private bool TryDamageMissile(Collider2D other)
    {
        MissileHealth missileHealth = other.GetComponentInParent<MissileHealth>();
        if (missileHealth == null || ShouldIgnoreMissile(missileHealth))
        {
            return false;
        }

        missileHealth.TakeDamage(damage);
        hasHit = true;
        Destroy(gameObject);
        return true;
    }

    private bool TryDamageShipHealth(Collider2D other)
    {
        ShipHealth shipHealth = other.GetComponentInParent<ShipHealth>();
        if (shipHealth == null)
        {
            return false;
        }

        shipHealth.TakeDamage(damage);
        hasHit = true;
        Destroy(gameObject);
        return true;
    }

    private bool TryDamageEnemyHealth(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null)
        {
            return false;
        }

        enemy.TakeDamage(damage);
        hasHit = true;
        Destroy(gameObject);
        return true;
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

    private bool ShouldIgnoreMissile(MissileHealth missileHealth)
    {
        if (ownerShip == null)
        {
            return false;
        }

        if (missileHealth.Team == ShipTeam.Neutral)
        {
            return !canHitNeutralShips;
        }

        return missileHealth.Team == ownerTeam;
    }
}
