# Phase 1: Core System Implementation

## Overview
게임의 코어 시스템 구현 - GameManager, SaveManager, GameData (런타임 상태)

## Dependencies
- DI Container (완료)
- EventBus (완료)
- GameDataAsset (완료)

---

## Files to Create

```
Assets/Scripts/Core/
├── GameManager.cs
├── SaveManager.cs
└── GameData.cs (runtime state - DI용)
```

---

## 1. GameData (Runtime State)

**Purpose:** DI Container로 관리되는 런타임 상태

**Fields:**
```csharp
public long TotalGold { get; set; }
public int CurrentGunIndex { get; set; }
public int[] ClickCounts { get; set; }  // 8개 총의 클릭 수
public int[] UpgradeLevels { get; set; } // 8개 총의 업그레이드 레벨
```

---

## 2. SaveManager

**Purpose:** PlayerPrefs로 저장/로드

**Methods:**
```csharp
void Save(GameData data);
GameData Load();
void Clear();
```

**Save Data Format:**
- TotalGold: PlayerPrefs key "totalGold"
- CurrentGunIndex: "currentGun"
- ClickCounts: "clickCounts" (JSON array)
- UpgradeLevels: "upgradeLevels" (JSON array)

---

## 3. GameManager

**Purpose:** 게임 상태 관리, 이벤트 처리

**Dependencies:**
- [Inject] GameDataAsset gameDataAsset
- [Inject] SaveManager saveManager

**Responsibilities:**
1. Initialize game state on start
2. Handle ClickEvent → add money
3. Handle GunSwitchedEvent → change gun
4. Handle UpgradePurchasedEvent → upgrade
5. Check unlock conditions
6. Publish state change events

**Events to Subscribe:**
- ClickEvent
- GunSwitchedEvent
- UpgradePurchasedEvent

**Events to Publish:**
- MoneyChangedEvent
- GunUnlockedEvent
- GameInitializedEvent

**Key Methods:**
```csharp
void Initialize();
void OnClick();
void SwitchGun(int gunId);
void PurchaseUpgrade(int gunId);
bool CanUnlockGun(int gunId);
bool CanPurchaseUpgrade(int gunId);
int CalculateClickValue(int gunId);
int CalculateUpgradeCost(int gunId);
```

---

## 4. GlobalInstaller Update

**Purpose:** Core 서비스 DI 등록

```csharp
public override void InstallBindings() {
    Bind(Resources.Load<GameDataAsset>("GameDataAsset"));
    Bind(new GameData());
    Bind(new SaveManager());
    Bind(new GameManager());
}
```

---

## Implementation Order

1. GameData.cs (runtime state)
2. SaveManager.cs
3. GameManager.cs
4. GlobalInstaller.cs update
5. Test in Unity Editor

---

## Testing Checklist

- [ ] DI Container에 GameDataAsset 등록
- [ ] DI Container에 GameData 등록
- [ ] DI Container에 SaveManager 등록
- [ ] DI Container에 GameManager 등록
- [ ] SaveManager 저장/로드 테스트
- [ ] GameManager 초기화 테스트
- [ ] ClickEvent → MoneyChangedEvent 테스트