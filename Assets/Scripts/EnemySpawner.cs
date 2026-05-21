using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject spreadEnemyPrefab;

    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;
    [SerializeField] private float spawnY = 6f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject prefabToSpawn = Random.value < 0.75f ? basicEnemyPrefab : spreadEnemyPrefab;
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
}