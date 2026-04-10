# Phase 3: UI System Implementation

## Overview
UI 시스템 구현 - UIManager, ShopPanel, CollectionPanel

## Dependencies
- EventBus (완료)
- Core System (Phase 1)
- GameDataAsset (완료)

---

## Files to Create

```
Assets/Scripts/UI/
├── UIManager.cs
├── ShopPanel.cs
└── CollectionPanel.cs
```

---

## 1. UIManager

**Purpose:** UI 전체 관리, 이벤트 수신하여 UI 업데이트

**Dependencies:**
- [Inject] GameManager gameManager
- [Inject] GameDataAsset gameDataAsset

**Events to Subscribe:**
- MoneyChangedEvent → Update money display
- GunUnlockedEvent → Show unlock animation
- GunSwitchedEvent → Update gun display
- GameInitializedEvent → Initialize UI

**UI Elements:**
- MoneyText (TMP)
- CurrentGunText
- GunSprite (center)
- ShopTabButton
- CollectionTabButton

**Methods:**
```csharp
void UpdateMoneyDisplay(long amount);
void UpdateGunDisplay(int gunId);
void ShowUnlockAnimation(int gunId);
void SwitchTab(string tabName);
```

---

## 2. ShopPanel

**Purpose:** 업그레이드 상점 UI

**Dependencies:**
- [Inject] GameManager gameManager
- [Inject] GameDataAsset gameDataAsset
- [Inject] GameData gameData (runtime state)

**UI Elements:**
- UpgradeLevelText
- CostText
- BuyButton
- CurrentValueText

**Events to Publish:**
- UpgradePurchasedEvent

**Events to Subscribe:**
- MoneyChangedEvent → Update affordability
- GunSwitchedEvent → Update shop content

**Methods:**
```csharp
void UpdateDisplay(int gunId);
void OnBuyButtonClick();
bool CanAffordUpgrade();
```

---

## 3. CollectionPanel

**Purpose:** 총 컬렉션 UI (8개 총 표시)

**Dependencies:**
- [Inject] GameManager gameManager
- [Inject] GameDataAsset gameDataAsset
- [Inject] GameData gameData (runtime state)

**UI Elements:**
- GunSlotGrid (8 slots)
- Each slot: GunSprite, NameText, LockOverlay, UnlockRequirement

**Events to Publish:**
- GunSwitchedEvent

**Events to Subscribe:**
- GunUnlockedEvent → Update slot unlock state
- GunSwitchedEvent → Highlight current gun
- MoneyChangedEvent → Update unlock progress

**Methods:**
```csharp
void InitializeSlots();
void UpdateSlot(int gunId);
void OnSlotClick(int gunId);
```

---

## UI Layout (Unity Scene)

```
Canvas
├── TopBar
│   ├── MoneyText
│   └── CurrentGunText
├── GunDisplay
│   └── GunSprite
│   └── ClickHandler (Phase 2)
├── TabBar
│   ├── ShopTabButton
│   └── CollectionTabButton
├── ShopPanel (inactive by default)
│   ├── UpgradeLevelText
│   ├── CostText
│   ├── BuyButton
│   └── CurrentValueText
└── CollectionPanel (inactive by default)
    └── GunSlotGrid
        ├── Slot1 (Revolver)
        ├── Slot2 (M92)
        ├── ...
        └── Slot8 (M24)
```

---

## Implementation Order

1. UIManager.cs - basic structure
2. ShopPanel.cs - upgrade UI
3. CollectionPanel.cs - gun slots
4. Create Unity UI GameObjects
5. Connect components
6. Test UI updates via events

---

## Testing Checklist

- [ ] UIManager 컴포넌트 생성
- [ ] MoneyChangedEvent → UI 업데이트 테스트
- [ ] GunSwitchedEvent → UI 업데이트 테스트
- [ ] ShopPanel 업그레이드 구매 테스트
- [ ] CollectionPanel 총 선택 테스트
- [ ] 탭 전환 테스트