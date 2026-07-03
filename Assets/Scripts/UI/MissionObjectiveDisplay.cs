using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionObjectiveDisplay : MonoBehaviour
{
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private bool findMissionManagerOnStart = true;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject progressRoot;
    [SerializeField] private float phaseMessageDuration = 2f;
 
    private float phaseMessageTimer;

    private void Start()
    {
        if (findMissionManagerOnStart && missionManager == null)
        {
            missionManager = FindFirstObjectByType<MissionManager>();
        }

        if (missionManager != null)
        {
            missionManager.StateChanged += HandleStateChanged;
            HandleStateChanged(missionManager.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (missionManager != null)
        {
            missionManager.StateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (missionManager == null)
        {
            return;
        }

        if (objectiveText != null)
        {
            objectiveText.text = missionManager.CurrentObjectiveText;
        }

        float progress = missionManager.ObjectiveProgress;
        bool showProgress = missionManager.ObjectiveProgressVisible;

        if (progressRoot != null)
        {
            progressRoot.SetActive(showProgress);
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = progress;
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }

        if (progressText != null)
        {
            progressText.text = showProgress ? Mathf.RoundToInt(progress * 100f) + "%" : string.Empty;
        }

        UpdatePhaseMessage();
    }

    private void HandleStateChanged(MissionState state)
    {
        if (phaseText == null || missionManager == null)
        {
            return;
        }

        phaseText.text = missionManager.CurrentObjectiveText;
        phaseText.gameObject.SetActive(true);
        phaseMessageTimer = phaseMessageDuration;
    }

    private void UpdatePhaseMessage()
    {
        if (phaseText == null || phaseMessageTimer <= 0f)
        {
            return;
        }

        phaseMessageTimer -= Time.deltaTime;
        if (phaseMessageTimer <= 0f)
        {
            phaseText.gameObject.SetActive(false);
        }
    }
}
