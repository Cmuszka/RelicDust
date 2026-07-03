using UnityEngine;

public class ShipEnergy : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float regenPerSecond = 18f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private bool startFull = true;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float RegenPerSecond => regenPerSecond;
    public float NormalizedEnergy => maxEnergy > 0f ? Mathf.Clamp01(currentEnergy / maxEnergy) : 0f;

    private void Awake()
    {
        currentEnergy = startFull ? maxEnergy : Mathf.Clamp(currentEnergy, 0f, maxEnergy);
    }

    private void Update()
    {
        if (regenPerSecond <= 0f || currentEnergy >= maxEnergy)
        {
            return;
        }

        currentEnergy = Mathf.Min(currentEnergy + regenPerSecond * Time.deltaTime, maxEnergy);
    }

    public bool CanSpendEnergy(float amount)
    {
        return amount <= 0f || currentEnergy >= amount;
    }

    public bool SpendEnergy(float amount)
    {
        if (!CanSpendEnergy(amount))
        {
            return false;
        }

        currentEnergy = Mathf.Max(currentEnergy - Mathf.Max(amount, 0f), 0f);
        return true;
    }

    public void RechargeEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }
}
