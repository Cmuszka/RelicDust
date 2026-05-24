using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public Vector2 AimDirection { get; private set; }

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 direction = (mouseWorldPosition - transform.position);
        AimDirection = direction.normalized;
    }
}