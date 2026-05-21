using UnityEngine;

public class PlayerShieldAbility : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private float shieldDuration = 1.5f;
    [SerializeField] private float cooldownDuration = 4f;
    [SerializeField] private KeyCode activationKey = KeyCode.Space;

    [Header("Visual")]
    [SerializeField] private GameObject shieldVisual;

    public bool IsShieldActive { get; private set; }

    private float shieldTimer;
    private float cooldownTimer;

    private void Start()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsShieldActive)
        {
            shieldTimer -= Time.deltaTime;

            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
        else
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(activationKey) && cooldownTimer <= 0f)
            {
                ActivateShield();
            }
        }
    }

    private void ActivateShield()
    {
        IsShieldActive = true;
        shieldTimer = shieldDuration;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }

    private void DeactivateShield()
    {
        IsShieldActive = false;
        cooldownTimer = cooldownDuration;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    public float GetCooldownRemaining()
    {
        return cooldownTimer;
    }

    public float GetCooldownDuration()
    {
        return cooldownDuration;
    }
}