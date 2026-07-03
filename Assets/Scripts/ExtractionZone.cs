using UnityEngine;

public class ExtractionZone : MonoBehaviour
{
    [SerializeField] private float extractionRadius = 5f;
    [SerializeField] private float extractionDuration = 3f;
    [SerializeField] private bool resetProgressWhenPlayerLeaves = true;
    [SerializeField] private GameObject extractionVisual;

    private Transform player;
    private float extractionTimer;

    public bool IsExtractionEnabled { get; private set; }
    public bool IsComplete { get; private set; }
    public float ExtractionProgress => extractionDuration > 0f ? Mathf.Clamp01(extractionTimer / extractionDuration) : 1f;

    private void Awake()
    {
        SetExtractionEnabled(IsExtractionEnabled);
    }

    private void Update()
    {
        if (!IsExtractionEnabled || IsComplete || player == null)
        {
            return;
        }

        if (IsPlayerInExtractionRange())
        {
            extractionTimer += Time.deltaTime;
            if (extractionTimer >= extractionDuration)
            {
                extractionTimer = extractionDuration;
                IsComplete = true;
            }
        }
        else if (resetProgressWhenPlayerLeaves)
        {
            extractionTimer = 0f;
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    public void SetExtractionEnabled(bool isEnabled)
    {
        IsExtractionEnabled = isEnabled;

        if (!IsExtractionEnabled)
        {
            extractionTimer = 0f;
            IsComplete = false;
        }

        if (extractionVisual != null)
        {
            extractionVisual.SetActive(IsExtractionEnabled);
        }
    }

    public bool IsPlayerInExtractionRange()
    {
        if (player == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, player.position) <= extractionRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, extractionRadius);
    }
}
