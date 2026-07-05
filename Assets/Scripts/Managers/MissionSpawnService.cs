using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MissionSpawnGroup
{
    public GameObject enemyPrefab;
    public int count = 1;
    public Transform[] spawnPoints;
    public float spawnRadius = 3f;
    public float delayBetweenSpawns = 0.25f;
}

[Serializable]
public class MissionWave
{
    public string waveName;
    public MissionSpawnGroup[] groups;
}

public class MissionSpawnService : MonoBehaviour
{
    public event Action<Ship> EnemySpawned;
    public event Action EnemyDestroyed;

    public int EnemiesDestroyed { get; private set; }

    private readonly HashSet<Ship> registeredEnemies = new HashSet<Ship>();

    public void ResetTracking()
    {
        EnemiesDestroyed = 0;
        registeredEnemies.Clear();
    }

    public Coroutine SpawnWave(MissionWave wave)
    {
        if (!isActiveAndEnabled || wave == null)
        {
            return null;
        }

        return StartCoroutine(SpawnWaveRoutine(wave));
    }

    public void StopAllWaves()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnWaveRoutine(MissionWave wave)
    {
        if (wave.groups == null)
        {
            yield break;
        }

        foreach (MissionSpawnGroup group in wave.groups)
        {
            if (group == null || group.enemyPrefab == null)
            {
                continue;
            }

            for (int i = 0; i < group.count; i++)
            {
                GameObject enemyObject = Instantiate(
                    group.enemyPrefab,
                    GetSpawnPosition(group),
                    Quaternion.identity);

                RegisterEnemy(enemyObject.GetComponent<Ship>());

                if (group.delayBetweenSpawns > 0f)
                {
                    yield return new WaitForSeconds(group.delayBetweenSpawns);
                }
            }
        }
    }

    public void RegisterEnemy(Ship enemyShip)
    {
        if (enemyShip == null || registeredEnemies.Contains(enemyShip))
        {
            return;
        }

        registeredEnemies.Add(enemyShip);
        EnemySpawned?.Invoke(enemyShip);

        if (enemyShip.health != null)
        {
            enemyShip.health.OnDeath.AddListener(HandleEnemyDestroyed);
        }
    }

    private Vector3 GetSpawnPosition(MissionSpawnGroup group)
    {
        Transform spawnPoint = null;
        if (group.spawnPoints != null && group.spawnPoints.Length > 0)
        {
            spawnPoint = group.spawnPoints[UnityEngine.Random.Range(0, group.spawnPoints.Length)];
        }

        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * Mathf.Max(group.spawnRadius, 0f);
        return center + (Vector3)offset;
    }

    private void HandleEnemyDestroyed()
    {
        EnemiesDestroyed++;
        EnemyDestroyed?.Invoke();
    }
}
