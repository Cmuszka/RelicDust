using UnityEngine;

[RequireComponent(typeof(ShipSteeringComputer))]
public class PlayerAutopilotAssist : MonoBehaviour
{
    private enum AutopilotMode
    {
        SelectedTarget,
        Destination
    }

    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private bool autopilotEnabled;
    [SerializeField] private KeyCode toggleAutopilotKey = KeyCode.R;
    [SerializeField] private float preferredRange = 10f;
    [SerializeField] private float rangeTolerance = 2f;
    [SerializeField] private float destinationArrivalDistance = 1.5f;

    private ShipSteeringComputer steering;
    private AutopilotMode autopilotMode = AutopilotMode.SelectedTarget;
    private Vector2 destination;

    public bool AutopilotEnabled => autopilotEnabled;
    public bool HasDestination => autopilotMode == AutopilotMode.Destination && autopilotEnabled;
    public Vector2 Destination => destination;

    private void Awake()
    {
        steering = GetComponent<ShipSteeringComputer>();

        if (targetingSystem == null)
        {
            targetingSystem = GetComponent<TargetingSystem>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleAutopilotKey))
        {
            ToggleAutopilot();
        }

        if (!autopilotEnabled)
        {
            return;
        }

        UpdateAutopilot();
    }

    public void SetDestination(Vector2 worldPosition)
    {
        destination = worldPosition;
        autopilotMode = AutopilotMode.Destination;
        SetAutopilotEnabled(true);
    }

    public void ClearDestination()
    {
        if (autopilotMode == AutopilotMode.Destination)
        {
            steering.Stop();
            SetAutopilotEnabled(false);
        }
    }

    private void UpdateAutopilot()
    {
        if (autopilotMode == AutopilotMode.Destination)
        {
            UpdateDestinationAutopilot();
            return;
        }

        UpdateSelectedTargetAutopilot();
    }

    private void UpdateSelectedTargetAutopilot()
    {
        if (!HasSelectedTarget())
        {
            steering.Stop();
            return;
        }

        Vector2 targetPosition = targetingSystem.CurrentTargetPosition;
        float distance = Vector2.Distance(transform.position, targetPosition);

        if (Mathf.Abs(distance - preferredRange) <= rangeTolerance)
        {
            steering.FacePosition(targetPosition);
            return;
        }

        steering.KeepRangeFrom(targetPosition, preferredRange, rangeTolerance);
    }

    private void UpdateDestinationAutopilot()
    {
        float distance = Vector2.Distance(transform.position, destination);
        if (distance <= destinationArrivalDistance)
        {
            steering.Stop();
            SetAutopilotEnabled(false);
            return;
        }

        steering.MoveToward(destination);
    }

    public void ToggleAutopilot()
    {
        SetAutopilotEnabled(!autopilotEnabled);
    }

    public void SetAutopilotEnabled(bool isEnabled)
    {
        autopilotEnabled = isEnabled;

        if (autopilotEnabled && autopilotMode != AutopilotMode.Destination)
        {
            autopilotMode = AutopilotMode.SelectedTarget;
        }

        if (!autopilotEnabled && steering != null)
        {
            steering.Stop();
        }
    }

    private bool HasSelectedTarget()
    {
        return targetingSystem != null
            && (targetingSystem.currentTarget != null || targetingSystem.currentMissileTarget != null);
    }
}
