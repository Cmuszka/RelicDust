using UnityEngine;
using UnityEngine.UI;

public class RadarBlip : MonoBehaviour
{
    [SerializeField] private Image image;

    private RectTransform rectTransform;

    public RectTransform RectTransform => rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    public void SetColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
