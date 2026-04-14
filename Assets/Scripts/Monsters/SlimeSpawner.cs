using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnOption
{
    public string monsterName = "Slime";
    public int monsterId = 0;
    public int monsterHP = 16;
    public SlimePool pool;
}

public class SlimeSpawner : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private SlimePool slimePool;
    [SerializeField] private List<MonsterSpawnOption> monsterPools = new List<MonsterSpawnOption>();

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
        if (!HasValidPoolConfiguration())
        {
            Debug.LogError("[SlimeSpawner] No valid monster pool is assigned.");
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
        var spawnOption = GetRandomSpawnOption();
        if (spawnOption == null || spawnOption.pool == null)
        {
            Debug.LogError("[SlimeSpawner] Failed to find a valid monster pool.");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        var slime = spawnOption.pool.GetSlime(position, spawnOption.monsterId, spawnOption.monsterHP);

        if (slime == null)
        {
            Debug.LogError("[SlimeSpawner] Failed to get monster from pool.");
            return;
        }

        EventBus<MonsterSpawnedEvent>.Publish(new MonsterSpawnedEvent
        {
            MonsterId = spawnOption.monsterId,
            MaxHP = spawnOption.monsterHP,
            MonsterName = spawnOption.monsterName
        });

        Debug.Log($"[SlimeSpawner] Spawned {spawnOption.monsterName} at {position}");
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

    private bool HasValidPoolConfiguration()
    {
        if (GetValidSpawnOptions().Count > 0)
        {
            return true;
        }

        return slimePool != null;
    }

    private MonsterSpawnOption GetRandomSpawnOption()
    {
        var validOptions = GetValidSpawnOptions();
        if (validOptions.Count == 0)
        {
            if (slimePool == null)
            {
                return null;
            }

            return new MonsterSpawnOption
            {
                pool = slimePool,
                monsterId = monsterId,
                monsterHP = monsterHP,
                monsterName = monsterName
            };
        }

        int index = Random.Range(0, validOptions.Count);
        return validOptions[index];
    }

    private List<MonsterSpawnOption> GetValidSpawnOptions()
    {
        var validOptions = new List<MonsterSpawnOption>();

        if (monsterPools == null)
        {
            return validOptions;
        }

        foreach (var option in monsterPools)
        {
            if (option != null && option.pool != null)
            {
                validOptions.Add(option);
            }
        }

        return validOptions;
    }
}
