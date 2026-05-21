using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float destroyY = -6f;

    private void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}