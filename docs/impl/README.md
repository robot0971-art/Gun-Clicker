# Gun Clicker - Implementation Overview

## Total Phases: 5

| Phase | Focus | Files | Estimated Time |
|-------|-------|-------|----------------|
| 1 | Core System | GameManager, SaveManager, GameData | ~2h |
| 2 | Click System | ClickHandler | ~30m |
| 3 | UI System | UIManager, ShopPanel, CollectionPanel | ~3h |
| 4 | Effects | ClickEffect, UnlockEffect | ~1h |
| 5 | Integration | Prefabs, Scene setup, Testing | ~2h |

**Total Estimated: ~8-10 hours**

---

## Architecture Summary

### DI Container
- GlobalInstaller: Core services 등록
- [Inject] 어트리뷰트로 의존성 주입

### EventBus
- 이벤트 기반 컴포넌트 통신
- Decoupled architecture

### Data Flow
```
Excel → ExcelConverter → GameDataAsset (ScriptableObject)
GameDataAsset → DI → GameManager
Click → EventBus → GameManager → MoneyChanged → UI
```

---

## File Structure (Final)

```
Assets/
├── Scripts/
│   ├── DI/
│   │   └── DIContainer.cs ✓
│   ├── ExcelConverter/
│   │   ├── Attributes.cs ✓
│   │   ├── ExcelConverter.cs ✓
│   │   └── Editor/
│   │       └── ExcelConverterEditor.cs ✓
│   ├── Installers/
│   │   ├── GlobalInstaller.cs ✓
│   │   └── GameInstaller.cs ✓
│   ├── Core/
│   │   ├── GameManager.cs (Phase 1)
│   │   ├── SaveManager.cs (Phase 1)
│   │   └── GameData.cs (Phase 1)
│   ├── Events/
│   │   ├── EventBus.cs ✓
│   │   └── Events.cs ✓
│   ├── UI/
│   │   ├── UIManager.cs (Phase 3)
│   │   ├── ShopPanel.cs (Phase 3)
│   │   ├── CollectionPanel.cs (Phase 3)
│   │   └── ClickHandler.cs (Phase 2)
│   ├── Effects/
│   │   ├── ClickEffect.cs (Phase 4)
│   │   └── UnlockEffect.cs (Phase 4)
│   ├── GameDataAsset.cs ✓
│   └── EventBus.cs ✓
├── StreamingAssets/
│   └── GameData.xlsx ✓
├── Resources/
│   └── GameDataAsset.asset ✓
├── Plugins/
│   ├── ExcelDataReader.dll ✓
│   └── ExcelDataReader.DataSet.dll ✓
└── Prefabs/
    ├── FloatingText.prefab (Phase 5)
    ├── UnlockPopup.prefab (Phase 5)
    ├── GunSlot.prefab (Phase 5)
    └── UpgradeItem.prefab (Phase 5)

docs/
├── impl/
│   ├── phase1-core-system.md ✓
│   ├── phase2-click-system.md ✓
│   ├── phase3-ui-system.md ✓
│   ├── phase4-effects.md ✓
│   ├── phase5-integration.md ✓
│   └── README.md (this file)
└── superpowers/
    └── specs/
        └── 2026-04-09-gun-clicker-design.md ✓
```

---

## Implementation Order

1. **Phase 1**: Core System
   - GameData, SaveManager, GameManager
   - DI 등록
   
2. **Phase 2**: Click System
   - ClickHandler
   - 이벤트 플로우 테스트

3. **Phase 3**: UI System
   - UIManager, ShopPanel, CollectionPanel
   - Scene UI 구성

4. **Phase 4**: Effects
   - ClickEffect, UnlockEffect
   - Prefabs 생성

5. **Phase 5**: Integration
   - 전체 테스트
   - Build

---

## Key Design Decisions

### DI vs Singleton
- DI Container 사용 (유연성, 테스트 용이성)
- Singleton 제거

### EventBus vs Direct Reference
- EventBus 사용 (decoupled)
- 직접 참조 최소화

### ScriptableObject vs MonoBehaviour
- 정적 데이터: ScriptableObject (GameDataAsset)
- 런타임 상태: DI Container (GameData)

### PlayerPrefs vs JSON File
- PlayerPrefs (단순, KISS)
- JSON 파일 불필요

---

## SOLID/KISS/YAGNI Applied

### SOLID
- S: Each class has one responsibility
- O: Extend via events, not modification
- L: Interfaces for dependencies
- I: Small interfaces
- D: DI Container manages dependencies

### KISS
- Simple click mechanics
- PlayerPrefs for save
- No over-engineering

### YAGNI
- No auto-clicker
- No infinite guns
- Minimal features