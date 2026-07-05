using UnityEngine;

public class RelayObjective : MonoBehaviour, IDamageable
{
    [SerializeField] private float activationRadius = 5f;
    [SerializeField] private float uploadDuration = 30f;
    [SerializeField] private bool requirePlayerInRadius = true;

    [Header("Optional Integrity")]
    [SerializeField] private ShipTeam protectedTeam = ShipTeam.Player;
    [SerializeField] private bool ignoreNeutralDamage;
    [SerializeField] private bool useIntegrity;
    [SerializeField] private float maxIntegrity = 100f;
    [SerializeField] private float currentIntegrity = 100f;

    private Transform player;
    private float uploadTimer;

    public float UploadProgress => uploadDuration > 0f ? Mathf.Clamp01(uploadTimer / uploadDuration) : 1f;
    public bool IsUploading { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IsFailed => useIntegrity && currentIntegrity <= 0f;
    public float CurrentIntegrity => currentIntegrity;
    public float MaxIntegrity => maxIntegrity;

    private void Awake()
    {
        currentIntegrity = maxIntegrity;
    }

    private void Update()
    {
        if (!IsUploading || IsComplete || IsFailed)
        {
            return;
        }

        if (requirePlayerInRadius && !IsPlayerInActivationRange())
        {
            return;
        }

        uploadTimer += Time.deltaTime;
        if (uploadTimer >= uploadDuration)
        {
            uploadTimer = uploadDuration;
            IsComplete = true;
            IsUploading = false;
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    public void BeginUpload()
    {
        if (IsComplete || IsFailed)
        {
            return;
        }

        IsUploading = true;
    }

    public void ResetRelay()
    {
        uploadTimer = 0f;
        currentIntegrity = maxIntegrity;
        IsUploading = false;
        IsComplete = false;
    }

    public bool ShouldIgnoreDamageFrom(Ship sourceShip, ShipTeam sourceTeam)
    {
        if (sourceShip == null)
        {
            return ignoreNeutralDamage && sourceTeam == ShipTeam.Neutral;
        }

        return sourceTeam == protectedTeam;
    }

    public void TakeDamage(float damage)
    {
        if (!useIntegrity || damage <= 0f || IsFailed)
        {
            return;
        }

        currentIntegrity = Mathf.Max(currentIntegrity - damage, 0f);
    }

    public bool IsPlayerInActivationRange()
    {
        if (player == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, player.position) <= activationRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
