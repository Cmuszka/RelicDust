using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityDuration = 0.75f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [SerializeField] private CameraShake cameraShake;


    private int currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private Color originalColor;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;

            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || isInvulnerable)
            return;

        currentHealth -= damage;
        cameraShake.Shake();

        if (spriteRenderer != null)
        {
            CancelInvoke(nameof(ResetColor));
            spriteRenderer.color = damageFlashColor;
            Invoke(nameof(ResetColor), flashDuration);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
{
    IsDead = true;
    Debug.Log("Player died");

    PlayerMovement movement = GetComponent<PlayerMovement>();
    if (movement != null) movement.enabled = false;

    PlayerShooting shooting = GetComponent<PlayerShooting>();
    if (shooting != null) shooting.enabled = false;

    gameObject.SetActive(false);
}

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}