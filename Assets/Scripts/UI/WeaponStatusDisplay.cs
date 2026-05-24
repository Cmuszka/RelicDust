using System.Collections.Generic;
using UnityEngine;

public class WeaponStatusDisplay : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerShipController playerController;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Slots")]
    [SerializeField] private Transform slotRoot;
    [SerializeField] private WeaponStatusSlot slotPrefab;
    [SerializeField] private Sprite[] weaponIcons;

    private readonly List<WeaponStatusSlot> slots = new List<WeaponStatusSlot>();

    private void Awake()
    {
        if (slotRoot == null)
        {
            slotRoot = transform;
        }
    }

    private void Start()
    {
        if (findPlayerOnStart && playerController == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerController = playerObject.GetComponent<PlayerShipController>();
            }
        }
    }

    private void Update()
    {
        UpdateDisplay();
    }

    public void SetTarget(PlayerShipController newPlayerController)
    {
        playerController = newPlayerController;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        WeaponSystem[] weapons = GetWeapons();
        int weaponCount = weapons != null ? weapons.Length : 0;

        EnsureSlotCount(weaponCount);

        for (int i = 0; i < slots.Count; i++)
        {
            bool isActiveSlot = i < weaponCount;
            slots[i].gameObject.SetActive(isActiveSlot);

            if (!isActiveSlot)
            {
                continue;
            }

            slots[i].SetWeapon(
                i,
                weapons[i],
                GetWeaponIcon(i),
                playerController != null && i == playerController.SelectedWeaponIndex);
        }
    }

    private WeaponSystem[] GetWeapons()
    {
        if (playerController == null)
        {
            return null;
        }

        Ship ship = playerController.GetComponent<Ship>();
        if (ship == null)
        {
            return null;
        }

        if (ship.weapons == null || ship.weapons.Length == 0)
        {
            ship.RefreshSystems();
        }

        return ship.weapons;
    }

    private void EnsureSlotCount(int weaponCount)
    {
        if (slotPrefab == null)
        {
            return;
        }

        while (slots.Count < weaponCount)
        {
            slots.Add(Instantiate(slotPrefab, slotRoot));
        }
    }

    private Sprite GetWeaponIcon(int weaponIndex)
    {
        if (weaponIcons == null || weaponIndex < 0 || weaponIndex >= weaponIcons.Length)
        {
            return null;
        }

        return weaponIcons[weaponIndex];
    }
}
