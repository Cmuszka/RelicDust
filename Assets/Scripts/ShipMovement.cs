using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipMovement : MonoBehaviour
{
    [Header("Control")]
    [SerializeField] private bool readPlayerInput;

    [Header("Movement")]
    [SerializeField] private float thrustForce = 4f;
    [SerializeField] private float reverseForce = 2f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Manual Engine Throttle")]
    [SerializeField] private bool usePersistentThrottle = true;
    [SerializeField] private float maxReverseThrottle = 0.35f;
    [SerializeField] private float throttleIncreasePerSecond = 0.65f;
    [SerializeField] private float throttleDecreasePerSecond = 0.9f;
    [SerializeField] private KeyCode cutEngineKey = KeyCode.X;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 270f;

    [Header("Drag")]
    [SerializeField] private float linearDrag = 0f;
    [SerializeField] private float engineCutLinearDrag = 2f;
    [SerializeField] private float angularDrag = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string engineOnParameter = "EngineOn";

    private Rigidbody2D rb;

    private float thrustInput;
    private float rotationInput;
    private float engineThrottle;
    private float thrustMultiplier = 1f;
    private float maxSpeedMultiplier = 1f;
    private const float EngineOnThreshold = 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;

        if (!readPlayerInput)
        {
            readPlayerInput = CompareTag("Player") || GetComponent<PlayerShipController>() != null;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (!readPlayerInput)
        {
            UpdateAnimator();
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");
        UpdateManualThrottle(verticalInput);
        rotationInput = -Input.GetAxisRaw("Horizontal");
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
        UpdateLinearDrag();
        ClampVelocity();
    }

    private void HandleRotation()
    {
        rb.MoveRotation(rb.rotation + rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void HandleMovement()
    {
        Vector2 forward = transform.up;

        if (thrustInput > 0)
        {
            rb.AddForce(forward * thrustForce * thrustMultiplier);
        }
        else if (thrustInput < 0)
        {
            rb.AddForce(-forward * reverseForce);
        }
    }

    private void ClampVelocity()
    {
        float currentMaxSpeed = maxSpeed * maxSpeedMultiplier;

        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    public void SetMovementMultipliers(float newThrustMultiplier, float newMaxSpeedMultiplier)
    {
        thrustMultiplier = newThrustMultiplier;
        maxSpeedMultiplier = newMaxSpeedMultiplier;
    }

    public void ResetMovementMultipliers()
    {
        thrustMultiplier = 1f;
        maxSpeedMultiplier = 1f;
    }

    public void SetMovementInput(float newThrustInput, float newRotationInput)
    {
        thrustInput = Mathf.Clamp(newThrustInput, -1f, 1f);
        rotationInput = Mathf.Clamp(newRotationInput, -1f, 1f);
        UpdateAnimator();
    }

    public void SetManualMovementInput(float accelerateInput, float decelerateInput, float newRotationInput)
    {
        UpdateManualThrottle(Mathf.Clamp(accelerateInput, -1f, 1f), Mathf.Clamp01(decelerateInput));
        rotationInput = Mathf.Clamp(newRotationInput, -1f, 1f);
        UpdateAnimator();
    }

    public void StopMovementInput()
    {
        thrustInput = 0f;
        rotationInput = 0f;
        UpdateAnimator();
    }

    public void SetReadPlayerInput(bool shouldReadPlayerInput)
    {
        readPlayerInput = shouldReadPlayerInput;
        StopMovementInput();
    }

    public void CutEngine()
    {
        engineThrottle = 0f;
        thrustInput = 0f;
        UpdateLinearDrag();
        UpdateAnimator();
    }

    private void UpdateManualThrottle(float verticalInput)
    {
        float accelerateInput = Mathf.Max(verticalInput, 0f);
        float decelerateInput = Mathf.Max(-verticalInput, 0f);
        UpdateManualThrottle(accelerateInput, decelerateInput);
    }

    private void UpdateManualThrottle(float accelerateInput, float decelerateInput)
    {
        if (!usePersistentThrottle)
        {
            thrustInput = accelerateInput > 0f ? accelerateInput : -decelerateInput;
            engineThrottle = Mathf.Max(thrustInput, 0f);
            return;
        }

        if (Input.GetKeyDown(cutEngineKey))
        {
            CutEngine();
            return;
        }

        if (accelerateInput > 0f)
        {
            engineThrottle += throttleIncreasePerSecond * accelerateInput * Time.deltaTime;
        }

        if (decelerateInput > 0f)
        {
            engineThrottle -= throttleDecreasePerSecond * decelerateInput * Time.deltaTime;
        }

        engineThrottle = Mathf.Clamp(engineThrottle, -Mathf.Abs(maxReverseThrottle), 1f);
        thrustInput = engineThrottle;
    }

    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrWhiteSpace(engineOnParameter))
        {
            animator.SetBool(engineOnParameter, thrustInput > 0f);
        }
    }

    private void UpdateLinearDrag()
    {
        if (rb == null)
        {
            return;
        }

        bool engineIsOn = Mathf.Abs(thrustInput) > EngineOnThreshold;
        rb.linearDamping = engineIsOn ? linearDrag : engineCutLinearDrag;
    }
}
