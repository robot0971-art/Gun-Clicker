using DI;
using UnityEngine;

/// <summary>
/// 경험치/레벨업 시스템: 몬스터 처치 시 경험치 지급, 레벨업 체크
/// </summary>
public class ExperienceSystem
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    
    private bool initialized = false;
    
    public void Initialize()
    {
        if (initialized) return;
        
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        
        // 이벤트 구독
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);
        
        initialized = true;
        Debug.Log("[ExperienceSystem] Initialized");
    }
    
    public void Dispose()
    {
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);
    }
    
    /// <summary>
    /// 몬스터 처치 시 경험치 지급
    /// </summary>
    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        // 현재 장착 총에 경험치 추가
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        int previousLevel = gunExp.Level;
        
        gunExp.AddExp(e.ExpReward);
        
        Debug.Log($"[ExperienceSystem] Gun {gameData.CurrentGunIndex} gained {e.ExpReward} EXP (Lv.{previousLevel} -> Lv.{gunExp.Level})");
        
        // 레벨업 체크
        if (gunExp.Level > previousLevel)
        {
            OnLevelUp(gameData.CurrentGunIndex, gunExp.Level);
        }
    }
    
    /// <summary>
    /// 레벨업 처리
    /// </summary>
    private void OnLevelUp(int gunIndex, int newLevel)
    {
        var gunData = gameDataAsset.guns[gunIndex];
        
        Debug.Log($"[ExperienceSystem] Gun {gunIndex} leveled up to {newLevel}!");
        
        // 레벨업 이벤트 발행 (UI 이펙트용)
        EventBus<GunLevelUpEvent>.Publish(new GunLevelUpEvent 
        { 
            GunId = gunIndex,
            NewLevel = newLevel
        });
        
        // 진화 조건 체크 (EvolutionSystem에 위임)
        EventBus<CheckEvolutionEvent>.Publish(new CheckEvolutionEvent 
        { 
            GunId = gunIndex,
            CurrentLevel = newLevel
        });
    }
    
    public GunExperienceData GetGunExperience(int gunIndex)
    {
        if (gunIndex < 0 || gunIndex >= gameData.GunExperiences.Length)
            return null;
        return gameData.GunExperiences[gunIndex];
    }
}
