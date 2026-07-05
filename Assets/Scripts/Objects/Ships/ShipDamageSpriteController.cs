using UnityEngine;

public class ShipDamageSpriteController : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] hullStateSprites;

    private int currentSpriteIndex = -1;

    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (shipHealth == null || spriteRenderer == null || hullStateSprites == null || hullStateSprites.Length == 0)
        {
            return;
        }

        int spriteIndex = GetSpriteIndex();
        if (spriteIndex == currentSpriteIndex || hullStateSprites[spriteIndex] == null)
        {
            return;
        }

        currentSpriteIndex = spriteIndex;
        spriteRenderer.sprite = hullStateSprites[spriteIndex];
    }

    private int GetSpriteIndex()
    {
        if (shipHealth.MaxHullStrength <= 0f)
        {
            return hullStateSprites.Length - 1;
        }

        float hullPercent = Mathf.Clamp01(shipHealth.HullStrength / shipHealth.MaxHullStrength);
        int spriteIndex = Mathf.FloorToInt((1f - hullPercent) * hullStateSprites.Length);

        return Mathf.Clamp(spriteIndex, 0, hullStateSprites.Length - 1);
    }
}
