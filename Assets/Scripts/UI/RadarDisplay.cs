using System.Collections.Generic;
using UnityEngine;

public class RadarDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform radarRoot;
    [SerializeField] private RadarBlip blipPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Range")]
    [SerializeField] private float worldRange = 40f;
    [SerializeField] private bool clampToEdge = true;

    [Header("Colors")]
    [SerializeField] private Color enemyShipColor = Color.red;
    [SerializeField] private Color missileColor = new Color(1f, 0.65f, 0.1f);
    [SerializeField] private Color selectedTargetColor = Color.cyan;
    [SerializeField] private Color selectedMissileColor = Color.yellow;

    [Header("Selection")]
    [SerializeField] private float selectedBlipScale = 1.6f;

    private readonly List<RadarBlip> activeBlips = new List<RadarBlip>();
    private readonly Queue<RadarBlip> pooledBlips = new Queue<RadarBlip>();

    private Ship playerShip;
    private TargetingSystem playerTargeting;

    public float WorldRange => worldRange;

    private void Awake()
    {
        if (radarRoot == null)
        {
            radarRoot = GetComponent<RectTransform>();
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
    }

    private void LateUpdate()
    {
        if (player == null || radarRoot == null || blipPrefab == null)
        {
            HideActiveBlips();
            return;
        }

        BeginRefresh();
        DrawShips();
        DrawMissiles();
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        playerShip = player != null ? player.GetComponent<Ship>() : null;
        playerTargeting = player != null ? player.GetComponent<TargetingSystem>() : null;
    }

    private void BeginRefresh()
    {
        for (int i = 0; i < activeBlips.Count; i++)
        {
            RadarBlip blip = activeBlips[i];
            blip.gameObject.SetActive(false);
            pooledBlips.Enqueue(blip);
        }

        activeBlips.Clear();
    }

    private void DrawShips()
    {
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);

        foreach (Ship ship in ships)
        {
            if (!IsRadarShipTarget(ship))
            {
                continue;
            }

            bool isSelected = playerTargeting != null && playerTargeting.currentTarget == ship;
            DrawBlip(ship.transform.position, isSelected ? selectedTargetColor : enemyShipColor, isSelected);
        }
    }

    private void DrawMissiles()
    {
        MissileProjectile[] missiles = FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None);

        foreach (MissileProjectile missile in missiles)
        {
            if (!IsRadarMissileTarget(missile))
            {
                continue;
            }

            bool isSelected = playerTargeting != null && playerTargeting.currentMissileTarget == missile;
            DrawBlip(missile.transform.position, isSelected ? selectedMissileColor : missileColor, isSelected);
        }
    }

    private void DrawBlip(Vector3 worldPosition, Color color, bool isSelected)
    {
        Vector2 radarPosition;
        if (!TryGetRadarPosition(worldPosition, out radarPosition))
        {
            return;
        }

        RadarBlip blip = GetBlip();
        blip.RectTransform.anchoredPosition = radarPosition;
        blip.RectTransform.localScale = isSelected ? Vector3.one * selectedBlipScale : Vector3.one;
        blip.SetColor(color);
        blip.gameObject.SetActive(true);
        activeBlips.Add(blip);
    }

    private bool TryGetRadarPosition(Vector3 worldPosition, out Vector2 radarPosition)
    {
        Vector2 offset = worldPosition - player.position;
        float distance = offset.magnitude;

        if (distance > worldRange && !clampToEdge)
        {
            radarPosition = Vector2.zero;
            return false;
        }

        Vector2 normalizedOffset = offset / worldRange;
        if (normalizedOffset.magnitude > 1f)
        {
            normalizedOffset = normalizedOffset.normalized;
        }

        float radarRadius = Mathf.Min(radarRoot.rect.width, radarRoot.rect.height) * 0.5f;
        radarPosition = normalizedOffset * radarRadius;
        return true;
    }

    private RadarBlip GetBlip()
    {
        if (pooledBlips.Count > 0)
        {
            return pooledBlips.Dequeue();
        }

        return Instantiate(blipPrefab, radarRoot);
    }

    private bool IsRadarShipTarget(Ship ship)
    {
        if (ship == null || ship == playerShip || ship.IsDestroyed)
        {
            return false;
        }

        if (ship.health != null && ship.health.IsDead)
        {
            return false;
        }

        if (playerShip == null)
        {
            return true;
        }

        return ship.Team != ShipTeam.Neutral && ship.Team != playerShip.Team;
    }

    private bool IsRadarMissileTarget(MissileProjectile missile)
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

    private void HideActiveBlips()
    {
        for (int i = 0; i < activeBlips.Count; i++)
        {
            activeBlips[i].gameObject.SetActive(false);
            pooledBlips.Enqueue(activeBlips[i]);
        }

        activeBlips.Clear();
    }
}
