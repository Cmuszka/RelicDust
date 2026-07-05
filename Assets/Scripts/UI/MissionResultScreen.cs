using TMPro;
using UnityEngine;

public class MissionResultScreen : MonoBehaviour
{
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private bool findMissionManagerOnStart = true;
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI primaryObjectiveText;
    [SerializeField] private TextMeshProUGUI shipConditionText;
    [SerializeField] private TextMeshProUGUI enemiesDestroyedText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private TextMeshProUGUI analysisText;
 
    private void Start()
    {
        if (resultRoot != null)
        {
            resultRoot.SetActive(false);
        }

        if (findMissionManagerOnStart && missionManager == null)
        {
            missionManager = FindFirstObjectByType<MissionManager>();
        }

        if (missionManager != null)
        {
            missionManager.MissionResultCreated += ShowResult;
        }
    }

    private void OnDestroy()
    {
        if (missionManager != null)
        {
            missionManager.MissionResultCreated -= ShowResult;
        }
    }

    public void ShowResult()
    {
        if (missionManager == null)
        {
            return;
        }

        ShowResult(missionManager.LastResult);
    }

    public void ShowResult(MissionResult result)
    {
        if (result == null)
        {
            return;
        }

        if (resultRoot != null)
        {
            resultRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = result.Succeeded ? "MISSION COMPLETE" : "MISSION FAILED";
        }

        if (primaryObjectiveText != null)
        {
            primaryObjectiveText.text = "Relay Status: "
                + (result.RelayReactivated ? "Reactivated" : "Incomplete")
                + "\nData Recovered: "
                + result.DataRecoveredPercent
                + "%";
        }

        if (shipConditionText != null)
        {
            shipConditionText.text = "Ship Condition: " + result.ShipCondition;
        }

        if (enemiesDestroyedText != null)
        {
            enemiesDestroyedText.text = "Enemies Destroyed: " + result.EnemiesDestroyed;
        }

        if (rewardsText != null)
        {
            rewardsText.text = "Recovered Salvage: "
                + result.SalvageRecovered
                + "\nCredits Earned: "
                + result.CreditsEarned;
        }

        if (analysisText != null)
        {
            analysisText.text = "Black Fleet Analysis:\n\"" + result.RelicStoryHint + "\"";
        }
    }
}
