using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipThrustDisplay : MonoBehaviour
{
    [SerializeField] private ShipMovement shipMovement;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Display")]
    [SerializeField] private Image forwardFill;
    [SerializeField] private Image reverseFill;
    [SerializeField] private Slider throttleSlider;
    [SerializeField] private TextMeshProUGUI throttleText;
    [SerializeField] private TextMeshProUGUI engineStateText;

    private void Start()
    {
        if (findPlayerOnStart && shipMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                shipMovement = player.GetComponent<ShipMovement>();
            }
        }

        if (throttleSlider != null)
        {
            throttleSlider.minValue = -1f;
            throttleSlider.maxValue = 1f;
        }
    }

    private void Update()
    {
        if (shipMovement == null)
        {
            return;
        }

        float throttle = shipMovement.EngineThrottle;
        float forward = shipMovement.NormalizedForwardThrottle;
        float reverse = shipMovement.NormalizedReverseThrottle;

        if (forwardFill != null)
        {
            forwardFill.fillAmount = forward;
        }

        if (reverseFill != null)
        {
            reverseFill.fillAmount = reverse;
        }

        if (throttleSlider != null)
        {
            throttleSlider.value = throttle;
        }

        if (throttleText != null)
        {
            throttleText.text = Mathf.RoundToInt(throttle * 100f) + "%";
        }

        if (engineStateText != null)
        {
            engineStateText.text = shipMovement.EngineIsOn ? "THRUST" : "CUT";
        }
    }
}
