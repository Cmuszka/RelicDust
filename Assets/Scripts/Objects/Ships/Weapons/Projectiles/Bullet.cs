using UnityEngine;

public class Bullet : Projectile
{
    [SerializeField] private float speed = 12f;

    private Vector2 direction;

    public float Speed => speed;

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
        InitializeOwner(newOwnerShip);
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

        if (TryDamageable(other))
        {
            return;
        }

        TryDamageEnemyHealth(other);
    }
}
