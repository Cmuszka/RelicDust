using UnityEngine;

public class ShieldUtility : UtilitySystem
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float temporaryShieldAmount = 100f;
    [SerializeField] private GameObject shieldVisual;

    public bool IsActive { get; private set; }

    private float activeTimer;
    private float grantedTemporaryShield;

    private void Start()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    private void OnDisable()
    {
        RemoveRemainingTemporaryShield();

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        IsActive = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!IsActive)
        {
            return;
        }

        activeTimer -= Time.deltaTime;

        if (activeTimer <= 0f)
        {
            Deactivate();
        }
    }

    public override bool CanActivate()
    {
        return !IsActive && base.CanActivate();
    }

    public override void Activate()
    {
        if (!CanActivate())
        {
            return;
        }

        base.Activate();
        IsActive = true;
        activeTimer = duration;
        grantedTemporaryShield = ownerShip != null && ownerShip.health != null
            ? ownerShip.health.AddTemporaryShield(temporaryShieldAmount)
            : 0f;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }

    private void Deactivate()
    {
        RemoveRemainingTemporaryShield();
        IsActive = false;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    private void RemoveRemainingTemporaryShield()
    {
        if (grantedTemporaryShield <= 0f || ownerShip == null || ownerShip.health == null)
        {
            grantedTemporaryShield = 0f;
            return;
        }

        grantedTemporaryShield -= ownerShip.health.RemoveTemporaryShield(grantedTemporaryShield);
    }
}
