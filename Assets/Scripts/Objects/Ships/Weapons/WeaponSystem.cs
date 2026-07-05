using UnityEngine;

public abstract class WeaponSystem : MonoBehaviour
{
    [Header("Weapon")]
    public string weaponName;

    public float fireRate;
    public float energyCost;

    protected float fireCooldown;

    protected Ship ownerShip;

    public float CooldownRemaining => Mathf.Max(fireCooldown, 0f);
    public float CooldownDuration => fireRate > 0f ? 1f / fireRate : 0f;
    public float CooldownNormalized => CooldownDuration > 0f ? Mathf.Clamp01(CooldownRemaining / CooldownDuration) : 0f;

    protected virtual void Awake()
    {
        Initialize(GetComponentInParent<Ship>());
    }

    protected virtual void Update()
    {
        fireCooldown -= Time.deltaTime;
    }

    public virtual void Initialize(Ship ship)
    {
        ownerShip = ship;
    }

    public virtual bool CanFire()
    {
        return fireCooldown <= 0f
            && (ownerShip == null || !ownerShip.IsDestroyed)
            && (ownerShip == null || ownerShip.energy == null || ownerShip.energy.CanSpendEnergy(energyCost));
    }

    public virtual void Fire(Vector2 direction)
    {
        StartCooldown();
    }

    public virtual void Fire()
    {
        StartCooldown();
    }

    protected void StartCooldown()
    {
        if (ownerShip != null && ownerShip.energy != null)
        {
            ownerShip.energy.SpendEnergy(energyCost);
        }

        if (fireRate > 0f)
        {
            fireCooldown = 1f / fireRate;
        }
    }
}
