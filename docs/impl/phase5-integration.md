# Phase 5: Integration & Testing

## Overview
전체 통합, Prefab 생성, 최종 테스트

## Dependencies
- Phase 1-4 완료
- GunsPack Sprites import

---

## Prefabs to Create

```
Assets/Prefabs/
├── FloatingText.prefab
├── UnlockPopup.prefab
├── GunSlot.prefab (for CollectionPanel)
└── UpgradeItem.prefab (for ShopPanel)
```

---

## Scene Setup

### Main Scene Structure

```
MainScene
├── GlobalInstaller (GameObject)
│   ├── GlobalInstaller component
│   └── GameData component
│   └── GameManager component (runtime)
│   └── SaveManager component (runtime)
│
├── Canvas (UI)
│   ├── UIManager component
│   ├── TopBar
│   │   ├── MoneyText
│   │   └── CurrentGunText
│   ├── GunDisplay
│   │   ├── GunSprite (Image)
│   │   └── ClickHandler component
│   │   └── ClickEffect component
│   ├── TabBar
│   │   ├── ShopTabButton
│   │   └── CollectionTabButton
│   ├── ShopPanel
│   │   └── ShopPanel component
│   └── CollectionPanel
│       └── CollectionPanel component
│       └── GunSlotGrid
│
├── Effects
│   └── ClickEffect component
│   └── UnlockEffect component
│
└── Audio
    └── AudioSource (for unlock sound)
```

---

## Script Execution Order

Unity Edit > Project Settings > Script Execution Order:

| Script | Order | Reason |
|--------|-------|--------|
| GlobalInstaller | -100 | DI Container 초기화 |
| GameManager | -50 | 이벤트 구독 시작 |
| UIManager | -40 | UI 초기화 |
| Other scripts | Default | DI 주입 후 실행 |

---

## Final Testing Checklist

### Core System
- [ ] DI Container 정상 등록
- [ ] GameManager 초기화
- [ ] SaveManager 저장/로드

### Click System
- [ ] 클릭 감지
- [ ] 골드 증가
- [ ] 이벤트 플로우 정상

### UI System
- [ ] 골드 표시 업데이트
- [ ] 총 변경 UI 업데이트
- [ ] 업그레이드 구매
- [ ] 총 선택
- [ ] 탭 전환

### Effects
- [ ] 클릭 이펙트
- [ ] 해금 이펙트
- [ ] Floating text

### Data
- [ ] GameDataAsset 로드
- [ ] Excel → SO 변환 정상
- [ ] 8개 총 데이터 확인

### Persistence
- [ ] 게임 종료 후 데이터 저장
- [ ] 게임 시작 시 데이터 로드
- [ ] 골드, 총 인덱스, 업그레이드 레벨 저장

### Edge Cases
- [ ] 첫 실행 (데이터 없음)
- [ ] 모든 총 해금
- [ ] 최대 업그레이드 레벨
- [ ] 골드 부족 시 업그레이드 불가

---

## Build Checklist

- [ ] 모든 스크립트 컴파일 에러 없음
- [ ] 모든 Prefab 연결
- [ ] Scene 저장
- [ ] GameDataAsset.asset 생성
- [ ] GunsPack Sprites import
- [ ] Build & Run 테스트