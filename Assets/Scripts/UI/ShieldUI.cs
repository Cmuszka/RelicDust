using TMPro;
using UnityEngine;

public class ShieldUI : MonoBehaviour
{
    [SerializeField] private PlayerShieldAbility shieldAbility;
    [SerializeField] private TextMeshProUGUI shieldText;

    private void Start() {
        int selected = LoadoutData.selectedAbility;

        if (selected == 0)
        {
            shieldText.enabled = true;
        }    
    }

    private void Update()
    {
        if (shieldAbility == null || shieldText == null) return;

        if (shieldAbility.IsShieldActive)
        {
            shieldText.text = "RELIC PHASE SHIELD: ACTIVE";
        }
        else if (shieldAbility.GetCooldownRemaining() > 0f)
        {
            shieldText.text = "RELIC PHASE SHIELD: " + shieldAbility.GetCooldownRemaining().ToString("F1");
        }
        else
        {
            shieldText.text = "RELIC PHASE SHIELD: READY";
        }
    }
}