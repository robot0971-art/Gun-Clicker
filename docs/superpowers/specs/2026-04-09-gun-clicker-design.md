# Gun Clicker - Design Document

## Overview

A classic clicker game where players click on guns to earn money, unlock better guns, and upgrade their firepower. Built with Unity using the GunsPack asset sprites.

## Design Principles

### SOLID
- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Extend via events, not by modifying core classes
- **Liskov Substitution**: Interfaces for dependencies (IGameManager, ISaveManager)
- **Interface Segregation**: Small, specific interfaces
- **Dependency Inversion**: DI Container manages dependencies

### KISS (Keep It Simple, Stupid)
- No over-engineering
- Simple click mechanics without auto-fire
- Direct event-based communication
- Minimal abstraction layers

### YAGNI (You Ain't Gonna Need It)
- No unnecessary features
- Only 8 guns (no infinite collection)
- Manual clicking only (no auto-clicker)
- Essential UI only (Shop, Collection)

---

## Architecture

### Dependency Management

**DI Container**: Manages all singleton dependencies
```csharp
// GlobalInstaller registers core services
Bind<GameDataAsset>();       // Static data from Excel
Bind<GameManager>();         // Runtime state
Bind<SaveManager>();         // Persistence
Bind<UIManager>();           // UI controller

// Usage in classes
[Inject] private GameManager gameManager;
[Inject] private GameDataAsset gameData;
```

**EventBus**: Decoupled event communication
```csharp
// Define events as structs
public struct MoneyChangedEvent { public long Amount; }
public struct GunUnlockedEvent { public int GunId; }
public struct GunSwitchedEvent { public int GunId; }

// Subscribe
EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);

// Publish
EventBus<MoneyChangedEvent>.Publish(new MoneyChangedEvent { Amount = 100 });
```

### Component Communication Flow
```
ClickHandler → EventBus<ClickEvent> → GameManager
GameManager → EventBus<MoneyChangedEvent> → UIManager
GameManager → EventBus<GunUnlockedEvent> → UIManager + Effects
```

---

## Core Mechanics

### Click-to-Earn System
- Player clicks the gun sprite in the center of the screen
- Each click earns money based on current gun's power
- No auto-fire mechanic - pure manual clicking for simplicity

### Gun Collection System (8 Guns)

| Order | Gun | Click Value | Unlock Requirement |
|-------|-----|-------------|-------------------|
| 1 | Revolver | $1 | Starting gun (free) |
| 2 | M92 | $2 | 100 clicks |
| 3 | Luger | $4 | 200 clicks |
| 4 | SawedOffShotgun | $7 | 400 clicks |
| 5 | MP5 | $12 | 800 clicks |
| 6 | M15 | $20 | 1,600 clicks |
| 7 | AK47 | $35 | 3,200 clicks |
| 8 | M24 | $60 | 6,400 clicks |

### Level-Based Unlock System
- Each gun has a click counter
- When current gun reaches required click count, next gun unlocks
- Player can switch between unlocked guns
- Locked guns show as silhouettes with unlock requirements

---

## Events

| Event | Publisher | Subscriber |
|-------|-----------|------------|
| ClickEvent | ClickHandler | GameManager |
| MoneyChangedEvent | GameManager | UIManager |
| GunUnlockedEvent | GameManager | UIManager, CollectionPanel |
| GunSwitchedEvent | CollectionPanel | GameManager, UIManager |
| UpgradePurchasedEvent | ShopPanel | GameManager |

```csharp
// Event definitions (Events.cs)
public struct ClickEvent { }
public struct MoneyChangedEvent { public long Amount; public long Delta; }
public struct GunUnlockedEvent { public int GunId; }
public struct GunSwitchedEvent { public int GunId; }
public struct UpgradePurchasedEvent { public int GunId; public int Level; }
```

---

## UI Layout

### Screen Structure
```
+------------------------------------------+
|  Money: $1,234    Current Gun: Revolver   |
+------------------------------------------+
|                                          |
|                                          |
|              [GUN SPRITE]                |
|              Click Here                  |
|                                          |
|                                          |
+------------------------------------------+
|   [Shop Tab]    [Collection Tab]         |
+------------------------------------------+
```

### Shop Tab
- Upgrade current gun's click value (multiplier)
- Visual bullet animation on click
- Upgrade costs scale with current power

### Collection Tab
- Grid of all 8 guns
- Unlocked: full color sprite, selectable
- Locked: silhouette + click requirement shown
- Current gun highlighted

---

## Visual Effects

### Click Feedback
- Gun scale animation (bounce effect)
- Bullet sprite flies out from gun
- Floating text showing money earned (+$1)
- Particle effect on click location

### Unlock Animation
- Dramatic reveal when new gun unlocks
- Sound effect + screen flash
- Notification popup

---

## Technical Architecture

### Core Components (DI-managed)

| Component | Responsibility | Dependencies |
|-----------|---------------|--------------|
| GameManager | Game state, money, unlocks | GameDataAsset, SaveManager |
| SaveManager | Load/Save persistence | - |
| UIManager | UI display, animations | GameManager, GameDataAsset |
| ClickHandler | Click detection | - (publishes events) |

### Data Components

| Component | Type | Source |
|-----------|------|--------|
| GameDataAsset | ScriptableObject | Excel Converter |
| GameData | MonoBehaviour | Runtime state (DI) |

### Data Flow
```
Excel (GameData.xlsx) → ExcelConverter → GameDataAsset.asset
GameDataAsset → DI Container → GameManager/UIManager
GameManager → EventBus → UIManager
PlayerPrefs → SaveManager → GameManager
```

### Data Persistence
- PlayerPrefs for save data
- Save: total money, current gun index, click counts per gun, upgrade levels

### Asset Integration
- GunsPack/Guns/*.png → Gun sprites
- GunsPack/Bullets/*.png → Click effect particles

---

## File Structure

```
Assets/
├── Scripts/
│   ├── DI/
│   │   └── DIContainer.cs          # DI system
│   ├── ExcelConverter/
│   │   └── ExcelConverter.cs       # Excel → SO conversion
│   ├── Installers/
│   │   ├── GlobalInstaller.cs      # Core services DI
│   │   └── GameInstaller.cs        # Scene-specific DI
│   ├── Core/
│   │   ├── GameManager.cs          # State management
│   │   └── SaveManager.cs          # Persistence
│   │   └── GameData.cs             # Runtime state
│   ├── Events/
│   │   ├── EventBus.cs             # Event system
│   │   └── Events.cs               # Event definitions
│   ├── UI/
│   │   ├── UIManager.cs            # UI controller
│   │   ├── ShopPanel.cs            # Upgrade shop
│   │   └── CollectionPanel.cs      # Gun collection
│   │   └── ClickHandler.cs         # Click detection
│   └── Effects/
│       ├── ClickEffect.cs          # Click feedback
│       └── UnlockEffect.cs         # Unlock animation
│   ├── GameDataAsset.cs            # Static data SO
│   └── EventBus.cs                 # Generic event bus
├── StreamingAssets/
│   └── GameData.xlsx               # Excel data source
├── Resources/
│   └── GameDataAsset.asset         # Converted data
└── Prefabs/
    ├── GunButton.prefab
    ├── ShopUpgradeItem.prefab
    └── CollectionSlot.prefab
```

---

## Success Criteria

1. Player can click gun to earn money
2. All 8 guns display correctly with GunsPack sprites
3. Guns unlock at correct click thresholds
4. Player can switch between unlocked guns
5. Money and progress persist between sessions
6. Satisfying click feedback with bullet effects
7. Clear UI showing money, current gun, unlock progress
8. DI Container properly manages dependencies
9. EventBus enables decoupled communication
10. SOLID/KISS/YAGNI principles followed