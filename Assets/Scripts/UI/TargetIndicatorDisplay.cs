using System.Collections.Generic;
using UnityEngine;

public class TargetIndicatorDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform indicatorRoot;
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private WorldTargetIndicator selectedTargetIndicatorPrefab;
    [SerializeField] private WorldTargetIndicator offscreenTargetArrowPrefab;
    [SerializeField] private WorldTargetIndicator missileIndicatorPrefab;
    [SerializeField] private Camera canvasCamera;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Colors")]
    [SerializeField] private Color selectedShipColor = Color.cyan;
    [SerializeField] private Color selectedMissileColor = new Color(1f, 0.65f, 0.1f);
    [SerializeField] private Color hostileMissileColor = Color.red;

    [Header("Missiles")]
    [SerializeField] private bool showAllHostileMissiles = true;

    [Header("Offscreen")]
    [SerializeField] private float screenEdgePadding = 40f;

    private readonly List<WorldTargetIndicator> activeMissileIndicators = new List<WorldTargetIndicator>();
    private readonly Queue<WorldTargetIndicator> pooledMissileIndicators = new Queue<WorldTargetIndicator>();

    private Camera mainCamera;
    private WorldTargetIndicator selectedTargetIndicator;
    private WorldTargetIndicator offscreenTargetArrow;
    private Ship playerShip;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (indicatorRoot == null)
        {
            indicatorRoot = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (findPlayerOnStart && targetingSystem == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                targetingSystem = playerObject.GetComponent<TargetingSystem>();
                playerShip = playerObject.GetComponent<Ship>();
            }
        }
        else if (targetingSystem != null)
        {
            playerShip = targetingSystem.GetComponent<Ship>();
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        UpdateSelectedTargetIndicator();
        UpdateMissileIndicators();
    }

    private void UpdateSelectedTargetIndicator()
    {
        if (targetingSystem == null || selectedTargetIndicatorPrefab == null || indicatorRoot == null)
        {
            HideSelectedTargetIndicator();
            return;
        }

        Transform targetTransform;
        Color targetColor;
        if (!TryGetSelectedTarget(out targetTransform, out targetColor))
        {
            HideSelectedTargetIndicator();
            return;
        }

        if (selectedTargetIndicator == null)
        {
            selectedTargetIndicator = Instantiate(selectedTargetIndicatorPrefab, indicatorRoot);
        }

        selectedTargetIndicator.SetColor(targetColor);
        bool isOnScreen = TryPlaceIndicator(selectedTargetIndicator.RectTransform, targetTransform.position);
        selectedTargetIndicator.gameObject.SetActive(isOnScreen);
        UpdateOffscreenArrow(targetTransform.position, targetColor, isOnScreen);
    }

    private bool TryGetSelectedTarget(out Transform targetTransform, out Color targetColor)
    {
        if (targetingSystem.currentMissileTarget != null)
        {
            targetTransform = targetingSystem.currentMissileTarget.transform;
            targetColor = selectedMissileColor;
            return true;
        }

        if (targetingSystem.currentTarget != null && !targetingSystem.currentTarget.IsDestroyed)
        {
            targetTransform = targetingSystem.currentTarget.transform;
            targetColor = selectedShipColor;
            return true;
        }

        targetTransform = null;
        targetColor = Color.white;
        return false;
    }

    private void HideSelectedTargetIndicator()
    {
        if (selectedTargetIndicator != null)
        {
            selectedTargetIndicator.gameObject.SetActive(false);
        }

        if (offscreenTargetArrow != null)
        {
            offscreenTargetArrow.gameObject.SetActive(false);
        }
    }

    private void UpdateOffscreenArrow(Vector3 worldPosition, Color color, bool selectedTargetIsOnScreen)
    {
        if (selectedTargetIsOnScreen || offscreenTargetArrowPrefab == null || indicatorRoot == null || mainCamera == null)
        {
            if (offscreenTargetArrow != null)
            {
                offscreenTargetArrow.gameObject.SetActive(false);
            }

            return;
        }

        if (offscreenTargetArrow == null)
        {
            offscreenTargetArrow = Instantiate(offscreenTargetArrowPrefab, indicatorRoot);
        }

        Vector2 localPosition;
        float angle;
        if (!TryGetOffscreenIndicatorPosition(worldPosition, out localPosition, out angle))
        {
            offscreenTargetArrow.gameObject.SetActive(false);
            return;
        }

        offscreenTargetArrow.RectTransform.anchoredPosition = localPosition;
        offscreenTargetArrow.RectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        offscreenTargetArrow.SetColor(color);
        offscreenTargetArrow.gameObject.SetActive(true);
    }

    private void UpdateMissileIndicators()
    {
        HideMissileIndicators();

        if (!showAllHostileMissiles || missileIndicatorPrefab == null || indicatorRoot == null)
        {
            return;
        }

        MissileProjectile[] missiles = FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None);
        foreach (MissileProjectile missile in missiles)
        {
            if (!IsHostileMissile(missile))
            {
                continue;
            }

            WorldTargetIndicator indicator = GetMissileIndicator();
            indicator.SetColor(hostileMissileColor);
            indicator.gameObject.SetActive(TryPlaceIndicator(indicator.RectTransform, missile.transform.position));
            activeMissileIndicators.Add(indicator);
        }
    }

    private bool TryPlaceIndicator(RectTransform indicator, Vector3 worldPosition)
    {
        if (mainCamera == null || indicatorRoot == null)
        {
            return false;
        }

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        if (viewportPosition.z < 0f)
        {
            return false;
        }

        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        Vector2 localPosition;
        bool hasPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorRoot,
            screenPosition,
            canvasCamera,
            out localPosition);

        if (!hasPosition)
        {
            return false;
        }

        indicator.anchoredPosition = localPosition;
        return IsOnScreen(viewportPosition);
    }

    private bool TryGetOffscreenIndicatorPosition(Vector3 worldPosition, out Vector2 localPosition, out float angle)
    {
        localPosition = Vector2.zero;
        angle = 0f;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        if (viewportPosition.z < 0f)
        {
            viewportPosition.x = 1f - viewportPosition.x;
            viewportPosition.y = 1f - viewportPosition.y;
        }

        Vector2 viewportCenter = new Vector2(0.5f, 0.5f);
        Vector2 directionFromCenter = new Vector2(viewportPosition.x, viewportPosition.y) - viewportCenter;
        if (directionFromCenter == Vector2.zero)
        {
            return false;
        }

        Vector2 clampedViewportPosition = viewportCenter + directionFromCenter.normalized * 0.5f;
        clampedViewportPosition.x = Mathf.Clamp(clampedViewportPosition.x, 0f, 1f);
        clampedViewportPosition.y = Mathf.Clamp(clampedViewportPosition.y, 0f, 1f);

        Vector2 screenPosition = new Vector2(
            clampedViewportPosition.x * Screen.width,
            clampedViewportPosition.y * Screen.height);

        screenPosition.x = Mathf.Clamp(screenPosition.x, screenEdgePadding, Screen.width - screenEdgePadding);
        screenPosition.y = Mathf.Clamp(screenPosition.y, screenEdgePadding, Screen.height - screenEdgePadding);

        bool hasPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorRoot,
            screenPosition,
            canvasCamera,
            out localPosition);

        if (!hasPosition)
        {
            return false;
        }

        angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg - 90f;
        return true;
    }

    private bool IsOnScreen(Vector3 viewportPosition)
    {
        return viewportPosition.x >= 0f
            && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f
            && viewportPosition.y <= 1f;
    }

    private WorldTargetIndicator GetMissileIndicator()
    {
        if (pooledMissileIndicators.Count > 0)
        {
            return pooledMissileIndicators.Dequeue();
        }

        return Instantiate(missileIndicatorPrefab, indicatorRoot);
    }

    private void HideMissileIndicators()
    {
        for (int i = 0; i < activeMissileIndicators.Count; i++)
        {
            WorldTargetIndicator indicator = activeMissileIndicators[i];
            indicator.gameObject.SetActive(false);
            pooledMissileIndicators.Enqueue(indicator);
        }

        activeMissileIndicators.Clear();
    }

    private bool IsHostileMissile(MissileProjectile missile)
    {
        if (missile == null)
        {
            return false;
        }

        if (playerShip == null)
        {
            return true;
        }

        return missile.Team != ShipTeam.Neutral && missile.Team != playerShip.Team;
    }
}
