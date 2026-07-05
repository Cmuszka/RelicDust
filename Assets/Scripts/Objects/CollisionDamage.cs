using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipHealth))]
public class CollisionDamage : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float safeImpactSpeed = 4f;
    [SerializeField] private float damageMultiplier = 4f;
    [SerializeField] private float damageExponent = 1.25f;
    [SerializeField] private float maxDamage = 50f;
    [SerializeField] private float damageCooldown = 0.4f;
    [SerializeField] private bool bypassShields;

    [Header("Filtering")]
    [SerializeField] private LayerMask damageLayers = ~0;
    [SerializeField] private bool useTagFilter;
    [SerializeField] private string[] damageTags;

    private readonly Dictionary<Collider2D, float> lastDamageTimes = new Dictionary<Collider2D, float>();
    private ShipHealth shipHealth;

    private void Awake()
    {
        shipHealth = GetComponent<ShipHealth>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplyCollisionDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryApplyCollisionDamage(collision);
    }

    private void TryApplyCollisionDamage(Collision2D collision)
    {
        if (shipHealth == null || shipHealth.IsDead || collision == null || !CanDamageFrom(collision.collider))
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < safeImpactSpeed)
        {
            return;
        }

        if (IsOnCooldown(collision.collider))
        {
            return;
        }

        float damage = CalculateDamage(impactSpeed);
        if (damage <= 0f)
        {
            return;
        }

        lastDamageTimes[collision.collider] = Time.time;
        shipHealth.TakeDamage(damage, bypassShields);
    }

    private float CalculateDamage(float impactSpeed)
    {
        float excessSpeed = impactSpeed - safeImpactSpeed;
        float damage = Mathf.Pow(excessSpeed, damageExponent) * damageMultiplier;
        return maxDamage > 0f ? Mathf.Min(damage, maxDamage) : damage;
    }

    private bool CanDamageFrom(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        if ((damageLayers.value & 1 << other.gameObject.layer) == 0)
        {
            return false;
        }

        if (!useTagFilter)
        {
            return true;
        }

        if (damageTags == null || damageTags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < damageTags.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(damageTags[i]) && other.CompareTag(damageTags[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOnCooldown(Collider2D other)
    {
        if (damageCooldown <= 0f)
        {
            return false;
        }

        if (!lastDamageTimes.TryGetValue(other, out float lastDamageTime))
        {
            return false;
        }

        return Time.time < lastDamageTime + damageCooldown;
    }
}
