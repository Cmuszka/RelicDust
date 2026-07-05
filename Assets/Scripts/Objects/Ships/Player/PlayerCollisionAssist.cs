using UnityEngine;

[RequireComponent(typeof(ShipMovement))]
[RequireComponent(typeof(ShipSteeringComputer))]
public class PlayerCollisionAssist : MonoBehaviour
{
    [SerializeField] private bool assistEnabled = true;
    [SerializeField] private float assistThrust = 0.75f;
    [SerializeField] private KeyCode toggleAssistKey = KeyCode.LeftAlt;

    private ShipMovement movement;
    private ShipSteeringComputer steering;

    public bool AssistEnabled => assistEnabled;

    private void Awake()
    {
        movement = GetComponent<ShipMovement>();
        steering = GetComponent<ShipSteeringComputer>();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(toggleAssistKey))
        {
            ToggleAssist();
        }

        if (!assistEnabled || !steering.HasPredictedCollision())
        {
            return;
        }

        Vector2 avoidanceDirection = steering.GetPredictedCollisionAvoidanceDirection();
        if (avoidanceDirection == Vector2.zero)
        {
            return;
        }

        float signedAngle = Vector2.SignedAngle(transform.up, avoidanceDirection);
        float rotationInput = Mathf.Sign(signedAngle);
        float thrustInput = Mathf.Abs(signedAngle) <= 45f ? assistThrust : 0f;

        movement.SetMovementInput(thrustInput, rotationInput);
    }

    public void ToggleAssist()
    {
        SetAssistEnabled(!assistEnabled);
    }

    public void SetAssistEnabled(bool isEnabled)
    {
        assistEnabled = isEnabled;
    }
}
