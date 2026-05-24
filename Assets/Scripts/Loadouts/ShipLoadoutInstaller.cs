using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Ship))]
public class ShipLoadoutInstaller : MonoBehaviour
{
    [SerializeField] private ShipLoadoutDefinition loadoutDefinition;
    [SerializeField] private Transform systemsRoot;
    [SerializeField] private bool installOnStart = true;
    [SerializeField] private bool useSelectedLoadoutData;

    private readonly List<Component> installedSystems = new List<Component>();
    private Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Start()
    {
        if (installOnStart)
        {
            ShipLoadoutDefinition selectedLoadout =
                useSelectedLoadoutData && LoadoutData.selectedLoadout != null
                    ? LoadoutData.selectedLoadout
                    : loadoutDefinition;

            InstallLoadout(selectedLoadout);
        }
    }

    public void InstallLoadout(ShipLoadoutDefinition newLoadout)
    {
        if (newLoadout == null)
        {
            ship.RefreshSystems();
            return;
        }

        ClearInstalledSystems();
        loadoutDefinition = newLoadout;

        Transform parent = GetSystemsRoot();

        InstantiateWeaponSystems(newLoadout.weaponPrefabs, parent);
        InstantiateUtilitySystems(newLoadout.utilityPrefabs, parent);

        ship.RefreshSystems();
    }

    private void InstantiateWeaponSystems(WeaponSystem[] weaponPrefabs, Transform parent)
    {
        if (weaponPrefabs == null)
        {
            return;
        }

        foreach (WeaponSystem weaponPrefab in weaponPrefabs)
        {
            if (weaponPrefab == null)
            {
                continue;
            }

            WeaponSystem weapon = Instantiate(weaponPrefab, parent);
            weapon.Initialize(ship);
            installedSystems.Add(weapon);
        }
    }

    private void InstantiateUtilitySystems(UtilitySystem[] utilityPrefabs, Transform parent)
    {
        if (utilityPrefabs == null)
        {
            return;
        }

        foreach (UtilitySystem utilityPrefab in utilityPrefabs)
        {
            if (utilityPrefab == null)
            {
                continue;
            }

            UtilitySystem utility = Instantiate(utilityPrefab, parent);
            utility.Initialize(ship);
            installedSystems.Add(utility);
        }
    }

    private Transform GetSystemsRoot()
    {
        if (systemsRoot != null)
        {
            return systemsRoot;
        }

        GameObject root = new GameObject("Installed Systems");
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        systemsRoot = root.transform;

        return systemsRoot;
    }

    private void ClearInstalledSystems()
    {
        foreach (Component installedSystem in installedSystems)
        {
            if (installedSystem != null)
            {
                Destroy(installedSystem.gameObject);
            }
        }

        installedSystems.Clear();
    }
}
