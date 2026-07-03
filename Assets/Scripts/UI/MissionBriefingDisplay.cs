using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionBriefingDisplay : MonoBehaviour
{
    [SerializeField] private string missionSceneName = "PlayTest";
    [SerializeField] private TextMeshProUGUI briefingText;
 
    [TextArea(8, 18)]
    [SerializeField] private string briefing =
        "Contract: Relay in the Gravefield\n\n"
        + "Employer: Black Fleet Survey Division\n\n"
        + "Objective: Reactivate a damaged relay inside the outer wreck field and recover its stored telemetry.\n\n"
        + "Expected Resistance: Scavenger fighters, cannon ships, possible missile craft.\n\n"
        + "Environmental Hazards: Dense debris field, limited maneuvering space, unstable relic signal.\n\n"
        + "Recommended Systems: CIWS, shield generator, heavy cannon or missile launcher.\n\n"
        + "Reward: Credits, salvage, relic data.\n\n"
        + "Intel Reliability: Medium";

    private void Start()
    {
        if (briefingText != null)
        {
            briefingText.text = briefing;
        }
    }

    public void Deploy()
    {
        SceneManager.LoadScene(missionSceneName);
    }
}
