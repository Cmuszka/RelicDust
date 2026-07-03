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
            missionManager.OnMissionCompleted.AddListener(ShowResult);
            missionManager.OnMissionFailed.AddListener(ShowResult);
        }
    }

    private void OnDestroy()
    {
        if (missionManager != null)
        {
            missionManager.OnMissionCompleted.RemoveListener(ShowResult);
            missionManager.OnMissionFailed.RemoveListener(ShowResult);
        }
    }

    public void ShowResult()
    {
        if (missionManager == null)
        {
            return;
        }

        if (resultRoot != null)
        {
            resultRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = missionManager.MissionSucceeded ? "MISSION COMPLETE" : "MISSION FAILED";
        }

        if (primaryObjectiveText != null)
        {
            primaryObjectiveText.text = "Relay Status: "
                + (missionManager.DataRecoveredPercent >= 100 ? "Reactivated" : "Incomplete")
                + "\nData Recovered: "
                + missionManager.DataRecoveredPercent
                + "%";
        }

        if (shipConditionText != null)
        {
            shipConditionText.text = "Ship Condition: " + GetPlayerCondition();
        }

        if (enemiesDestroyedText != null)
        {
            enemiesDestroyedText.text = "Enemies Destroyed: " + missionManager.EnemiesDestroyed;
        }

        if (rewardsText != null)
        {
            rewardsText.text = "Recovered Salvage: "
                + missionManager.SalvageRecovered
                + "\nCredits Earned: "
                + missionManager.CreditsEarned;
        }

        if (analysisText != null)
        {
            analysisText.text = "Black Fleet Analysis:\n\"" + missionManager.RelicStoryHint + "\"";
        }
    }

    private string GetPlayerCondition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        ShipHealth health = player != null ? player.GetComponent<ShipHealth>() : null;
        if (health == null)
        {
            return "Unknown";
        }

        float hullPercent = health.MaxHullStrength > 0f
            ? health.HullStrength / health.MaxHullStrength
            : 0f;

        if (hullPercent <= 0f)
        {
            return "Destroyed";
        }

        if (hullPercent < 0.35f)
        {
            return "Critical but operational";
        }

        if (hullPercent < 0.75f)
        {
            return "Damaged but operational";
        }

        return "Operational";
    }
}
