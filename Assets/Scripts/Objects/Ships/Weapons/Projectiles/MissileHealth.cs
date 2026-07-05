using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MissileProjectile))]
public class MissileHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 2f;

    public UnityEvent OnDestroyed;

    private MissileProjectile missile;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public ShipTeam Team => missile != null ? missile.Team : ShipTeam.Neutral;

    private void Awake()
    {
        missile = GetComponent<MissileProjectile>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            DestroyMissile();
        }
    }

    private void DestroyMissile()
    {
        OnDestroyed?.Invoke();

        if (missile != null)
        {
            missile.DestroyMissile();
            return;
        }

        Destroy(gameObject);
    }
}
