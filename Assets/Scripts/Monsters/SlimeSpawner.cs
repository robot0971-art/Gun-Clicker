using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private SlimePool slimePool;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int monsterId = 0;
    [SerializeField] private int monsterHP = 16;
    [SerializeField] private string monsterName = "Slime";

    [Header("Timing")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float spawnInterval = 1f;

    private void Start()
    {
        if (slimePool == null)
        {
            Debug.LogError("[SlimeSpawner] SlimePool is not assigned.");
            return;
        }

        HideSceneSlimes();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (slimePool == null)
        {
            Debug.LogError("[SlimeSpawner] SlimePool is not assigned.");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        var slime = slimePool.GetSlime(position, monsterId, monsterHP);

        if (slime == null)
        {
            Debug.LogError("[SlimeSpawner] Failed to get slime from pool.");
            return;
        }

        EventBus<MonsterSpawnedEvent>.Publish(new MonsterSpawnedEvent
        {
            MonsterId = monsterId,
            MaxHP = monsterHP,
            MonsterName = monsterName
        });

        Debug.Log($"[SlimeSpawner] Spawned {monsterName} at {position}");
    }

    private void HideSceneSlimes()
    {
        var existingSlimes = FindObjectsOfType<Slime>(true);
        foreach (var existingSlime in existingSlimes)
        {
            if (existingSlime != null && existingSlime.gameObject.scene.IsValid())
            {
                existingSlime.gameObject.SetActive(false);
            }
        }
    }
}