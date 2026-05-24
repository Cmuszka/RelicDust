using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRigidbody;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Follow")]
    [SerializeField] private float followSmoothTime = 0.25f;
    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 0f, -10f);

    [Header("Velocity Look Ahead")]
    [SerializeField] private float maxLookAheadDistance = 4f;
    [SerializeField] private float speedForMaxLookAhead = 15f;
    [SerializeField] private float lookAheadSmoothTime = 0.35f;

    [Header("Zoom")]
    [SerializeField] private float minOrthographicSize = 5f;
    [SerializeField] private float maxOrthographicSize = 12f;
    [SerializeField] private float zoomStep = 1f;
    [SerializeField] private float zoomSmoothTime = 0.15f;
    [SerializeField] private bool dynamicThreatZoom = true;
    [SerializeField] private float calmOrthographicSize = 6f;
    [SerializeField] private float threatScanRadius = 25f;
    [SerializeField] private float shipThreatWeight = 1f;
    [SerializeField] private float missileThreatWeight = 1.5f;
    [SerializeField] private float threatScoreForMaxZoom = 6f;
    [SerializeField] private float manualZoomOffsetMin = -2f;
    [SerializeField] private float manualZoomOffsetMax = 2f;

    private Camera cameraComponent;
    private Vector3 followVelocity;
    private Vector2 currentLookAhead;
    private Vector2 lookAheadVelocity;
    private float targetOrthographicSize;
    private float manualZoomOffset;
    private float zoomVelocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        if (cameraComponent != null)
        {
            targetOrthographicSize = cameraComponent.orthographicSize;
        }
    }

    private void Start()
    {
        if (findPlayerOnStart && target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                SetTarget(playerObject.transform);
            }
        }
        else if (target != null && targetRigidbody == null)
        {
            targetRigidbody = target.GetComponent<Rigidbody2D>();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleZoomInput();
        UpdateZoom();

        Vector2 desiredLookAhead = GetDesiredLookAhead();
        currentLookAhead = Vector2.SmoothDamp(
            currentLookAhead,
            desiredLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime);

        Vector3 targetPosition = target.position + baseOffset + (Vector3)currentLookAhead;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref followVelocity,
            followSmoothTime);
    }

    public void ZoomIn()
    {
        SetManualZoomOffset(manualZoomOffset - zoomStep);
    }

    public void ZoomOut()
    {
        SetManualZoomOffset(manualZoomOffset + zoomStep);
    }

    public void SetZoom(float orthographicSize)
    {
        if (dynamicThreatZoom)
        {
            SetManualZoomOffset(orthographicSize - GetDynamicZoomSize());
            return;
        }

        targetOrthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetRigidbody = target != null ? target.GetComponent<Rigidbody2D>() : null;
    }

    private Vector2 GetDesiredLookAhead()
    {
        if (targetRigidbody == null || targetRigidbody.linearVelocity == Vector2.zero)
        {
            return Vector2.zero;
        }

        Vector2 velocity = targetRigidbody.linearVelocity;
        float speedRatio = Mathf.Clamp01(velocity.magnitude / speedForMaxLookAhead);

        return velocity.normalized * maxLookAheadDistance * speedRatio;
    }

    private void HandleZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        if (dynamicThreatZoom)
        {
            SetManualZoomOffset(manualZoomOffset - scroll * zoomStep);
            return;
        }

        SetZoom(targetOrthographicSize - scroll * zoomStep);
    }

    private void UpdateZoom()
    {
        if (cameraComponent == null || !cameraComponent.orthographic)
        {
            return;
        }

        if (dynamicThreatZoom)
        {
            targetOrthographicSize = Mathf.Clamp(
                GetDynamicZoomSize() + manualZoomOffset,
                minOrthographicSize,
                maxOrthographicSize);
        }

        cameraComponent.orthographicSize = Mathf.SmoothDamp(
            cameraComponent.orthographicSize,
            targetOrthographicSize,
            ref zoomVelocity,
            zoomSmoothTime);
    }

    private void SetManualZoomOffset(float newOffset)
    {
        manualZoomOffset = Mathf.Clamp(newOffset, manualZoomOffsetMin, manualZoomOffsetMax);
    }

    private float GetDynamicZoomSize()
    {
        float threatScore = GetThreatScore();
        float threatRatio = Mathf.Clamp01(threatScore / threatScoreForMaxZoom);

        return Mathf.Lerp(calmOrthographicSize, maxOrthographicSize, threatRatio);
    }

    private float GetThreatScore()
    {
        if (target == null)
        {
            return 0f;
        }

        float threatScore = 0f;
        Vector2 targetPosition = target.position;

        Ship ownerShip = target.GetComponent<Ship>();
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (Ship ship in ships)
        {
            if (!IsHostileShip(ownerShip, ship))
            {
                continue;
            }

            threatScore += GetWeightedThreat(targetPosition, ship.transform.position, shipThreatWeight);
        }

        MissileProjectile[] missiles = FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None);
        foreach (MissileProjectile missile in missiles)
        {
            if (!IsHostileMissile(ownerShip, missile))
            {
                continue;
            }

            threatScore += GetWeightedThreat(targetPosition, missile.transform.position, missileThreatWeight);
        }

        return threatScore;
    }

    private float GetWeightedThreat(Vector2 targetPosition, Vector2 threatPosition, float weight)
    {
        float distance = Vector2.Distance(targetPosition, threatPosition);
        if (distance > threatScanRadius)
        {
            return 0f;
        }

        float proximity = 1f - distance / threatScanRadius;
        return weight * Mathf.Lerp(0.35f, 1f, proximity);
    }

    private static bool IsHostileShip(Ship ownerShip, Ship candidate)
    {
        if (candidate == null || candidate == ownerShip || candidate.IsDestroyed)
        {
            return false;
        }

        if (candidate.health != null && candidate.health.IsDead)
        {
            return false;
        }

        if (ownerShip == null)
        {
            return true;
        }

        return candidate.Team != ShipTeam.Neutral && candidate.Team != ownerShip.Team;
    }

    private static bool IsHostileMissile(Ship ownerShip, MissileProjectile missile)
    {
        if (missile == null)
        {
            return false;
        }

        if (ownerShip == null)
        {
            return true;
        }

        return missile.Team != ShipTeam.Neutral && missile.Team != ownerShip.Team;
    }
}
