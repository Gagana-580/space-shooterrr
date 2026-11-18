
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Existing Enemy (Optional)")]
    public GameObject mainEnemy;

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public int enemiesToSpawn = 3;
    public float initialSpawnDelay = 3f;
    public float spawnDelay = 5f;

    [Header("Enemy Size")]
    [Range(0.1f, 2f)]
    public float enemyScale = 0.3f;

    [Header("Random Spawn Area")]
    public Vector2 minPosition = new Vector2(-2f, 0.5f);
    public Vector2 maxPosition = new Vector2(2f, 2.5f);

    [Header("Overlap Protection")]
    public float minimumGap = 0.2f;
    public int maxAttempts = 100;

    [Header("Player Safety")]
    public Transform player;
    public float minimumPlayerDistance = 1.5f;

    public bool IsSpawningFinished { get; private set; } = true;

    private readonly List<GameObject> spawnedEnemies =
        new List<GameObject>();

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (spawnOnStart)
        {
            BeginSpawning();
        }
    }

    [ContextMenu("Start Spawning")]
    public void BeginSpawning()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode first.");
            return;
        }

        if (spawnRoutine != null)
        {
            Debug.LogWarning("Spawner is already running.");
            return;
        }

        IsSpawningFinished = false;
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    [ContextMenu("Stop Spawning")]
    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        IsSpawningFinished = true;
        Debug.Log("Enemy spawning stopped.");
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, initialSpawnDelay)
        );

        int spawnedCount = 0;
        int targetCount = Mathf.Max(0, enemiesToSpawn);

        for (int i = 0; i < targetCount; i++)
        {
            if (!TrySpawnEnemy())
            {
                Debug.LogWarning(
                    "No safe spawn position found."
                );
                break;
            }

            spawnedCount++;

            if (i < targetCount - 1)
            {
                yield return new WaitForSeconds(
                    Mathf.Max(0.1f, spawnDelay)
                );
            }
        }

        IsSpawningFinished = true;
        spawnRoutine = null;

        Debug.Log(
            "Spawning finished: " +
            spawnedCount + " enemies created."
        );
    }

    private bool TrySpawnEnemy()
    {
        List<GameObject> validPrefabs =
            new List<GameObject>();

        if (enemyPrefabs != null)
        {
            foreach (GameObject prefab in enemyPrefabs)
            {
                if (prefab != null)
                {
                    validPrefabs.Add(prefab);
                }
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("Assign enemy prefabs!");
            return false;
        }

        GameObject selectedPrefab =
            validPrefabs[Random.Range(0, validPrefabs.Count)];

        GameObject newEnemy = Instantiate(
            selectedPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        newEnemy.SetActive(false);
        newEnemy.transform.localScale *= enemyScale;

        SpriteRenderer newRenderer =
            newEnemy.GetComponentInChildren<SpriteRenderer>(true);

        if (newRenderer == null)
        {
            Debug.LogWarning("Enemy has no SpriteRenderer!");
            Destroy(newEnemy);
            return false;
        }

        spawnedEnemies.RemoveAll(enemy => enemy == null);

        for (int attempt = 0;
             attempt < Mathf.Max(1, maxAttempts);
             attempt++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minPosition.x, maxPosition.x),
                Random.Range(minPosition.y, maxPosition.y),
                0f
            );

            newEnemy.transform.position = randomPosition;

            // Center the sprite on the chosen position.
            newEnemy.transform.position +=
                randomPosition - newRenderer.bounds.center;

            Bounds newBounds = newRenderer.bounds;
            newBounds.Expand(Mathf.Max(0f, minimumGap));

            bool overlaps = false;

            // Keep enemies away from the player.
            if (player != null)
            {
                float distance = Vector2.Distance(
                    newRenderer.bounds.center,
                    player.position
                );

                if (distance < minimumPlayerDistance)
                {
                    continue;
                }
            }

            // Avoid overlapping the original main enemy.
            if (mainEnemy != null &&
                mainEnemy.activeInHierarchy)
            {
                SpriteRenderer mainRenderer =
                    mainEnemy.GetComponentInChildren<SpriteRenderer>();

                if (mainRenderer != null &&
                    newBounds.Intersects(mainRenderer.bounds))
                {
                    overlaps = true;
                }
            }

            // Avoid previously spawned enemies.
            foreach (GameObject existing in spawnedEnemies)
            {
                if (existing == null ||
                    !existing.activeInHierarchy)
                {
                    continue;
                }

                SpriteRenderer existingRenderer =
                    existing.GetComponentInChildren<SpriteRenderer>();

                if (existingRenderer != null &&
                    newBounds.Intersects(existingRenderer.bounds))
                {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
            {
                continue;
            }

            newEnemy.SetActive(true);
            spawnedEnemies.Add(newEnemy);

            Debug.Log(
                "Enemy spawned safely at " +
                newRenderer.bounds.center
            );

            return true;
        }

        Destroy(newEnemy);
        Debug.LogWarning(
            "Could not find a safe spawn location."
        );

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center =
            (minPosition + maxPosition) / 2f;

        Vector2 size =
            maxPosition - minPosition;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            new Vector3(center.x, center.y, 0f),
            new Vector3(size.x, size.y, 0f)
        );
    }
}
