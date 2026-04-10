# Gun Evolution System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 클리커에서 RPG 전투/성장/진화 시스템으로 전환 - 클릭→발사→몬스터 처치→경험치→레벨업→총 진화

**Architecture:** 기존 EventBus + DI Container 활용. CombatManager(전투), ExperienceSystem(경험치), EvolutionSystem(진화)를 새로 추가하고 기존 GameManager와 연동. Monster는 ScriptableObject로 데이터 관리.

**Tech Stack:** Unity C#, 기존 EventBus/DI 시스템 재사용

---

## File Structure

### New Files
- `Assets/Scripts/Data/MonsterData.cs` - 몬스터 데이터 구조
- `Assets/Scripts/Core/CombatManager.cs` - 전투 로직 (데미지, 크리티컬)
- `Assets/Scripts/Core/ExperienceSystem.cs` - 경험치/레벨업 관리
- `Assets/Scripts/Core/EvolutionSystem.cs` - 진화 조건 및 실행
- `Assets/Scripts/Data/GunExperienceData.cs` - 총별 경험치 데이터 (런타임)
- `Assets/Scripts/Events/CombatEvents.cs` - 전투 관련 이벤트 정의

### Modified Files
- `Assets/Scripts/GameDataAsset.cs` - GunData에 전투/진화 필드 추가
- `Assets/Scripts/Core/GameData.cs` - 총별 레벨/경험치 저장 추가
- `Assets/Scripts/Core/GameManager.cs` - 전투/경험치/진화 시스템 연동
- `Assets/Scripts/UI/ClickHandler.cs` - 클릭 시 발사/전투 로직 연결
- `Assets/Scripts/Events/Events.cs` - 새 이벤트 추가

---

## Task 1: Combat Events 정의

**Files:**
- Create: `Assets/Scripts/Events/CombatEvents.cs`

**Context:** 기존 EventBus<T> 패턴 사용. struct 기반 이벤트.

- [ ] **Step 1: 전투 관련 이벤트 struct 작성**

```csharp
using System;

// 공격 이벤트 (클릭 시 발생)
public struct AttackEvent 
{ 
    public int GunId;
    public int Damage;
    public bool IsCritical;
}

// 몬스터 피격 이벤트
public struct MonsterHitEvent 
{ 
    public int MonsterId;
    public int Damage;
    public int CurrentHP;
    public bool IsCritical;
}

// 몬스터 사망 이벤트
public struct MonsterKilledEvent 
{ 
    public int MonsterId;
    public int ExpReward;
    public int GoldReward;
}

// 새 몬스터 스폰 이벤트
public struct MonsterSpawnedEvent 
{ 
    public int MonsterId;
    public int MaxHP;
    public string MonsterName;
}

// 크리티컬 발생 이벤트 (UI 이펙트용)
public struct CriticalHitEvent 
{ 
    public int Damage;
    public int MonsterId;
}
```

- [ ] **Step 2: 파일 저장 및 확인**

Unity Editor에서 에러 없는지 확인

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Events/CombatEvents.cs
git commit -m "feat: add combat events"
```

---

## Task 2: MonsterData ScriptableObject 데이터 구조

**Files:**
- Create: `Assets/Scripts/Data/MonsterData.cs`

**Context:** 기존 GunData/UpgradeData와 동일한 방식. ExcelConverter와 연동.

- [ ] **Step 1: MonsterData 클래스 작성**

```csharp
using System;
using UnityEngine;

[Serializable]
public class MonsterData
{
    public int Id;
    public string Name;
    public int BaseHP;
    public int BaseDefense;        // 방어력 (데미지 감소)
    public int ExpReward;          // 처치 시 경험치
    public int GoldReward;         // 처치 시 골드
    public float HpScaling;        // 스테이지별 HP 증가율
    public string SpriteName;
}
```

- [ ] **Step 2: GameDataAsset에 Monster 목록 추가 (나중에 Task 4에서 완료)**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Data/MonsterData.cs
git commit -m "feat: add MonsterData structure"
```

---

## Task 3: GunData 전투/진화 필드 확장

**Files:**
- Modify: `Assets/Scripts/GameDataAsset.cs`

**Context:** 기존 GunData에 전투 관련 필드 추가. 기존 데이터와 호환성 유지.

- [ ] **Step 1: GunData에 전투/진화 필드 추가**

```csharp
[Serializable]
public class GunData
{
    public int Id;
    public string Name;
    
    // 기존 필드 (하위 호환성 유지)
    public int ClickValue;         // [Deprecated] 기존 클릭 가치
    public int UnlockClicks;       // 해금에 필요한 클릭 수
    public string SpriteName;
    
    // 새로운 전투 필드
    public int BaseDamage;         // 기본 공격력
    public float AttackSpeed;      // 연사력 (쿨타임 감소)
    public float CriticalChance;   // 크리티컬 확률 (0.0 ~ 1.0)
    public float CriticalMultiplier; // 크리티컬 배율 (예: 2.0 = 2배)
    
    // 새로운 진화 필드
    public int EvolveLevel;        // 진화에 필요한 레벨
    public int NextGunId;          // 진화 후 변경될 총 ID (-1이면 최종)
    public bool IsFinalForm;       // 최종 형태 여부
}
```

- [ ] **Step 2: UpgradeData는 그대로 유지 (업그레이드 시스템 별도로 존재)**

- [ ] **Step 3: GameDataAsset에 MonsterData 목록 추가 필드 (Excel 연동용)**

```csharp
[CreateAssetMenu(fileName = "GameDataAsset", menuName = "Game/DataAsset")]
public class GameDataAsset : ScriptableObject
{
    [Sheet("Guns")]
    public List<GunData> guns;
    
    [Sheet("Upgrades")]
    public List<UpgradeData> upgrades;
    
    [Sheet("Monsters")]
    public List<MonsterData> monsters;
    
    [Sheet("Config")]
    public List<ConfigData> config;
}
```

- [ ] **Step 4: Unity Editor에서 컴파일 확인**

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/GameDataAsset.cs
git commit -m "feat: extend GunData with combat/evolution fields, add MonsterData to asset"
```

---

## Task 4: GameData에 총별 레벨/경험치 저장 추가

**Files:**
- Modify: `Assets/Scripts/Core/GameData.cs`

**Context:** 런타임에 각 총의 레벨과 경험치를 추적. SaveManager와 연동됨.

- [ ] **Step 1: 총별 레벨/경험치 데이터 구조 추가**

```csharp
using System;

/// <summary>
/// 총별 경험치/레벨 데이터
/// </summary>
[Serializable]
public class GunExperienceData
{
    public int GunId;
    public int Level;              // 현재 레벨 (1부터 시작)
    public int CurrentExp;         // 현재 경험치
    public int ExpToNextLevel;     // 다음 레벨까지 필요한 경험치
    
    public GunExperienceData(int gunId)
    {
        GunId = gunId;
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = CalculateExpForLevel(2);
    }
    
    private int CalculateExpForLevel(int level)
    {
        // 레벨업 필요 경험치: 레벨^2 * 100
        return level * level * 100;
    }
    
    public void AddExp(int exp)
    {
        CurrentExp += exp;
        while (CurrentExp >= ExpToNextLevel && Level < 100)
        {
            CurrentExp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = CalculateExpForLevel(Level + 1);
        }
    }
}
```

- [ ] **Step 2: GameData에 GunExperienceData 배열 추가**

```csharp
/// <summary>
/// 런타임 게임 상태 (DI Container로 관리)
/// </summary>
public class GameData
{
    public long TotalGold { get; set; }
    public int CurrentGunIndex { get; set; }
    public int[] ClickCounts { get; set; }
    public int[] UpgradeLevels { get; set; }
    
    // 새로운 필드: 총별 레벨/경험치 (8개 총용)
    public GunExperienceData[] GunExperiences { get; set; }
    
    // 현재 전투 중인 몬스터 상태
    public int CurrentMonsterId { get; set; }
    public int CurrentMonsterHP { get; set; }
    public int CurrentStage { get; set; }
    
    public GameData()
    {
        TotalGold = 0;
        CurrentGunIndex = 0;
        ClickCounts = new int[8];
        UpgradeLevels = new int[8];
        
        // 총별 경험치 데이터 초기화
        GunExperiences = new GunExperienceData[8];
        for (int i = 0; i < 8; i++)
        {
            GunExperiences[i] = new GunExperienceData(i);
        }
        
        CurrentMonsterId = 0;
        CurrentMonsterHP = 0;
        CurrentStage = 1;
    }
    
    public void Reset()
    {
        TotalGold = 0;
        CurrentGunIndex = 0;
        ClickCounts = new int[8];
        UpgradeLevels = new int[8];
        
        for (int i = 0; i < 8; i++)
        {
            GunExperiences[i] = new GunExperienceData(i);
        }
        
        CurrentMonsterId = 0;
        CurrentMonsterHP = 0;
        CurrentStage = 1;
    }
}
```

- [ ] **Step 3: 컴파일 확인**

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/GameData.cs
git commit -m "feat: add GunExperienceData and monster state to GameData"
```

---

## Task 5: CombatManager 구현

**Files:**
- Create: `Assets/Scripts/Core/CombatManager.cs`

**Context:** 전투 로직 중앙 관리. EventBus로 다른 시스템과 통신.

- [ ] **Step 1: CombatManager 클래스 작성**

```csharp
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
```

- [ ] **Step 2: 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/CombatManager.cs
git commit -m "feat: implement CombatManager with damage calculation and critical hits"
```

---

## Task 6: ExperienceSystem 구현

**Files:**
- Create: `Assets/Scripts/Core/ExperienceSystem.cs`

**Context:** 경험치 획득 및 레벨업 처리. 진화 조건 체크.

- [ ] **Step 1: ExperienceSystem 클래스 작성**

```csharp
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

// 레벨업 이벤트
public struct GunLevelUpEvent 
{ 
    public int GunId;
    public int NewLevel;
}

// 진화 체크 이벤트
public struct CheckEvolutionEvent 
{ 
    public int GunId;
    public int CurrentLevel;
}
```

- [ ] **Step 2: GunLevelUpEvent와 CheckEvolutionEvent를 Events.cs로 이동 (선택사항)**

- [ ] **Step 3: 컴파일 확인**

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/ExperienceSystem.cs
git commit -m "feat: implement ExperienceSystem with level up and evolution check"
```

---

## Task 7: EvolutionSystem 구현

**Files:**
- Create: `Assets/Scripts/Core/EvolutionSystem.cs`

**Context:** 진화 조건 체크 및 실행. 총 교체 로직.

- [ ] **Step 1: EvolutionSystem 클래스 작성**

```csharp
using DI;
using UnityEngine;

/// <summary>
/// 진화 시스템: 레벨 조건 충족 시 다음 총으로 진화
/// </summary>
public class EvolutionSystem
{
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    private SaveManager saveManager;
    
    private bool initialized = false;
    
    public void Initialize()
    {
        if (initialized) return;
        
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        saveManager = DIContainer.Resolve<SaveManager>();
        
        // 이벤트 구독
        EventBus<CheckEvolutionEvent>.Subscribe(OnCheckEvolution);
        
        initialized = true;
        Debug.Log("[EvolutionSystem] Initialized");
    }
    
    public void Dispose()
    {
        EventBus<CheckEvolutionEvent>.Unsubscribe(OnCheckEvolution);
    }
    
    /// <summary>
    /// 진화 조건 체크
    /// </summary>
    private void OnCheckEvolution(CheckEvolutionEvent e)
    {
        var gunData = gameDataAsset.guns[e.GunId];
        
        // 최종 형태인지 체크
        if (gunData.IsFinalForm || gunData.NextGunId < 0)
        {
            Debug.Log($"[EvolutionSystem] Gun {e.GunId} is already at final form");
            return;
        }
        
        // 진화 레벨에 도달했는지 체크
        if (e.CurrentLevel >= gunData.EvolveLevel)
        {
            EvolveGun(e.GunId, gunData.NextGunId);
        }
    }
    
    /// <summary>
    /// 총 진화 실행
    /// </summary>
    private void EvolveGun(int currentGunId, int nextGunId)
    {
        Debug.Log($"[EvolutionSystem] Evolving Gun {currentGunId} -> Gun {nextGunId}");
        
        // 다음 총 데이터 유효성 체크
        if (nextGunId < 0 || nextGunId >= gameDataAsset.guns.Count)
        {
            Debug.LogError($"[EvolutionSystem] Invalid next gun ID: {nextGunId}");
            return;
        }
        
        var nextGunData = gameDataAsset.guns[nextGunId];
        
        // 진화 이벤트 발행 (UI 이펙트용)
        EventBus<GunEvolvedEvent>.Publish(new GunEvolvedEvent 
        { 
            PreviousGunId = currentGunId,
            NewGunId = nextGunId,
            NewGunName = nextGunData.Name
        });
        
        // 현재 장착 총이 진화한 총이면 자동 교체
        if (gameData.CurrentGunIndex == currentGunId)
        {
            gameData.CurrentGunIndex = nextGunId;
            
            EventBus<GunSwitchedEvent>.Publish(new GunSwitchedEvent 
            { 
                GunId = nextGunId
            });
        }
        
        // 해금 처리 (진화된 총 해금)
        gameData.ClickCounts[nextGunId] = 1;
        
        // 저장
        saveManager.Save(gameData);
        
        Debug.Log($"[EvolutionSystem] Evolution complete! New gun: {nextGunData.Name}");
    }
    
    public bool CanEvolve(int gunId)
    {
        if (gunId < 0 || gunId >= gameDataAsset.guns.Count)
            return false;
        
        var gunData = gameDataAsset.guns[gunId];
        if (gunData.IsFinalForm || gunData.NextGunId < 0)
            return false;
        
        var gunExp = gameData.GunExperiences[gunId];
        return gunExp.Level >= gunData.EvolveLevel;
    }
}

// 진화 이벤트
public struct GunEvolvedEvent 
{ 
    public int PreviousGunId;
    public int NewGunId;
    public string NewGunName;
}
```

- [ ] **Step 2: GunEvolvedEvent를 Events.cs로 이동 (선택사항)**

- [ ] **Step 3: 컴파일 확인**

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/EvolutionSystem.cs
git commit -m "feat: implement EvolutionSystem with automatic gun evolution"
```

---

## Task 8: GlobalInstaller에 새 시스템 등록

**Files:**
- Modify: `Assets/Scripts/Installers/GlobalInstaller.cs`

**Context:** DI Container에 CombatManager, ExperienceSystem, EvolutionSystem 등록.

- [ ] **Step 1: 새 시스템을 DI Container에 등록**

```csharp
using DI;
using UnityEngine;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Static data (from Excel)
        var gameDataAsset = Resources.Load<GameDataAsset>("GameDataAsset");
        Bind(gameDataAsset);
        
        // Runtime state
        Bind(new GameData());
        
        // Services
        Bind(new SaveManager());
        
        // Core Managers
        Bind(new GameManager());
        Bind(new CombatManager());        // 전투 시스템
        Bind(new ExperienceSystem());     // 경험치 시스템
        Bind(new EvolutionSystem());      // 진화 시스템
        
        // Initialize all managers
        var gameManager = DIContainer.Resolve<GameManager>();
        gameManager.Initialize();
        
        var combatManager = DIContainer.Resolve<CombatManager>();
        combatManager.Initialize();
        
        var expSystem = DIContainer.Resolve<ExperienceSystem>();
        expSystem.Initialize();
        
        var evoSystem = DIContainer.Resolve<EvolutionSystem>();
        evoSystem.Initialize();
        
        Debug.Log("[GlobalInstaller] All services registered and initialized");
    }

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
    
    protected override void OnDestroy()
    {
        // Dispose in reverse order
        var evoSystem = DIContainer.Resolve<EvolutionSystem>();
        evoSystem?.Dispose();
        
        var expSystem = DIContainer.Resolve<ExperienceSystem>();
        expSystem?.Dispose();
        
        var combatManager = DIContainer.Resolve<CombatManager>();
        combatManager?.Dispose();
        
        var gameManager = DIContainer.Resolve<GameManager>();
        gameManager?.Dispose();
        
        base.OnDestroy();
    }
}
```

- [ ] **Step 2: 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Installers/GlobalInstaller.cs
git commit -m "feat: register CombatManager, ExperienceSystem, EvolutionSystem in DI"
```

---

## Task 9: ClickHandler를 전투 시스템에 연결

**Files:**
- Modify: `Assets/Scripts/UI/ClickHandler.cs`

**Context:** 클릭 시 AttackEvent 발행하도록 변경.

- [ ] **Step 1: 클릭 핸들러 수정 - AttackEvent 발행**

```csharp
using UnityEngine;

/// <summary>
/// 클릭 감지 및 AttackEvent 발행
/// </summary>
public class ClickHandler : MonoBehaviour
{
    [SerializeField] private bool useRaycast = false;
    
    private Camera mainCamera;
    private float lastAttackTime;
    private float attackCooldown = 0.1f; // 최소 공격 간격 (연사력 적용 가능)
    
    private void Start()
    {
        mainCamera = Camera.main;
        lastAttackTime = 0f;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 연사 쿨타임 체크
            if (Time.time - lastAttackTime < attackCooldown)
                return;
            
            if (useRaycast)
            {
                HandleRaycastClick();
            }
            else
            {
                // Simple: 화면 어디든 클릭하면 공격
                PublishAttack();
            }
        }
    }
    
    private void HandleRaycastClick()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                PublishAttack();
            }
        }
    }
    
    private void PublishAttack()
    {
        lastAttackTime = Time.time;
        
        // AttackEvent 발행 (CombatManager가 처리)
        EventBus<AttackEvent>.Publish(new AttackEvent 
        { 
            GunId = -1, // CombatManager가 현재 총 가져옴
            Damage = 0, // CombatManager가 계산
            IsCritical = false // CombatManager가 계산
        });
        
        // 기존 ClickEvent도 유지 (하위 호환성)
        EventBus<ClickEvent>.Publish(new ClickEvent());
    }
}
```

- [ ] **Step 2: 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/ClickHandler.cs
git commit -m "feat: connect ClickHandler to CombatManager via AttackEvent"
```

---

## Task 10: GameManager 수정 (레거시 로직 제거/통합)

**Files:**
- Modify: `Assets/Scripts/Core/GameManager.cs`

**Context:** 기존 클릭당 골드 로직을 전투 보상으로 변경. 진화는 EvolutionSystem으로 위임.

- [ ] **Step 1: GameManager 수정 - 전투 시스템과 통합**

```csharp
using DI;
using UnityEngine;

/// <summary>
/// 게임 상태 관리, 이벤트 처리 (Combat/Exp/Evolution 시스템과 통합)
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
            
            // TODO: GunExperiences, CurrentStage 등 새 필드도 로드
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
        
        int baseValue = gun.BaseDamage > 0 ? gun.BaseDamage : gun.ClickValue;
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
```

- [ ] **Step 2: 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/GameManager.cs
git commit -m "refactor: integrate GameManager with Combat/Exp/Evolution systems"
```

---

## Task 11: UIManager 업데이트 (전투 UI 추가)

**Files:**
- Modify: `Assets/Scripts/UI/UIManager.cs`

**Context:** 몬스터 HP바, 경험치 바, 전투 정보 표시.

- [ ] **Step 1: 전투 관련 UI 이벤트 구독 추가**

```csharp
using DI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 전체 관리, 전투/진화 UI 추가
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text currentGunText;
    
    [Header("Gun Display")]
    [SerializeField] private Image gunImage;
    [SerializeField] private ClickHandler clickHandler;
    
    [Header("Monster UI")]
    [SerializeField] private GameObject monsterPanel;
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private Slider monsterHPSlider;
    [SerializeField] private TMP_Text monsterHPText;
    
    [Header("Experience UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    
    [Header("Tabs")]
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button collectionTabButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject collectionPanel;
    
    private GameManager gameManager;
    private GameDataAsset gameDataAsset;
    private GameData gameData;
    private CombatManager combatManager;
    
    private void Start()
    {
        // DI 주입
        gameManager = DIContainer.Resolve<GameManager>();
        gameDataAsset = DIContainer.Resolve<GameDataAsset>();
        gameData = DIContainer.Resolve<GameData>();
        combatManager = DIContainer.Resolve<CombatManager>();
        
        // 이벤트 구독
        EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Subscribe(OnGunSwitched);
        EventBus<GunUnlockedEvent>.Subscribe(OnGunUnlocked);
        EventBus<GameInitializedEvent>.Subscribe(OnGameInitialized);
        
        // 전투 이벤트 구독
        EventBus<MonsterSpawnedEvent>.Subscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Subscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Subscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Subscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Subscribe(OnGunEvolved);
        
        // 버튼 이벤트
        shopTabButton.onClick.AddListener(() => SwitchTab("shop"));
        collectionTabButton.onClick.AddListener(() => SwitchTab("collection"));
        
        // 기본 탭
        SwitchTab("shop");
    }
    
    private void OnDestroy()
    {
        EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
        EventBus<GunSwitchedEvent>.Unsubscribe(OnGunSwitched);
        EventBus<GunUnlockedEvent>.Unsubscribe(OnGunUnlocked);
        EventBus<GameInitializedEvent>.Unsubscribe(OnGameInitialized);
        
        EventBus<MonsterSpawnedEvent>.Unsubscribe(OnMonsterSpawned);
        EventBus<MonsterHitEvent>.Unsubscribe(OnMonsterHit);
        EventBus<MonsterKilledEvent>.Unsubscribe(OnMonsterKilled);
        EventBus<GunLevelUpEvent>.Unsubscribe(OnGunLevelUp);
        EventBus<GunEvolvedEvent>.Unsubscribe(OnGunEvolved);
    }
    
    private void OnGameInitialized(GameInitializedEvent e)
    {
        UpdateMoneyDisplay(gameManager.GetTotalGold());
        UpdateGunDisplay(gameManager.GetCurrentGunIndex());
        UpdateExpDisplay();
    }
    
    private void OnMoneyChanged(MoneyChangedEvent e)
    {
        UpdateMoneyDisplay(e.Amount);
    }
    
    private void OnGunSwitched(GunSwitchedEvent e)
    {
        UpdateGunDisplay(e.GunId);
        UpdateExpDisplay();
    }
    
    private void OnGunUnlocked(GunUnlockedEvent e)
    {
        Debug.Log($"[UIManager] Gun unlocked: {gameDataAsset.guns[e.GunId].Name}");
    }
    
    // 전투 UI 업데이트
    private void OnMonsterSpawned(MonsterSpawnedEvent e)
    {
        if (monsterNameText != null)
            monsterNameText.text = e.MonsterName;
        
        UpdateMonsterHP(e.MaxHP, e.MaxHP);
    }
    
    private void OnMonsterHit(MonsterHitEvent e)
    {
        int maxHP = combatManager.GetCurrentMonsterMaxHP();
        UpdateMonsterHP(e.CurrentHP, maxHP);
        
        // 크리티컬 이펙트
        if (e.IsCritical)
        {
            Debug.Log($"[UIManager] CRITICAL! Damage: {e.Damage}");
            // TODO: 크리티컬 이펙트 연출
        }
    }
    
    private void OnMonsterKilled(MonsterKilledEvent e)
    {
        Debug.Log($"[UIManager] Monster killed! +{e.ExpReward} EXP, +{e.GoldReward} Gold");
        UpdateExpDisplay();
    }
    
    private void OnGunLevelUp(GunLevelUpEvent e)
    {
        Debug.Log($"[UIManager] Level Up! Gun {e.GunId} is now Lv.{e.NewLevel}");
        UpdateExpDisplay();
        // TODO: 레벨업 이펙트 연출
    }
    
    private void OnGunEvolved(GunEvolvedEvent e)
    {
        Debug.Log($"[UIManager] EVOLUTION! {gameDataAsset.guns[e.PreviousGunId].Name} -> {e.NewGunName}");
        UpdateGunDisplay(e.NewGunId);
        UpdateExpDisplay();
        // TODO: 진화 이펙트 연출
    }
    
    private void UpdateMoneyDisplay(long amount)
    {
        if (moneyText != null)
            moneyText.text = $"${FormatNumber(amount)}";
    }
    
    private void UpdateGunDisplay(int gunId)
    {
        var gun = gameDataAsset.guns[gunId];
        if (currentGunText != null)
            currentGunText.text = gun.Name;
        
        // 스프라이트 로드 (Resources 폴더에서)
        // var sprite = Resources.Load<Sprite>($"Guns/{gun.SpriteName}");
        // if (sprite != null) gunImage.sprite = sprite;
    }
    
    private void UpdateMonsterHP(int currentHP, int maxHP)
    {
        if (monsterHPSlider != null)
        {
            monsterHPSlider.maxValue = maxHP;
            monsterHPSlider.value = currentHP;
        }
        
        if (monsterHPText != null)
            monsterHPText.text = $"{currentHP}/{maxHP}";
    }
    
    private void UpdateExpDisplay()
    {
        var gunExp = gameData.GunExperiences[gameData.CurrentGunIndex];
        
        if (levelText != null)
            levelText.text = $"Lv.{gunExp.Level}";
        
        if (expSlider != null)
        {
            expSlider.maxValue = gunExp.ExpToNextLevel;
            expSlider.value = gunExp.CurrentExp;
        }
        
        if (expText != null)
            expText.text = $"{gunExp.CurrentExp}/{gunExp.ExpToNextLevel}";
    }
    
    private void SwitchTab(string tabName)
    {
        shopPanel.SetActive(tabName == "shop");
        collectionPanel.SetActive(tabName == "collection");
    }
    
    private string FormatNumber(long num)
    {
        if (num >= 1000000) return $"{num / 1000000f:F1}M";
        if (num >= 1000) return $"{num / 1000f:F1}K";
        return num.ToString();
    }
}
```

- [ ] **Step 2: 컴파일 확인**

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/UIManager.cs
git commit -m "feat: add combat and evolution UI to UIManager"
```

---

## Task 12: SaveManager 업데이트 (새 데이터 저장)

**Files:**
- Modify: `Assets/Scripts/Core/SaveManager.cs`

**Context:** GunExperiences, CurrentStage 등 새 필드 저장/로드.

- [ ] **Step 1: SaveData 구조체 확장**

```csharp
using System;
using UnityEngine;

/// <summary>
/// 저장용 데이터 구조체
/// </summary>
[Serializable]
public class SaveData
{
    public long TotalGold;
    public int CurrentGunIndex;
    public int[] ClickCounts;
    public int[] UpgradeLevels;
    
    // 새로운 필드
    public GunExperienceSaveData[] GunExperiences;
    public int CurrentStage;
    public int CurrentMonsterId;
    public int CurrentMonsterHP;
}

/// <summary>
/// 총 경험치 저장용 데이터
/// </summary>
[Serializable]
public class GunExperienceSaveData
{
    public int GunId;
    public int Level;
    public int CurrentExp;
    public int ExpToNextLevel;
}
```

- [ ] **Step 2: SaveManager 수정**

```csharp
using UnityEngine;

/// <summary>
/// 게임 데이터 저장/로드
/// </summary>
public class SaveManager
{
    private const string SAVE_KEY = "GunClicker_SaveData";
    
    public void Save(GameData gameData)
    {
        var saveData = new SaveData
        {
            TotalGold = gameData.TotalGold,
            CurrentGunIndex = gameData.CurrentGunIndex,
            ClickCounts = gameData.ClickCounts,
            UpgradeLevels = gameData.UpgradeLevels,
            CurrentStage = gameData.CurrentStage,
            CurrentMonsterId = gameData.CurrentMonsterId,
            CurrentMonsterHP = gameData.CurrentMonsterHP
        };
        
        // GunExperiences 변환
        saveData.GunExperiences = new GunExperienceSaveData[gameData.GunExperiences.Length];
        for (int i = 0; i < gameData.GunExperiences.Length; i++)
        {
            var exp = gameData.GunExperiences[i];
            saveData.GunExperiences[i] = new GunExperienceSaveData
            {
                GunId = exp.GunId,
                Level = exp.Level,
                CurrentExp = exp.CurrentExp,
                ExpToNextLevel = exp.ExpToNextLevel
            };
        }
        
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("[SaveManager] Game saved");
    }
    
    public SaveData Load()
    {
        if (!HasSaveData())
        {
            return null;
        }
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        var saveData = JsonUtility.FromJson<SaveData>(json);
        
        Debug.Log("[SaveManager] Game loaded");
        return saveData;
    }
    
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        Debug.Log("[SaveManager] Save deleted");
    }
}
```

- [ ] **Step 3: 컴파일 확인**

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/SaveManager.cs
git commit -m "feat: extend SaveManager to save GunExperiences and combat state"
```

---

## Summary

모든 작업이 완료되면:

1. ✅ **CombatManager** - 전투 로직, 데미지 계산, 크리티컬
2. ✅ **ExperienceSystem** - 경험치/레벨업 관리
3. ✅ **EvolutionSystem** - 진화 조건 체크 및 실행
4. ✅ **DI 통합** - GlobalInstaller에 모든 시스템 등록
5. ✅ **EventBus 연결** - UI ↔ 시스템 간 통신
6. ✅ **데이터 저장** - GunExperiences, 전투 상태 저장

다음 단계:
- Unity Editor에서 Inspector 설정 (Slider, Text 등 연결)
- Excel 데이터 작성 (Monsters 시트, GunData 확장)
- 테스트 및 밸런스 조정
