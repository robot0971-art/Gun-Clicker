using DI;
using UnityEngine;

public class CombatManager
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;

    private bool initialized = false;
    private bool monsterAlive = false;

    public void Initialize()
    {
        if (initialized) return;

        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();

        initialized = true;
        Debug.Log("[CombatManager] Initialized");
    }

    public void Dispose()
    {
    }

    public void SpawnMonster()
    {
        if (gameDataAsset.monsters == null || gameDataAsset.monsters.Count == 0)
        {
            Debug.LogError("[CombatManager] No monster data available");
            return;
        }

        int monsterIndex = (gameData.CurrentStage - 1) % gameDataAsset.monsters.Count;
        var monsterData = gameDataAsset.monsters[monsterIndex];

        int scaledHP = Mathf.RoundToInt(monsterData.BaseHP * Mathf.Pow(monsterData.HpScaling, gameData.CurrentStage - 1));
        if (monsterData.Name == "Slime")
        {
            scaledHP = 16;
        }

        gameData.CurrentMonsterId = monsterData.Id;
        gameData.CurrentMonsterHP = scaledHP;
        monsterAlive = true;

        EventBus<MonsterSpawnedEvent>.Publish(new MonsterSpawnedEvent
        {
            MonsterId = monsterData.Id,
            MaxHP = scaledHP,
            MonsterName = monsterData.Name
        });

        Debug.Log($"[CombatManager] Monster spawned: {monsterData.Name} (HP: {scaledHP})");
    }

    private void OnMonsterHit(MonsterHitEvent e)
    {
        Debug.Log($"[CombatManager] MonsterHit received monsterId={e.MonsterId} damage={e.Damage} currentHP={e.CurrentHP} crit={e.IsCritical}");

        if (e.MonsterId != -1)
        {
            Debug.Log("[CombatManager] Ignored MonsterHit because MonsterId was not -1");
            return;
        }

        if (!monsterAlive)
        {
            Debug.Log("[CombatManager] Ignored MonsterHit because monster is already dead");
            return;
        }

        var gunData = gameDataAsset.guns[gameData.CurrentGunIndex];
        int finalDamage = e.IsCritical
            ? Mathf.RoundToInt(e.Damage * gunData.CriticalMultiplier)
            : e.Damage;

        Debug.Log($"[CombatManager] Applying damage {finalDamage} to monster {gameData.CurrentMonsterId} (before={gameData.CurrentMonsterHP})");

        gameData.CurrentMonsterHP = Mathf.Max(0, gameData.CurrentMonsterHP - finalDamage);
        Debug.Log($"[CombatManager] Monster HP after damage: {gameData.CurrentMonsterHP}");

        EventBus<MonsterHitEvent>.Publish(new MonsterHitEvent
        {
            MonsterId = gameData.CurrentMonsterId,
            Damage = finalDamage,
            CurrentHP = gameData.CurrentMonsterHP,
            IsCritical = e.IsCritical
        });

        if (e.IsCritical)
        {
            EventBus<CriticalHitEvent>.Publish(new CriticalHitEvent
            {
                Damage = finalDamage,
                MonsterId = gameData.CurrentMonsterId
            });
        }

        if (gameData.CurrentMonsterHP <= 0)
        {
            OnMonsterKilled();
        }
    }

    private void OnMonsterKilled()
    {
        monsterAlive = false;
        gameData.CurrentMonsterHP = 0;

        var monsterData = gameDataAsset.monsters.Find(m => m.Id == gameData.CurrentMonsterId);
        if (monsterData == null) return;

        int expReward = Mathf.RoundToInt(monsterData.ExpReward * (1 + gameData.CurrentStage * 0.1f));

        EventBus<MonsterKilledEvent>.Publish(new MonsterKilledEvent
        {
            MonsterId = gameData.CurrentMonsterId,
            ExpReward = expReward
        });

        Debug.Log($"[CombatManager] Monster killed! EXP: {expReward}");

        gameData.CurrentStage++;
    }

    public int GetCurrentMonsterHP()
    {
        return gameData.CurrentMonsterHP;
    }

    public int GetCurrentMonsterMaxHP()
    {
        var monsterData = gameDataAsset.monsters.Find(m => m.Id == gameData.CurrentMonsterId);
        if (monsterData == null) return 0;
        return Mathf.RoundToInt(monsterData.BaseHP * Mathf.Pow(monsterData.HpScaling, gameData.CurrentStage - 1));
    }
}
