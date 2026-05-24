using UnityEngine;

public enum ShipTeam
{
    Neutral,
    Player,
    Enemy
}

public class Ship : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private ShipTeam team = ShipTeam.Neutral;
    [SerializeField] private bool isDestroyed;

    [Header("Core Systems")]
    public ShipMovement movement;
    public ShipHealth health;
    public TargetingSystem targeting;

    [Header("Loadout")]
    public WeaponSystem[] weapons;
    public UtilitySystem[] utilities;

    public ShipTeam Team => team;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        CacheCoreSystems();
        RefreshSystems();
    }

    public void RefreshSystems()
    {
        weapons = GetComponentsInChildren<WeaponSystem>(true);
        utilities = GetComponentsInChildren<UtilitySystem>(true);

        foreach (WeaponSystem weapon in weapons)
        {
            weapon.Initialize(this);
        }

        foreach (UtilitySystem utility in utilities)
        {
            utility.Initialize(this);
        }
    }

    public void MarkDestroyed()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;
        DeactivateMovementAndFire();
    }

    public void DeactivateMovementAndFire()
    {
        PlayerShipController playerController = GetComponent<PlayerShipController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        EnemyShipController enemyController = GetComponent<EnemyShipController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        EnemyBehavior[] enemyBehaviors = GetComponents<EnemyBehavior>();
        foreach (EnemyBehavior enemyBehavior in enemyBehaviors)
        {
            if (enemyBehavior != null)
            {
                enemyBehavior.enabled = false;
            }
        }

        if (movement != null)
        {
            movement.StopMovementInput();
            movement.enabled = false;
        }

        if (weapons != null)
        {
            foreach (WeaponSystem weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.enabled = false;
                }
            }
        }

        if (utilities != null)
        {
            foreach (UtilitySystem utility in utilities)
            {
                if (utility != null)
                {
                    utility.enabled = false;
                }
            }
        }
    }

    private void CacheCoreSystems()
    {
        if (movement == null)
        {
            movement = GetComponent<ShipMovement>();
        }

        if (health == null)
        {
            health = GetComponent<ShipHealth>();
        }

        if (targeting == null)
        {
            targeting = GetComponent<TargetingSystem>();
        }
    }
}
