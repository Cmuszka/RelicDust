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

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 270f;

    [Header("Drag")]
    [SerializeField] private float linearDrag = 0f;
    [SerializeField] private float angularDrag = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string engineOnParameter = "EngineOn";

    private Rigidbody2D rb;

    private float thrustInput;
    private float rotationInput;
    private float thrustMultiplier = 1f;
    private float maxSpeedMultiplier = 1f;

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

        thrustInput = Input.GetAxisRaw("Vertical");
        rotationInput = -Input.GetAxisRaw("Horizontal");
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
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

    public void StopMovementInput()
    {
        thrustInput = 0f;
        rotationInput = 0f;
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrWhiteSpace(engineOnParameter))
        {
            animator.SetBool(engineOnParameter, thrustInput > 0f);
        }
    }
}
