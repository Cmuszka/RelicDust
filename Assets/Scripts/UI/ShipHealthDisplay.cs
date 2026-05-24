using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipHealthDisplay : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private bool findPlayerOnStart = true;

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

    private void Start()
    {
        if (findPlayerOnStart && shipHealth == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                shipHealth = playerObject.GetComponent<ShipHealth>();
            }
        }
    }

    private void Update()
    {
        if (shipHealth == null)
        {
            return;
        }

        UpdateLayer(
            shieldFill,
            shieldSlider,
            shieldText,
            shipHealth.ShieldHealth,
            shipHealth.MaxShieldHealth);

        UpdateLayer(
            armorFill,
            armorSlider,
            armorText,
            shipHealth.ArmorHealth,
            shipHealth.MaxArmorHealth);

        UpdateLayer(
            hullFill,
            hullSlider,
            hullText,
            shipHealth.HullStrength,
            shipHealth.MaxHullStrength);
    }

    public void SetTarget(ShipHealth newShipHealth)
    {
        shipHealth = newShipHealth;
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
            slider.maxValue = maxValue;
            slider.value = currentValue;
        }

        if (label != null)
        {
            label.text = Mathf.CeilToInt(currentValue) + " / " + Mathf.CeilToInt(maxValue);
        }
    }
}
