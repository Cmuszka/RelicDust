using UnityEngine;

[CreateAssetMenu(menuName = "Relic Dust/Ship Loadout", fileName = "ShipLoadout")]
public class ShipLoadoutDefinition : ScriptableObject
{
    public string loadoutName;
    public WeaponSystem[] weaponPrefabs;
    public UtilitySystem[] utilityPrefabs;
}
