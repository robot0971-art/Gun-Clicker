using DI;
using UnityEngine;

/// <summary>
/// 게임 상태 관리, 이벤트 처리
/// </summary>
public class GameManager
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    private SaveManager saveManager;
    
    private bool initialized = false;
    
    public void Initialize()
    {
        if (initialized) return;
        
        // DI 주입
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        saveManager = DIContainer.Resolve<SaveManager>();
        
        // 저장 데이터 로드
        if (saveManager.HasSaveData())
        {
            var savedData = saveManager.Load();
            gameData.TotalGold = savedData.TotalGold;
            gameData.CurrentGunIndex = savedData.CurrentGunIndex;
            gameData.ClickCounts = savedData.ClickCounts;
            gameData.UpgradeLevels = savedData.UpgradeLevels;
        }
        
        // 이벤트 구독 (Combat/Exp 시스템에서 처리하므로 일부 제거)
        EventBus<GunSwitchedEvent>.Subscribe(OnGunSwitched);
        EventBus<UpgradePurchasedEvent>.Subscribe(OnUpgradePurchased);
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled); // 골드 보상용
        
        initialized = true;
        
        EventBus<GameInitializedEvent>.Publish(new GameInitializedEvent());
        
        Debug.Log("[GameManager] Initialized (Integrated with Combat/Exp/Evolution)");
    }
    
    public void Dispose()
    {
        EventBus<GunSwitchedEvent>.Unsubscribe(OnGunSwitched);
        EventBus<UpgradePurchasedEvent>.Unsubscribe(OnUpgradePurchased);
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);
        
        saveManager.Save(gameData);
    }
    
    private void OnGunSwitched(GunSwitchedEvent e)
    {
        if (e.GunId < 0 || e.GunId >= gameDataAsset.guns.Count) return;
        
        gameData.CurrentGunIndex = e.GunId;
        saveManager.Save(gameData);
    }
    
    private void OnUpgradePurchased(UpgradePurchasedEvent e)
    {
        gameData.UpgradeLevels[e.GunId]++;
        saveManager.Save(gameData);
    }
    
    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        // CombatManager에서 이미 골드 추가했으므로 여기선 저장만
        saveManager.Save(gameData);
        
        // 클릭 카운트 증가 (해금 체크용)
        gameData.ClickCounts[gameData.CurrentGunIndex]++;
        
        // 해금 체크 (기존 로직 유지)
        CheckUnlocks();
    }
    
    private void CheckUnlocks()
    {
        for (int i = 1; i < gameDataAsset.guns.Count; i++)
        {
            if (CanUnlockGun(i))
            {
                // 이미 해금된 총은 스킵
                if (gameData.ClickCounts[i] > 0) continue;
                
                gameData.ClickCounts[i] = 1; // 해금 표시
                
                EventBus<GunUnlockedEvent>.Publish(new GunUnlockedEvent { GunId = i });
                
                Debug.Log($"[GameManager] Gun unlocked: {gameDataAsset.guns[i].Name}");
            }
        }
    }
    
    public bool CanUnlockGun(int gunId)
    {
        if (gunId <= 0) return true; // 첫 번째 총은 기본 해금
        
        var gun = gameDataAsset.guns[gunId];
        var currentGunClicks = gameData.ClickCounts[gameData.CurrentGunIndex];
        
        return currentGunClicks >= gun.UnlockClicks;
    }
    
    public bool CanPurchaseUpgrade(int gunId)
    {
        if (gunId < 0 || gunId >= gameDataAsset.upgrades.Count) return false;
        
        var upgrade = gameDataAsset.upgrades[gunId];
        var currentLevel = gameData.UpgradeLevels[gunId];
        
        if (currentLevel >= upgrade.MaxLevel) return false;
        
        int cost = CalculateUpgradeCost(gunId);
        return gameData.TotalGold >= cost;
    }
    
    // 데미지 계산은 CombatManager로 이동
    [System.Obsolete("Use CombatManager.CalculateDamage instead")]
    public int CalculateClickValue(int gunId)
    {
        var gun = gameDataAsset.guns[gunId];
        var upgrade = gameDataAsset.upgrades[gunId];
        var level = gameData.UpgradeLevels[gunId];
        
        int baseValue = gun.ClickValue;
        float multiplier = Mathf.Pow(upgrade.ValueMultiplier, level);
        
        return Mathf.RoundToInt(baseValue * multiplier);
    }
    
    public int CalculateUpgradeCost(int gunId)
    {
        var upgrade = gameDataAsset.upgrades[gunId];
        var level = gameData.UpgradeLevels[gunId];
        
        return Mathf.RoundToInt(upgrade.BaseCost * Mathf.Pow(upgrade.CostMultiplier, level));
    }
    
    public int GetCurrentClickCount()
    {
        return gameData.ClickCounts[gameData.CurrentGunIndex];
    }
    
    public long GetTotalGold()
    {
        return gameData.TotalGold;
    }
    
    public int GetCurrentGunIndex()
    {
        return gameData.CurrentGunIndex;
    }
    
    public int GetUpgradeLevel(int gunId)
    {
        return gameData.UpgradeLevels[gunId];
    }
    
    public bool IsGunUnlocked(int gunId)
    {
        return gunId == 0 || gameData.ClickCounts[gunId] > 0;
    }
}