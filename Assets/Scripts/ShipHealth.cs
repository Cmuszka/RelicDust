using UnityEngine;
using UnityEngine.Events;

public class ShipHealth : MonoBehaviour
{
    [Header("Max Health")]
    [SerializeField] private float hullStrength = 100f;
    [SerializeField] private float armorHealth = 50f;
    [SerializeField] private float shieldHealth = 25f;

    [Header("Current Health")]
    [SerializeField] private float currentHullStrength;
    [SerializeField] private float currentArmorHealth;
    [SerializeField] private float currentShieldHealth;

    public UnityEvent OnDeath;

    public float HullStrength => currentHullStrength;
    public float ArmorHealth => currentArmorHealth;
    public float ShieldHealth => currentShieldHealth;
    public float MaxHullStrength => hullStrength;
    public float MaxArmorHealth => armorHealth;
    public float MaxShieldHealth => shieldHealth;
    public bool IsDead => currentHullStrength <= 0f;

    private Ship ship;
    private float temporaryShieldHealth;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        ResetHealth();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || IsDead)
        {
            return;
        }

        damage = ApplyDamageToShield(damage);
        damage = ApplyDamageToLayer(damage, ref currentArmorHealth);
        ApplyDamageToLayer(damage, ref currentHullStrength);

        if (IsDead)
        {
            Die();
        }
    }

    public void RepairHull(float amount)
    {
        currentHullStrength = RestoreHealth(currentHullStrength, hullStrength, amount);
    }

    public void RepairArmor(float amount)
    {
        currentArmorHealth = RestoreHealth(currentArmorHealth, armorHealth, amount);
    }

    public void RechargeShield(float amount)
    {
        currentShieldHealth = RestoreHealth(currentShieldHealth, shieldHealth, amount);
    }

    public float AddTemporaryShield(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return 0f;
        }

        temporaryShieldHealth += amount;
        currentShieldHealth += amount;

        return amount;
    }

    public float RemoveTemporaryShield(float amount)
    {
        if (amount <= 0f)
        {
            return 0f;
        }

        float removedShield = Mathf.Min(currentShieldHealth, temporaryShieldHealth, amount);
        currentShieldHealth -= removedShield;
        temporaryShieldHealth -= removedShield;

        return removedShield;
    }

    public void ResetHealth()
    {
        currentHullStrength = hullStrength;
        currentArmorHealth = armorHealth;
        currentShieldHealth = shieldHealth;
        temporaryShieldHealth = 0f;
    }

    private float ApplyDamageToShield(float damage)
    {
        if (damage <= 0f || currentShieldHealth <= 0f)
        {
            return damage;
        }

        float absorbedDamage = Mathf.Min(currentShieldHealth, damage);
        currentShieldHealth -= absorbedDamage;
        temporaryShieldHealth = Mathf.Max(temporaryShieldHealth - absorbedDamage, 0f);

        return damage - absorbedDamage;
    }

    private static float ApplyDamageToLayer(float damage, ref float healthLayer)
    {
        if (damage <= 0f || healthLayer <= 0f)
        {
            return damage;
        }

        float absorbedDamage = Mathf.Min(healthLayer, damage);
        healthLayer -= absorbedDamage;

        return damage - absorbedDamage;
    }

    private static float RestoreHealth(float currentHealth, float maxHealth, float amount)
    {
        if (amount <= 0f)
        {
            return currentHealth;
        }

        return Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        if (ship != null)
        {
            ship.MarkDestroyed();
        }

        OnDeath?.Invoke();
    }
}
