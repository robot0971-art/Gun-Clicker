using DI;
using UnityEngine;
using System;

/// <summary>
/// 전투 로직 관리: 공격 계산, 몬스터 데미지, 크리티컬 처리
/// </summary>
public class CombatManager
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    private System.Random random;
    
    private bool initialized = false;
    
    public void Initialize()
    {
        if (initialized) return;
        
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        random = new System.Random();
        
        // 이벤트 구독
        EventBus<AttackEvent>.Subscribe(OnAttack);
        
        // 초기 몬스터 스폰
        SpawnMonster();
        
        initialized = true;
        Debug.Log("[CombatManager] Initialized");
    }
    
    public void Dispose()
    {
        EventBus<AttackEvent>.Unsubscribe(OnAttack);
    }
    
    /// <summary>
    /// 몬스터 스폰
    /// </summary>
    private void SpawnMonster()
    {
        if (gameDataAsset.monsters == null || gameDataAsset.monsters.Count == 0)
        {
            Debug.LogError("[CombatManager] No monster data available");
            return;
        }
        
        // 현재 스테이지에 맞는 몬스터 선택 (순환)
        int monsterIndex = (gameData.CurrentStage - 1) % gameDataAsset.monsters.Count;
        var monsterData = gameDataAsset.monsters[monsterIndex];
        
        // 스테이지에 따른 HP 스케일링
        int scaledHP = Mathf.RoundToInt(monsterData.BaseHP * Mathf.Pow(monsterData.HpScaling, gameData.CurrentStage - 1));
        
        gameData.CurrentMonsterId = monsterData.Id;
        gameData.CurrentMonsterHP = scaledHP;
        
        EventBus<MonsterSpawnedEvent>.Publish(new MonsterSpawnedEvent 
        { 
            MonsterId = monsterData.Id,
            MaxHP = scaledHP,
            MonsterName = monsterData.Name
        });
        
        Debug.Log($"[CombatManager] Monster spawned: {monsterData.Name} (HP: {scaledHP})");
    }
    
    /// <summary>
    /// 공격 처리
    /// </summary>
    private void OnAttack(AttackEvent e)
    {
        // 현재 장착 총 정보 가져오기
        var gunData = gameDataAsset.guns[gameData.CurrentGunIndex];
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        
        // 데미지 계산 (총 기본 데미지 + 레벨 보너스)
        int baseDamage = CalculateDamage(gunData, gunExp);
        
        // 크리티컬 체크
        bool isCritical = random.NextDouble() < gunData.CriticalChance;
        int finalDamage = isCritical ? Mathf.RoundToInt(baseDamage * gunData.CriticalMultiplier) : baseDamage;
        
        // 몬스터 HP 감소
        gameData.CurrentMonsterHP = Mathf.Max(0, gameData.CurrentMonsterHP - finalDamage);
        
        // 이벤트 발행
        EventBus<MonsterHitEvent>.Publish(new MonsterHitEvent 
        { 
            MonsterId = gameData.CurrentMonsterId,
            Damage = finalDamage,
            CurrentHP = gameData.CurrentMonsterHP,
            IsCritical = isCritical
        });
        
        if (isCritical)
        {
            EventBus<CriticalHitEvent>.Publish(new CriticalHitEvent 
            { 
                Damage = finalDamage,
                MonsterId = gameData.CurrentMonsterId
            });
        }
        
        // 몬스터 사망 체크
        if (gameData.CurrentMonsterHP <= 0)
        {
            OnMonsterKilled();
        }
    }
    
    /// <summary>
    /// 데미지 계산 (총 기본 데미지 + 레벨 보너스 + 업그레이드 보너스)
    /// </summary>
    private int CalculateDamage(GunData gun, GunExperienceData exp)
    {
        // 기본 데미지
        float damage = gun.BaseDamage;
        
        // 레벨 보너스 (레벨당 10% 증가)
        damage *= (1 + (exp.Level - 1) * 0.1f);
        
        // 업그레이드 보너스 (기존 업그레이드 시스템 연동)
        var upgradeData = gameDataAsset.upgrades[gameData.CurrentGunIndex];
        var upgradeLevel = gameData.UpgradeLevels[gameData.CurrentGunIndex];
        damage *= Mathf.Pow(upgradeData.ValueMultiplier, upgradeLevel);
        
        return Mathf.RoundToInt(damage);
    }
    
    /// <summary>
    /// 몬스터 처치 처리
    /// </summary>
    private void OnMonsterKilled()
    {
        var monsterData = gameDataAsset.monsters.Find(m => m.Id == gameData.CurrentMonsterId);
        if (monsterData == null) return;
        
        // 보상 계산 (스테이지에 따른 스케일링)
        int expReward = Mathf.RoundToInt(monsterData.ExpReward * (1 + gameData.CurrentStage * 0.1f));
        int goldReward = Mathf.RoundToInt(monsterData.GoldReward * (1 + gameData.CurrentStage * 0.1f));
        
        // 골드 추가
        gameData.TotalGold += goldReward;
        EventBus<MoneyChangedEvent>.Publish(new MoneyChangedEvent 
        { 
            Amount = gameData.TotalGold,
            Delta = goldReward
        });
        
        // 이벤트 발행
        EventBus<MonsterKilledEvent>.Publish(new MonsterKilledEvent 
        { 
            MonsterId = gameData.CurrentMonsterId,
            ExpReward = expReward,
            GoldReward = goldReward
        });
        
        Debug.Log($"[CombatManager] Monster killed! EXP: {expReward}, Gold: {goldReward}");
        
        // 다음 스테이지
        gameData.CurrentStage++;
        
        // 새 몬스터 스폰
        SpawnMonster();
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
