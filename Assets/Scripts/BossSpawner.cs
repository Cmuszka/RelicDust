using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float spawnDelay = 25f;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 7f, 0f);

    private bool hasSpawned;

    private void Update()
    {
        if (hasSpawned) return;

        spawnDelay -= Time.deltaTime;

        if (spawnDelay <= 0f)
        {
            Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            hasSpawned = true;
        }
    }
}