using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipEnergyDisplay : MonoBehaviour
{
    [SerializeField] private ShipEnergy shipEnergy;
    [SerializeField] private bool findPlayerOnStart = true;
    [SerializeField] private Image energyFill;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText;

    private void Start()
    {
        if (findPlayerOnStart && shipEnergy == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                shipEnergy = player.GetComponent<ShipEnergy>();
            }
        }
    }
 
    private void Update()
    {
        if (shipEnergy == null)
        {
            return;
        }

        if (energyFill != null)
        {
            energyFill.fillAmount = shipEnergy.NormalizedEnergy;
        }

        if (energySlider != null)
        {
            energySlider.maxValue = shipEnergy.MaxEnergy;
            energySlider.value = shipEnergy.CurrentEnergy;
        }

        if (energyText != null)
        {
            energyText.text = Mathf.CeilToInt(shipEnergy.CurrentEnergy)
                + " / "
                + Mathf.CeilToInt(shipEnergy.MaxEnergy);
        }
    }
}
