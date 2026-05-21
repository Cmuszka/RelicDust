using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Bounds")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 movement;
    [SerializeField] private float shieldMoveMultiplier = 0.5f;

    private PlayerShieldAbility shield;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shield = GetComponent<PlayerShieldAbility>();
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        movement = input.normalized;
    }

    private void FixedUpdate()
    {
        float currentSpeed = moveSpeed;

        if (shield != null && shield.IsShieldActive)
        {
            currentSpeed *= shieldMoveMultiplier;
        }

        rb.linearVelocity = movement * currentSpeed;


        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);

        rb.position = clampedPosition;
    }
}