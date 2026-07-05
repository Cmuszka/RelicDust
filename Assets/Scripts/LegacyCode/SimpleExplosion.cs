using UnityEngine;

public class SimpleExplosion : MonoBehaviour
{
    [SerializeField] private float growSpeed = 5f;
    [SerializeField] private float lifetime = 0.3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
    }
}