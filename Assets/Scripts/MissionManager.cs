using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum MissionState
{
    MissionStart,
    ApproachRelay,
    ActivateRelay,
    DefendRelay,
    Escalation,
    Extraction,
    MissionComplete,
    MissionFailed
}

[Serializable]
public class MissionSpawnGroup
{
    public GameObject enemyPrefab;
    public int count = 1;
    public Transform[] spawnPoints;
    public float spawnRadius = 3f;
    public float delayBetweenSpawns = 0.25f;
}

[Serializable]
public class MissionWave
{
    public string waveName;
    public MissionSpawnGroup[] groups;
}

public class MissionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private RelayObjective relayObjective;
    [SerializeField] private ExtractionZone extractionZone;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Mission")]
    [SerializeField] private bool startOnStart = true;
    [SerializeField] private float escalationUploadProgress = 0.55f;

    [Header("Objectives")]
    [SerializeField] private string approachRelayText = "Reach the damaged relay.";
    [SerializeField] private string activateRelayText = "Activate the relay.";
    [SerializeField] private string defendRelayText = "Defend the relay while data uploads.";
    [SerializeField] private string escalationText = "Missile craft detected. Survive the escalation.";
    [SerializeField] private string extractionText = "Reach the extraction zone.";
    [SerializeField] private string completeText = "Mission complete.";
    [SerializeField] private string failedText = "Mission failed.";

    [Header("Enemy Waves")]
    [SerializeField] private MissionWave openingWave;
    [SerializeField] private MissionWave relayDefenseWave;
    [SerializeField] private MissionWave escalationWave;
    [SerializeField] private MissionWave extractionWave;

    [Header("Result")]
    [SerializeField] private int salvageReward = 42;
    [SerializeField] private int creditReward = 350;
    [SerializeField] private string relicStoryHint =
        "Recovered telemetry contains fragments from an earlier Black Fleet deployment. No matching operation is recorded.";

    public UnityEvent OnMissionCompleted = new UnityEvent();
    public UnityEvent OnMissionFailed = new UnityEvent();

    public event Action<MissionState> StateChanged;

    public MissionState CurrentState { get; private set; } = MissionState.MissionStart;
    public int EnemiesDestroyed { get; private set; }
    public bool MissionEnded => CurrentState == MissionState.MissionComplete || CurrentState == MissionState.MissionFailed;
    public bool MissionSucceeded => CurrentState == MissionState.MissionComplete;
    public int SalvageRecovered => MissionSucceeded ? salvageReward : 0;
    public int CreditsEarned => MissionSucceeded ? creditReward : 0;
    public string RelicStoryHint => relicStoryHint;

    public string CurrentObjectiveText
    {
        get
        {
            switch (CurrentState)
            {
                case MissionState.ApproachRelay:
                    return approachRelayText;
                case MissionState.ActivateRelay:
                    return activateRelayText;
                case MissionState.DefendRelay:
                    return defendRelayText;
                case MissionState.Escalation:
                    return escalationText;
                case MissionState.Extraction:
                    return extractionText;
                case MissionState.MissionComplete:
                    return completeText;
                case MissionState.MissionFailed:
                    return failedText;
                default:
                    return approachRelayText;
            }
        }
    }

    public bool ObjectiveProgressVisible =>
        CurrentState == MissionState.ActivateRelay
        || CurrentState == MissionState.DefendRelay
        || CurrentState == MissionState.Escalation
        || CurrentState == MissionState.Extraction;

    public float ObjectiveProgress
    {
        get
        {
            if (CurrentState == MissionState.Extraction && extractionZone != null)
            {
                return extractionZone.ExtractionProgress;
            }

            return relayObjective != null ? relayObjective.UploadProgress : 0f;
        }
    }

    public Transform ObjectiveTarget
    {
        get
        {
            if (CurrentState == MissionState.Extraction && extractionZone != null)
            {
                return extractionZone.transform;
            }

            if (!MissionEnded && relayObjective != null)
            {
                return relayObjective.transform;
            }

            return null;
        }
    }

    public int DataRecoveredPercent =>
        relayObjective != null ? Mathf.RoundToInt(relayObjective.UploadProgress * 100f) : 0;

    private bool relayStarted;
    private bool escalationStarted;
    private bool extractionStarted;

    private void Start()
    {
        ResolveReferences();

        if (startOnStart)
        {
            StartMission();
        }
    }

    private void Update()
    {
        if (MissionEnded)
        {
            return;
        }

        if (playerShip != null && (playerShip.IsDestroyed || playerShip.health != null && playerShip.health.IsDead))
        {
            FailMission();
            return;
        }

        UpdateMissionState();
    }

    public void StartMission()
    {
        ResolveReferences();

        if (relayObjective != null && playerShip != null)
        {
            relayObjective.SetPlayer(playerShip.transform);
        }

        if (extractionZone != null && playerShip != null)
        {
            extractionZone.SetPlayer(playerShip.transform);
            extractionZone.SetExtractionEnabled(false);
        }

        relayStarted = false;
        escalationStarted = false;
        extractionStarted = false;
        EnemiesDestroyed = 0;

        SetState(MissionState.ApproachRelay);
        StartCoroutine(SpawnWave(openingWave));
    }

    public void CompleteMission()
    {
        if (MissionEnded)
        {
            return;
        }

        SetState(MissionState.MissionComplete);
        OnMissionCompleted?.Invoke();
    }

    public void FailMission()
    {
        if (MissionEnded)
        {
            return;
        }

        SetState(MissionState.MissionFailed);
        OnMissionFailed?.Invoke();
    }

    public void RegisterEnemy(Ship enemyShip)
    {
        if (enemyShip == null || enemyShip.health == null)
        {
            return;
        }

        enemyShip.health.OnDeath.AddListener(HandleEnemyDestroyed);
    }

    private void UpdateMissionState()
    {
        if (relayObjective != null && relayObjective.IsFailed)
        {
            FailMission();
            return;
        }

        switch (CurrentState)
        {
            case MissionState.ApproachRelay:
                if (relayObjective != null && relayObjective.IsPlayerInActivationRange())
                {
                    StartRelayUpload();
                }
                break;
            case MissionState.DefendRelay:
            case MissionState.Escalation:
                UpdateRelayDefense();
                break;
            case MissionState.Extraction:
                if (extractionZone != null && extractionZone.IsComplete)
                {
                    CompleteMission();
                }
                break;
        }
    }

    private void StartRelayUpload()
    {
        if (relayStarted || relayObjective == null)
        {
            return;
        }

        relayStarted = true;
        SetState(MissionState.ActivateRelay);
        relayObjective.BeginUpload();
        SetState(MissionState.DefendRelay);
        StartCoroutine(SpawnWave(relayDefenseWave));
    }

    private void UpdateRelayDefense()
    {
        if (relayObjective == null)
        {
            return;
        }

        if (!escalationStarted && relayObjective.UploadProgress >= escalationUploadProgress)
        {
            escalationStarted = true;
            SetState(MissionState.Escalation);
            StartCoroutine(SpawnWave(escalationWave));
        }

        if (relayObjective.IsComplete)
        {
            StartExtraction();
        }
    }

    private void StartExtraction()
    {
        if (extractionStarted)
        {
            return;
        }

        extractionStarted = true;
        SetState(MissionState.Extraction);

        if (extractionZone != null)
        {
            extractionZone.SetExtractionEnabled(true);
        }

        StartCoroutine(SpawnWave(extractionWave));
    }

    private IEnumerator SpawnWave(MissionWave wave)
    {
        if (wave == null || wave.groups == null)
        {
            yield break;
        }

        foreach (MissionSpawnGroup group in wave.groups)
        {
            if (MissionEnded)
            {
                yield break;
            }

            if (group == null || group.enemyPrefab == null)
            {
                continue;
            }

            for (int i = 0; i < group.count; i++)
            {
                if (MissionEnded)
                {
                    yield break;
                }

                GameObject enemyObject = Instantiate(
                    group.enemyPrefab,
                    GetSpawnPosition(group),
                    Quaternion.identity);

                Ship enemyShip = enemyObject.GetComponent<Ship>();
                RegisterEnemy(enemyShip);

                if (group.delayBetweenSpawns > 0f)
                {
                    yield return new WaitForSeconds(group.delayBetweenSpawns);
                }
            }
        }
    }

    private Vector3 GetSpawnPosition(MissionSpawnGroup group)
    {
        Transform spawnPoint = null;
        if (group.spawnPoints != null && group.spawnPoints.Length > 0)
        {
            spawnPoint = group.spawnPoints[UnityEngine.Random.Range(0, group.spawnPoints.Length)];
        }

        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * Mathf.Max(group.spawnRadius, 0f);
        return center + (Vector3)offset;
    }

    private void HandleEnemyDestroyed()
    {
        EnemiesDestroyed++;
    }

    private void SetState(MissionState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }

    private void ResolveReferences()
    {
        if (findPlayerOnStart && playerShip == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerShip = playerObject.GetComponent<Ship>();
            }
        }

        if (relayObjective == null)
        {
            relayObjective = FindFirstObjectByType<RelayObjective>();
        }

        if (extractionZone == null)
        {
            extractionZone = FindFirstObjectByType<ExtractionZone>();
        }
    }
}
