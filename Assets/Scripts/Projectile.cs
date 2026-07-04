using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected bool canHitNeutralShips;

    protected Ship ownerShip;
    protected ShipTeam ownerTeam = ShipTeam.Neutral;
    protected bool hasHit;

    public ShipTeam Team => ownerTeam;
    public Ship OwnerShip => ownerShip;

    public virtual void InitializeOwner(Ship newOwnerShip)
    {
        ownerShip = newOwnerShip;
        ownerTeam = ownerShip != null ? ownerShip.Team : ShipTeam.Neutral;

        CancelInvoke();
        Destroy(gameObject, lifetime);
    }

    protected bool ShouldIgnoreShip(Ship hitShip)
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

    protected bool ShouldIgnoreMissile(MissileHealth missileHealth)
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

    protected bool TryDamageMissile(Collider2D other)
    {
        MissileHealth missileHealth = other.GetComponentInParent<MissileHealth>();
        if (missileHealth == null || ShouldIgnoreMissile(missileHealth))
        {
            return false;
        }

        missileHealth.TakeDamage(damage);
        MarkHitAndDestroy();
        return true;
    }

    protected bool TryDamageShipHealth(Collider2D other)
    {
        ShipHealth shipHealth = other.GetComponentInParent<ShipHealth>();
        if (shipHealth == null)
        {
            return false;
        }

        shipHealth.TakeDamage(damage);
        MarkHitAndDestroy();
        return true;
    }

    protected bool TryDamageEnemyHealth(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null)
        {
            return false;
        }

        enemy.TakeDamage(Mathf.RoundToInt(damage));
        MarkHitAndDestroy();
        return true;
    }

    protected bool TryDamageable(Collider2D other)
    {
        IDamageable damageable = DamageableLookup.FindInParent(other);
        if (damageable == null)
        {
            return false;
        }

        RelayObjective relay = damageable as RelayObjective;
        if (relay != null && relay.ShouldIgnoreDamageFrom(ownerShip, ownerTeam))
        {
            return false;
        }

        damageable.TakeDamage(damage);
        MarkHitAndDestroy();
        return true;
    }

    protected void MarkHitAndDestroy()
    {
        hasHit = true;
        Destroy(gameObject);
    }

}
