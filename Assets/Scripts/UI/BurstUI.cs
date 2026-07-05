using TMPro;
using UnityEngine;

public class BurstUI : MonoBehaviour
{
    [SerializeField] private PlayerBurstAbility burstAbility;
    [SerializeField] private TextMeshProUGUI burstText;

    private void Start() {
        int selected = LoadoutData.selectedAbility;

        if (selected == 1)
        {
            burstText.enabled = true;
        }    
    }

    private void Update()
    {
        if (burstAbility == null || burstText == null) return;

        else if (burstAbility.GetCooldownRemaining() > 0f)
        {
            burstText.text = "RELIC WAVE SHOCK: " + burstAbility.GetCooldownRemaining().ToString("F1");
        }
        else
        {
            burstText.text = "RELIC WAVE SHOCK: READY";
        }
    }
}