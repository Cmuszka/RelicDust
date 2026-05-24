using UnityEngine;
using TMPro;

public class BossDefeatHandler : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private TextMeshProUGUI resultText;

    public void OnBossDefeated()
    {
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        if (resultText != null)
        {
            resultText.text = "VICTORY";
            resultText.gameObject.SetActive(true);
        }

        Debug.Log("Boss defeated");
    }
}