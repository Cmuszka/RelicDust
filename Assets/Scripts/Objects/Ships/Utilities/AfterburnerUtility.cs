using UnityEngine;

public class AfterburnerUtility : UtilitySystem
{
    [SerializeField] private float boostImpulse = 10f;
    [SerializeField] private bool clearExistingVelocity;

    [Header("Visual")]
    [SerializeField] private GameObject afterburnerVisual;
    [SerializeField] private float visualDuration = 0.35f;

    private float visualTimer;

    protected override void Awake()
    {
        base.Awake();

        if (afterburnerVisual != null)
        {
            afterburnerVisual.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (visualTimer <= 0f)
        {
            return;
        }

        visualTimer -= Time.deltaTime;
        if (visualTimer <= 0f)
        {
            SetAfterburnerVisual(false);
        }
    }

    public override void Activate()
    {
        if (!CanActivate())
        {
            return;
        }

        base.Activate();

        Rigidbody2D rb = ownerShip != null ? ownerShip.GetComponent<Rigidbody2D>() : GetComponentInParent<Rigidbody2D>();
        if (rb == null)
        {
            return;
        }

        if (clearExistingVelocity)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Vector2 boostDirection = ownerShip != null ? ownerShip.transform.up : transform.up;
        rb.AddForce(boostDirection * boostImpulse, ForceMode2D.Impulse);

        visualTimer = visualDuration;
        SetAfterburnerVisual(true);
    }

    private void OnDisable()
    {
        SetAfterburnerVisual(false);
        visualTimer = 0f;
    }

    private void SetAfterburnerVisual(bool isActive)
    {
        if (afterburnerVisual != null)
        {
            afterburnerVisual.SetActive(isActive);
        }
    }
}
