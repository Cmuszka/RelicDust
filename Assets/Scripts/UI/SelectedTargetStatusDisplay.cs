using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedTargetStatusDisplay : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI targetNameText;
    [SerializeField] private TextMeshProUGUI targetTypeText;

    [Header("Shield")]
    [SerializeField] private Image shieldFill;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("Armor")]
    [SerializeField] private Image armorFill;
    [SerializeField] private Slider armorSlider;
    [SerializeField] private TextMeshProUGUI armorText;

    [Header("Hull")]
    [SerializeField] private Image hullFill;
    [SerializeField] private Slider hullSlider;
    [SerializeField] private TextMeshProUGUI hullText;

    [Header("Weapons")]
    [SerializeField] private TextMeshProUGUI weaponsText;
    [SerializeField] private string noWeaponsText = "No weapons detected";

    private readonly StringBuilder weaponBuilder = new StringBuilder();

    private void Awake()
    {
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = GetComponent<CanvasGroup>();
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
            }
        }
    }

    private void Update()
    {
        if (targetingSystem == null)
        {
            SetPanelVisible(false);
            return;
        }

        if (targetingSystem.currentMissileTarget != null)
        {
            UpdateMissileTarget(targetingSystem.currentMissileTarget);
            return;
        }

        if (targetingSystem.currentTarget != null)
        {
            UpdateShipTarget(targetingSystem.currentTarget);
            return;
        }

        SetPanelVisible(false);
    }

    private void UpdateShipTarget(Ship targetShip)
    {
        if (targetShip == null || targetShip.IsDestroyed)
        {
            SetPanelVisible(false);
            return;
        }

        SetPanelVisible(true);

        if (targetNameText != null)
        {
            targetNameText.text = targetShip.name;
        }

        if (targetTypeText != null)
        {
            targetTypeText.text = "SHIP";
        }

        ShipHealth health = targetShip.health != null ? targetShip.health : targetShip.GetComponent<ShipHealth>();
        if (health != null)
        {
            UpdateLayer(shieldFill, shieldSlider, shieldText, health.ShieldHealth, health.MaxShieldHealth);
            UpdateLayer(armorFill, armorSlider, armorText, health.ArmorHealth, health.MaxArmorHealth);
            UpdateLayer(hullFill, hullSlider, hullText, health.HullStrength, health.MaxHullStrength);
        }
        else
        {
            UpdateLayer(shieldFill, shieldSlider, shieldText, 0f, 0f);
            UpdateLayer(armorFill, armorSlider, armorText, 0f, 0f);
            UpdateLayer(hullFill, hullSlider, hullText, 0f, 0f);
        }

        UpdateWeapons(targetShip);
    }

    private void UpdateMissileTarget(MissileProjectile missile)
    {
        if (missile == null)
        {
            SetPanelVisible(false);
            return;
        }

        SetPanelVisible(true);

        if (targetNameText != null)
        {
            targetNameText.text = missile.name;
        }

        if (targetTypeText != null)
        {
            targetTypeText.text = "MISSILE";
        }

        MissileHealth missileHealth = missile.GetComponent<MissileHealth>();
        float currentHealth = missileHealth != null ? missileHealth.CurrentHealth : 0f;
        float maxHealth = missileHealth != null ? missileHealth.MaxHealth : 0f;

        UpdateLayer(shieldFill, shieldSlider, shieldText, 0f, 0f);
        UpdateLayer(armorFill, armorSlider, armorText, 0f, 0f);
        UpdateLayer(hullFill, hullSlider, hullText, currentHealth, maxHealth);

        if (weaponsText != null)
        {
            weaponsText.text = noWeaponsText;
        }
    }

    private void UpdateWeapons(Ship targetShip)
    {
        if (weaponsText == null)
        {
            return;
        }

        WeaponSystem[] weapons = targetShip.weapons;
        if (weapons == null || weapons.Length == 0)
        {
            targetShip.RefreshSystems();
            weapons = targetShip.weapons;
        }

        if (weapons == null || weapons.Length == 0)
        {
            weaponsText.text = noWeaponsText;
            return;
        }

        weaponBuilder.Clear();
        for (int i = 0; i < weapons.Length; i++)
        {
            WeaponSystem weapon = weapons[i];
            if (weapon == null)
            {
                continue;
            }

            string weaponName = string.IsNullOrWhiteSpace(weapon.weaponName)
                ? weapon.GetType().Name
                : weapon.weaponName;

            if (weaponBuilder.Length > 0)
            {
                weaponBuilder.AppendLine();
            }

            weaponBuilder.Append(i + 1);
            weaponBuilder.Append(" - ");
            weaponBuilder.Append(weaponName);
        }

        weaponsText.text = weaponBuilder.Length > 0 ? weaponBuilder.ToString() : noWeaponsText;
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = isVisible ? 1f : 0f;
            panelCanvasGroup.interactable = isVisible;
            panelCanvasGroup.blocksRaycasts = isVisible;
            return;
        }

        if (panelRoot != null && panelRoot != gameObject && panelRoot.activeSelf != isVisible)
        {
            panelRoot.SetActive(isVisible);
        }
    }

    private static void UpdateLayer(
        Image fillImage,
        Slider slider,
        TextMeshProUGUI label,
        float currentValue,
        float maxValue)
    {
        float normalizedValue = maxValue > 0f ? Mathf.Clamp01(currentValue / maxValue) : 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = normalizedValue;
        }

        if (slider != null)
        {
            slider.maxValue = Mathf.Max(maxValue, 1f);
            slider.value = Mathf.Clamp(currentValue, 0f, slider.maxValue);
        }

        if (label != null)
        {
            label.text = maxValue > 0f
                ? Mathf.CeilToInt(currentValue) + " / " + Mathf.CeilToInt(maxValue)
                : "-";
        }
    }
}
