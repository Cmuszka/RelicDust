using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatusSlot : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private Image selectionHighlight;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.cyan;
    [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0.15f);

    public void SetWeapon(int weaponIndex, WeaponSystem weapon, Sprite icon, bool isSelected)
    {
        if (weaponText != null)
        {
            weaponText.text = GetWeaponLabel(weaponIndex, weapon);
        }

        if (weaponIcon != null)
        {
            weaponIcon.sprite = icon;
            weaponIcon.enabled = icon != null;
        }

        if (cooldownText != null)
        {
            cooldownText.text = GetCooldownText(weapon);
        }

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = weapon != null ? weapon.CooldownNormalized : 0f;
        }

        if (selectionHighlight != null)
        {
            selectionHighlight.color = isSelected ? selectedColor : unselectedColor;
        }
    }

    private static string GetWeaponLabel(int weaponIndex, WeaponSystem weapon)
    {
        if (weapon == null)
        {
            return (weaponIndex + 1) + " - EMPTY";
        }

        string weaponName = string.IsNullOrWhiteSpace(weapon.weaponName)
            ? weapon.GetType().Name
            : weapon.weaponName;

        return (weaponIndex + 1) + " - " + weaponName;
    }

    private static string GetCooldownText(WeaponSystem weapon)
    {
        if (weapon == null)
        {
            return "";
        }

        if (weapon.CooldownRemaining <= 0f)
        {
            return "READY";
        }

        return weapon.CooldownRemaining.ToString("F1");
    }
}
