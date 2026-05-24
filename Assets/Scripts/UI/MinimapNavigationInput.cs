using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapNavigationInput : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private RectTransform minimapRoot;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerAutopilotAssist autopilot;
    [SerializeField] private bool findPlayerOnStart = true;
    [SerializeField] private Camera canvasCamera;

    [Header("Map")]
    [SerializeField] private float worldRange = 40f;
    [SerializeField] private RectTransform destinationMarker;

    private void Awake()
    {
        if (minimapRoot == null)
        {
            minimapRoot = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (findPlayerOnStart && player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                SetPlayer(playerObject.transform);
            }
        }
        else
        {
            SetPlayer(player);
        }

        if (destinationMarker != null)
        {
            destinationMarker.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        UpdateDestinationMarker();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (player == null || autopilot == null || minimapRoot == null)
        {
            return;
        }

        Vector2 localPoint;
        bool hasPoint = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRoot,
            eventData.position,
            canvasCamera,
            out localPoint);

        if (!hasPoint)
        {
            return;
        }

        Vector2 worldOffset = LocalPointToWorldOffset(localPoint);
        autopilot.SetDestination((Vector2)player.position + worldOffset);
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        autopilot = player != null ? player.GetComponent<PlayerAutopilotAssist>() : null;
    }

    private Vector2 LocalPointToWorldOffset(Vector2 localPoint)
    {
        float minimapRadius = Mathf.Min(minimapRoot.rect.width, minimapRoot.rect.height) * 0.5f;
        if (minimapRadius <= 0f)
        {
            return Vector2.zero;
        }

        Vector2 normalizedOffset = Vector2.ClampMagnitude(localPoint / minimapRadius, 1f);
        return normalizedOffset * worldRange;
    }

    private Vector2 WorldOffsetToLocalPoint(Vector2 worldOffset)
    {
        float minimapRadius = Mathf.Min(minimapRoot.rect.width, minimapRoot.rect.height) * 0.5f;
        Vector2 normalizedOffset = Vector2.ClampMagnitude(worldOffset / worldRange, 1f);

        return normalizedOffset * minimapRadius;
    }

    private void UpdateDestinationMarker()
    {
        if (destinationMarker == null || player == null || autopilot == null || !autopilot.HasDestination)
        {
            if (destinationMarker != null)
            {
                destinationMarker.gameObject.SetActive(false);
            }

            return;
        }

        Vector2 worldOffset = autopilot.Destination - (Vector2)player.position;
        destinationMarker.anchoredPosition = WorldOffsetToLocalPoint(worldOffset);
        destinationMarker.gameObject.SetActive(true);
    }
}
