using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;

    private Vector2 direction;
    private bool hasHit;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
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

        PlayerShieldAbility shield = other.GetComponentInParent<PlayerShieldAbility>();
        if (shield != null && shield.IsShieldActive)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        ShieldUtility shieldUtility = other.GetComponentInParent<ShieldUtility>();
        if (shieldUtility != null && shieldUtility.IsActive)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        ShipHealth shipHealth = other.GetComponentInParent<ShipHealth>();
        if (shipHealth != null)
        {
            shipHealth.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
