using System;

[Serializable]
public class MissionResult
{
    public MissionState FinalState { get; }
    public bool Succeeded { get; }
    public bool RelayReactivated { get; }
    public int DataRecoveredPercent { get; }
    public string ShipCondition { get; }
    public int EnemiesDestroyed { get; }
    public int SalvageRecovered { get; }
    public int CreditsEarned { get; }
    public string RelicStoryHint { get; }

    public MissionResult(
        MissionState finalState,
        bool succeeded,
        bool relayReactivated,
        int dataRecoveredPercent,
        string shipCondition,
        int enemiesDestroyed,
        int salvageRecovered,
        int creditsEarned,
        string relicStoryHint)
    {
        FinalState = finalState;
        Succeeded = succeeded;
        RelayReactivated = relayReactivated;
        DataRecoveredPercent = dataRecoveredPercent;
        ShipCondition = shipCondition;
        EnemiesDestroyed = enemiesDestroyed;
        SalvageRecovered = salvageRecovered;
        CreditsEarned = creditsEarned;
        RelicStoryHint = relicStoryHint;
    }
}
