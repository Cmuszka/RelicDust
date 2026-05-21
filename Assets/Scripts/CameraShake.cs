using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    private float timer;
    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Shake()
    {
        timer = shakeDuration;
    }

    private void Update()
    {
        if (timer > 0)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            timer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
}