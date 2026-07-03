using TMPro;
using UnityEngine;

public class MissionObjectiveMarkerDisplay : MonoBehaviour
{
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private bool findMissionManagerOnStart = true;
    [SerializeField] private RectTransform marker;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Camera canvasCamera;
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private float screenEdgePadding = 36f;
    [SerializeField] private bool hideWhenTargetOnScreen;
 
    private CanvasGroup markerCanvasGroup;
    private Transform player;

    private void Awake()
    {
        if (markerRoot == null)
        {
            markerRoot = GetComponent<RectTransform>();
        }

        if (marker == null)
        {
            marker = transform as RectTransform;
        }

        markerCanvasGroup = marker != null ? marker.GetComponent<CanvasGroup>() : null;
    }

    private void Start()
    {
        if (findMissionManagerOnStart && missionManager == null)
        {
            missionManager = FindFirstObjectByType<MissionManager>();
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void LateUpdate()
    {
        if (missionManager == null || marker == null || markerRoot == null)
        {
            SetMarkerVisible(false);
            return;
        }

        Transform target = missionManager.ObjectiveTarget;
        if (target == null)
        {
            SetMarkerVisible(false);
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        Vector3 viewportPosition = worldCamera.WorldToViewportPoint(target.position);
        bool isOnScreen = viewportPosition.z >= 0f
            && viewportPosition.x >= 0f
            && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f
            && viewportPosition.y <= 1f;

        if (hideWhenTargetOnScreen && isOnScreen)
        {
            SetMarkerVisible(false);
            return;
        }

        Vector2 screenPosition = worldCamera.WorldToScreenPoint(target.position);
        screenPosition.x = Mathf.Clamp(screenPosition.x, screenEdgePadding, Screen.width - screenEdgePadding);
        screenPosition.y = Mathf.Clamp(screenPosition.y, screenEdgePadding, Screen.height - screenEdgePadding);

        Vector2 localPosition;
        bool hasPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            markerRoot,
            screenPosition,
            canvasCamera,
            out localPosition);

        if (!hasPosition)
        {
            SetMarkerVisible(false);
            return;
        }

        marker.anchoredPosition = localPosition;
        SetMarkerVisible(true);

        if (distanceText != null)
        {
            if (player != null)
            {
                float distance = Vector2.Distance(player.position, target.position);
                distanceText.text = Mathf.RoundToInt(distance) + "m";
            }
        }
    }

    private void SetMarkerVisible(bool isVisible)
    {
        if (marker == null)
        {
            return;
        }

        if (marker.gameObject == gameObject)
        {
            if (markerCanvasGroup == null)
            {
                markerCanvasGroup = marker.gameObject.AddComponent<CanvasGroup>();
            }

            markerCanvasGroup.alpha = isVisible ? 1f : 0f;
            markerCanvasGroup.blocksRaycasts = false;
            markerCanvasGroup.interactable = false;
        }
        else
        {
            marker.gameObject.SetActive(isVisible);
        }
    }
}
