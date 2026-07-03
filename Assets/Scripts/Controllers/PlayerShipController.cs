using UnityEngine;

[RequireComponent(typeof(Ship))]
public class PlayerShipController : MonoBehaviour
{
    [SerializeField] private int selectedWeaponIndex;
    [SerializeField] private int primaryUtilityIndex;
    [SerializeField] private int secondaryUtilityIndex = 1;

    [SerializeField] private KeyCode primaryUtilityKey = KeyCode.Space;
    [SerializeField] private KeyCode secondaryUtilityKey = KeyCode.Q;
    [SerializeField] private KeyCode cycleTargetKey = KeyCode.Tab;
    [SerializeField] private KeyCode cycleMissileTargetKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode selectTargetUnderCursorKey = KeyCode.Mouse2;
    [SerializeField] private KeyCode clearTargetKey = KeyCode.Escape;
    [SerializeField] private bool numberKeysFireWeapons = true;

    [Header("Movement")]
    [SerializeField] private bool rotateTowardCursor;
    [SerializeField] private KeyCode toggleCursorRotationKey = KeyCode.C;
    [SerializeField] private float cursorRotationDeadZone = 2f;
    [SerializeField] private float cursorRotationFullInputAngle = 45f;

    private Ship ship;
    private PlayerAim playerAim;
    private ShipMovement movement;
    private Camera mainCamera;
    private bool firedWeaponWithNumberKeyThisFrame;

    public int SelectedWeaponIndex => selectedWeaponIndex;
    public WeaponSystem SelectedWeapon
    {
        get
        {
            if (ship == null || ship.weapons == null || selectedWeaponIndex < 0 || selectedWeaponIndex >= ship.weapons.Length)
            {
                return null;
            }

            return ship.weapons[selectedWeaponIndex];
        }
    }

    private void Awake()
    {
        ship = GetComponent<Ship>();
        playerAim = GetComponent<PlayerAim>();
        movement = GetComponent<ShipMovement>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        ship.RefreshSystems();
        ApplyMovementMode();
    }

    private void Update()
    {
        firedWeaponWithNumberKeyThisFrame = false;

        UpdateMovement();
        UpdateTargeting();
        UpdateWeaponSelection();

        if (Input.GetMouseButton(0) && !firedWeaponWithNumberKeyThisFrame)
        {
            FireSelectedWeapon();
        }

        if (Input.GetKeyDown(primaryUtilityKey))
        {
            ActivateUtility(primaryUtilityIndex);
        }

        if (Input.GetKeyDown(secondaryUtilityKey))
        {
            ActivateUtility(secondaryUtilityIndex);
        }
    }

    private void UpdateMovement()
    {
        if (movement == null)
        {
            return;
        }

        if (Input.GetKeyDown(toggleCursorRotationKey))
        {
            rotateTowardCursor = !rotateTowardCursor;
            ApplyMovementMode();
        }

        if (!rotateTowardCursor)
        {
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");
        float accelerateInput = Mathf.Max(verticalInput, 0f);
        float decelerateInput = Mathf.Max(-verticalInput, 0f);
        movement.SetManualMovementInput(accelerateInput, decelerateInput, GetCursorRotationInput());
    }

    private void UpdateWeaponSelection()
    {
        if (ship.weapons == null || ship.weapons.Length == 0)
        {
            ship.RefreshSystems();
        }

        int weaponCount = ship.weapons != null ? Mathf.Min(ship.weapons.Length, 9) : 0;
        for (int i = 0; i < weaponCount; i++)
        {
            KeyCode numberKey = (KeyCode)((int)KeyCode.Alpha1 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad1 + i);

            if (Input.GetKeyDown(numberKey) || Input.GetKeyDown(keypadKey))
            {
                selectedWeaponIndex = i;
            }

            if (numberKeysFireWeapons && (Input.GetKey(numberKey) || Input.GetKey(keypadKey)))
            {
                FireWeapon(i);
                firedWeaponWithNumberKeyThisFrame = true;
            }
        }
    }

    private void UpdateTargeting()
    {
        if (ship.targeting == null)
        {
            return;
        }

        ship.targeting.SetAimDirection(GetAimDirection());

        if (Input.GetKeyDown(cycleTargetKey))
        {
            ship.targeting.CycleTarget();
        }

        if (Input.GetKeyDown(cycleMissileTargetKey))
        {
            ship.targeting.CycleMissileTarget();
        }

        if (Input.GetKeyDown(selectTargetUnderCursorKey))
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            ship.targeting.SelectTargetUnderCursor(mainCamera);
        }

        if (Input.GetKeyDown(clearTargetKey))
        {
            ship.targeting.ClearTarget();
        }
    }

    private void FireSelectedWeapon()
    {
        FireWeapon(selectedWeaponIndex);
    }

    private void FireWeapon(int weaponIndex)
    {
        if (ship.weapons == null || ship.weapons.Length == 0)
        {
            ship.RefreshSystems();
        }

        if (ship.weapons == null || weaponIndex < 0 || weaponIndex >= ship.weapons.Length)
        {
            return;
        }

        ship.weapons[weaponIndex].Fire(GetAimDirection());
    }

    private void ActivateUtility(int utilityIndex)
    {
        if (ship.utilities == null || ship.utilities.Length == 0)
        {
            ship.RefreshSystems();
        }

        if (ship.utilities == null || utilityIndex < 0 || utilityIndex >= ship.utilities.Length)
        {
            return;
        }

        UtilitySystem utility = ship.utilities[utilityIndex];

        if (utility.CanActivate())
        {
            utility.Activate();
        }
    }

    private Vector2 GetAimDirection()
    {
        if (playerAim != null && playerAim.AimDirection != Vector2.zero)
        {
            return playerAim.AimDirection;
        }

        return transform.up;
    }

    private void ApplyMovementMode()
    {
        if (movement != null)
        {
            movement.SetReadPlayerInput(!rotateTowardCursor);
        }
    }

    private float GetCursorRotationInput()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return 0f;
        }

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 directionToCursor = mouseWorldPosition - transform.position;
        if (directionToCursor == Vector2.zero)
        {
            return 0f;
        }

        float signedAngle = Vector2.SignedAngle(transform.up, directionToCursor.normalized);
        if (Mathf.Abs(signedAngle) <= cursorRotationDeadZone)
        {
            return 0f;
        }

        float fullInputAngle = Mathf.Max(cursorRotationFullInputAngle, cursorRotationDeadZone);
        return Mathf.Clamp(signedAngle / fullInputAngle, -1f, 1f);
    }
}
