using UnityEngine;

public abstract class UtilitySystem : MonoBehaviour
{
    public string utilityName;

    public float cooldown;
    public float energyCost;

    protected float cooldownTimer;

    protected Ship ownerShip;

    protected virtual void Awake()
    {
        Initialize(GetComponentInParent<Ship>());
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public virtual void Initialize(Ship ship)
    {
        ownerShip = ship;
    }

    public virtual bool CanActivate()
    {
        return cooldownTimer <= 0f
            && (ownerShip == null || !ownerShip.IsDestroyed)
            && (ownerShip == null || ownerShip.energy == null || ownerShip.energy.CanSpendEnergy(energyCost));
    }

    public virtual void Activate()
    {
        if (ownerShip != null && ownerShip.energy != null)
        {
            ownerShip.energy.SpendEnergy(energyCost);
        }

        cooldownTimer = cooldown;
    }
}
